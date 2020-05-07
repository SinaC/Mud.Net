using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Mud.Container;
using Mud.DataStructures.Trie;
using Mud.Domain;
using Mud.Logger;
using Mud.Server.Abilities;
using Mud.Server.Aura;
using Mud.Server.Common;
using Mud.Server.Entity;
using Mud.Server.Helpers;
using Mud.Server.Input;
using Mud.Server.Item;

namespace Mud.Server.Character
{
    public abstract partial class CharacterBase : EntityBase, ICharacter
    {
        private const int MinAlignment = -1000;
        private const int MaxAlignment = 1000;

        private static readonly Lazy<IReadOnlyTrie<CommandMethodInfo>> CharacterBaseCommands = new Lazy<IReadOnlyTrie<CommandMethodInfo>>(GetCommands<CharacterBase>);

        private readonly List<IItem> _inventory;
        private readonly List<EquipedItem> _equipments;
        // TODO: replace int[] with Dictionary<enum,int> ?
        private readonly int[] _baseAttributes;
        private readonly int[] _currentAttributes;
        private readonly int[] _maxResources;
        private readonly int[] _currentResources;
        private readonly Dictionary<IAbility, DateTime> _cooldowns; // Key: ability.Id, Value: Next ability availability
        private readonly List<KnownAbility> _knownAbilities;

        protected ITimeHandler TimeHandler => DependencyContainer.Current.GetInstance<ITimeHandler>();
        protected IRandomManager RandomManager => DependencyContainer.Current.GetInstance<IRandomManager>();
        protected ITableValues TableValues => DependencyContainer.Current.GetInstance<ITableValues>();

        protected int MaxHitPoints => _currentAttributes[(int) CharacterAttributes.MaxHitPoints];
        protected int MaxMovePoints => _currentAttributes[(int)CharacterAttributes.MaxMovePoints];

        protected CharacterBase(Guid guid, string name, string description)
            : base(guid, name, description)
        {
            _inventory = new List<IItem>();
            _equipments = new List<EquipedItem>();
            _baseAttributes = new int[EnumHelpers.GetCount<CharacterAttributes>()];
            _currentAttributes = new int[EnumHelpers.GetCount<CharacterAttributes>()];
            _maxResources = new int[EnumHelpers.GetCount<ResourceKinds>()];
            _currentResources = new int[EnumHelpers.GetCount<ResourceKinds>()];
            _cooldowns = new Dictionary<IAbility, DateTime>(new CompareIAbility());
            _knownAbilities = new List<KnownAbility>(); // handled by RecomputeKnownAbilities

            Position = Positions.Standing;
            Form = Forms.Normal;
        }

        #region ICharacter

        #region IEntity

        #region IActor

        public override IReadOnlyTrie<CommandMethodInfo> Commands => CharacterBaseCommands.Value;

        public override void Send(string message, bool addTrailingNewLine)
        {
            // TODO: use Act formatter ?
            base.Send(message, addTrailingNewLine);
            // TODO: do we really need to receive message sent to slave ?
            if (Settings.ForwardSlaveMessages && ControlledBy != null)
            {
                if (Settings.PrefixForwardedMessages)
                    message = "<CTRL|" + DisplayName + ">" + message;
                ControlledBy.Send(message, addTrailingNewLine);
            }
        }

        public override void Page(StringBuilder text)
        {
            base.Page(text);
            if (Settings.ForwardSlaveMessages && ControlledBy != null)
                ControlledBy.Page(text);
        }

        #endregion

        // TODO: override RelativeDescription ?

        public override bool ChangeIncarnation(IAdmin admin)
        {
            bool result = base.ChangeIncarnation(admin);
            if (result)
            {
                RecomputeKnownAbilities();
                Recompute();
            }
            return result;
        }

        #endregion

        #region IContainer

        public IEnumerable<IItem> Content => _inventory.Where(x => x.IsValid);

        public bool PutInContainer(IItem obj)
        {
            // TODO: check if already in a container
            _inventory.Insert(0, obj);
            return true;
        }

        public bool GetFromContainer(IItem obj)
        {
            bool removed = _inventory.Remove(obj);
            return removed;
        }

        #endregion

        public IRoom Room { get; protected set; }

        public ICharacter Fighting { get; protected set; }

        public IEnumerable<EquipedItem> Equipments => _equipments;
        public IEnumerable<IItem> Inventory => Content;

        // Furniture (sleep/sit/stand)
        public IItemFurniture Furniture { get; protected set; }

        // Position
        public Positions Position { get; protected set; }

        // Class/Race
        public IClass Class { get; protected set; }
        public IRace Race { get; protected set; }

        // Attributes
        public int Level { get; protected set; }
        public int HitPoints { get; protected set; }
        public int MovePoints { get; protected set; }

        public CharacterFlags BaseCharacterFlags { get; protected set; }
        public CharacterFlags CharacterFlags { get; protected set; }

        public IRVFlags BaseImmunities { get; protected set; }
        public IRVFlags Immunities { get; protected set; }
        public IRVFlags BaseResistances { get; protected set; }
        public IRVFlags Resistances { get; protected set; }
        public IRVFlags BaseVulnerabilities { get; protected set; }
        public IRVFlags Vulnerabilities { get; protected set; }

        public Sex BaseSex { get; protected set; }
        public Sex Sex { get; protected set; }

        public int Alignment { get; protected set; }
        public bool IsEvil => Alignment <= -350;
        public bool IsGood => Alignment >= 350;
        public bool IsNeutral => !IsEvil && !IsGood;

        public int this[CharacterAttributes attribute]
        {
            get
            {
                int index = (int)attribute;
                if (index >= _currentAttributes.Length)
                {
                    Log.Default.WriteLine(LogLevels.Error, "Trying to get current attribute for attribute {0} (index {1}) but current attribute length is smaller", attribute, index);
                    return 0;
                }
                return _currentAttributes[index];
            }
            protected set 
            {
                int index = (int)attribute;
                if (index >= _currentAttributes.Length)
                {
                    Log.Default.WriteLine(LogLevels.Error, "Trying to set current attribute for attribute {0} (index {1}) but current attribute length is smaller", attribute, index);
                    return;
                }
                _currentAttributes[index] = value;
            }
        }

        public int this[BasicAttributes attribute] => this[(CharacterAttributes)attribute];
        public int this[Armors armor] => this[(CharacterAttributes)armor] + (Position > Positions.Sleeping ? TableValues.DefensiveBonus(this) : 0);
        public int HitRoll => this[CharacterAttributes.HitRoll] + TableValues.HitBonus(this);
        public int DamRoll => this[CharacterAttributes.DamRoll] + TableValues.DamBonus(this);

        public int this[ResourceKinds resource]
        {
            get 
            {
                int index = (int)resource;
                if (index >= _currentResources.Length)
                {
                    Log.Default.WriteLine(LogLevels.Error, "Trying to get current resource for resource {0} (index {1}) but current resource length is smaller", resource, index);
                    return 0;
                }
                return _currentResources[index];
            }
            protected set
            {
                int index = (int)resource;
                if (index >= _currentResources.Length)
                {
                    Log.Default.WriteLine(LogLevels.Error, "Trying to set current resource for resource {0} (index {1}) but current resource length is smaller", resource, index);
                    return;
                }
                _currentResources[index] = value;
            }
        }

        public IEnumerable<ResourceKinds> CurrentResourceKinds { get; private set; }

        // Form
        public Forms Form { get; private set; }

        // Abilities

        public IEnumerable<KnownAbility> KnownAbilities => _knownAbilities;
        public KnownAbility this[IAbility ability] => _knownAbilities.SingleOrDefault(x => x.Ability == ability);


        // Slave
        public ICharacter Slave { get; protected set; } // who is our slave (related to charm command/spell)
        public ICharacter ControlledBy { get; protected set; } // who is our master (related to charm command/spell)

        // Controller
        public bool ChangeSlave(ICharacter slave) 
        {
            if (!IsValid)
            {
                Log.Default.WriteLine(LogLevels.Error, "ICharacter.ChangeController: {0} is not valid anymore", DebugName);
                return false;
            }
            Log.Default.WriteLine(LogLevels.Debug, "ICharacter.ChangeSlave: {0} slave: old: {1}; new {2}", DebugName, Slave?.DebugName ?? "<<none>>", slave?.DebugName ?? "<<none>>");
            Slave = slave;
            return true;
        }

        public bool ChangeController(ICharacter master) // if non-null, start slavery, else, stop slavery
        {
            if (!IsValid)
            {
                Log.Default.WriteLine(LogLevels.Error, "ICharacter.ChangeController: {0} is not valid anymore", DebugName);
                return false;
            }
            if (ControlledBy != null)
            {
                Log.Default.WriteLine(LogLevels.Error, "ICharacter.ChangeController: {0} is already controlled by {1}", DebugName, ControlledBy.DebugName);
                return false;
            }

            Log.Default.WriteLine(LogLevels.Debug, "ICharacter.ChangeController: {0} master: old: {1}; new {2}", DebugName, ControlledBy?.DebugName ?? "<<none>>", master?.DebugName ?? "<<none>>");
            //if (master == null) // TODO: remove display ???
            //{
            //    if (ControlledBy != null)
            //    {
            //        Act(ActOptions.ToCharacter, "You stop following {0}.", ControlledBy);
            //        ControlledBy.Act(ActOptions.ToCharacter, "{0} stops following you.", this);
            //    }
            //}
            //else
            //{
            //    Act(ActOptions.ToCharacter, "You now follow {0}.", master);
            //    master.Act(ActOptions.ToCharacter, "{0} now follows you.", this);
            //}
            ControlledBy = master;
            return true;
        }

        // Act
        // IFormattable cannot be used because formatting depends on who'll receive the message (CanSee check)
        public void Act(ActOptions option, string format, params object[] arguments)
        {
            //
            IEnumerable<ICharacter> targets = GetActTargets(option);
            //
            foreach (ICharacter target in targets)
            {
                // TODO: optimization ?  FormatActOneLine will always return the same string for every target different than 'this' --> no anymore true with QuestObjective
                string phrase = FormatActOneLine(target, format, arguments);
                target.Send(phrase);
            }
        }

        public void ActToNotVictim(ICharacter victim, string format, params object[] arguments) // to everyone except this and victim
        {
            foreach (ICharacter to in Room.People.Where(x => x != this && x != victim))
            {
                string phrase = FormatActOneLine(to, format, arguments);
                to.Send(phrase);
            }
        }

        public void Act(IEnumerable<ICharacter> characters, string format, params object[] arguments)
        {
            foreach (ICharacter target in characters)
            {
                string phrase = FormatActOneLine(target, format, arguments);
                target.Send(phrase);
            }
        }

        // Equipments
        public bool Unequip(IEquipableItem item)
        {
            foreach (EquipedItem equipmentSlot in _equipments.Where(x => x.Item == item))
                equipmentSlot.Item = null;
            Recompute();
            return true;
        }

        // Furniture
        public bool ChangeFurniture(IItemFurniture furniture)
        {
            Furniture = furniture;
            return true;
        }

        // Position
        public bool ChangePosition(Positions position)
        {
            // TODO: false in some cases ?
            Position = position;
            return true;
        }

        // Visibility
        public bool CanSee(ICharacter target)
        {
            return true; // TODO: invis/sneak/blind
        }

        public virtual bool CanSee(IItem target)
        {
            //if (target.ItemFlags.HasFlag(ItemFlags.Invisible) && !CanSeeInvisible) return false
            return true; // TODO: invis/blind
        }

        public bool CanSee(IExit exit)
        {
            return true; // TODO: Hidden
        }

        public bool CanSee(IRoom room)
        {
    //        if (IS_SET(pRoomIndex->room_flags, ROOM_IMP_ONLY)
    //&& get_trust(ch) < MAX_LEVEL)
    //            return FALSE;

    //        if (IS_SET(pRoomIndex->room_flags, ROOM_GODS_ONLY)
    //        && !IS_IMMORTAL(ch))
    //            return FALSE;

    //        if (IS_SET(pRoomIndex->room_flags, ROOM_HEROES_ONLY)
    //        && !IS_IMMORTAL(ch))
    //            return FALSE;

    //        if (IS_SET(pRoomIndex->room_flags, ROOM_NEWBIES_ONLY)
    //        && ch->level > 5 && !IS_IMMORTAL(ch))
    //            return FALSE;

    //        if (!IS_IMMORTAL(ch) && pRoomIndex->clan && ch->clan != pRoomIndex->clan)
    //            return FALSE;

    //        return TRUE;

            return true; // TODO
        }

        // Attributes
        public int BaseAttribute(CharacterAttributes attribute)
        {
            int index = (int)attribute;
            if (index >= _baseAttributes.Length)
            {
                Log.Default.WriteLine(LogLevels.Error, "Trying to get base attribute for attribute {0} (index {1}) but base attribute length is smaller", attribute, index);
                return 0;
            }
            return _baseAttributes[index];
        }

        public int MaxResource(ResourceKinds resource)
        {
            int index = (int)resource;
            if (index >= _maxResources.Length)
            {
                Log.Default.WriteLine(LogLevels.Error, "Trying to get max resource for resource {0} (index {1}) but max resource length is smaller", resource, index);
                return 0;
            }
            return _maxResources[index];
        }

        public void UpdateResource(ResourceKinds resource, int amount)
        {
            this[resource] = (this[resource] + amount).Range(0, _maxResources[(int)resource]);
        }

        public void UpdateHitPoints(int amount)
        {
            HitPoints = (HitPoints + amount).Range(0, MaxHitPoints);
        }

        public void UpdateMovePoints(int amount)
        {
            MovePoints = (MovePoints + amount).Range(0, MaxMovePoints);
        }

        public void UpdateAlignment(int amount) 
        {
            Alignment = (Alignment + amount).Range(-MinAlignment, MaxAlignment);
            // TODO: impact on items ?
        }

        public void Regen()
        {
            // Hp/Move
            (int hitGain, int moveGain, int manaGain) gains = RegenBaseValues();
            int hitGain = gains.hitGain;
            int moveGain = gains.moveGain;
            int manaGain = gains.manaGain;

            // TODO: other resources
            // TODO: room heal rate
            if (Furniture != null && Furniture?.HealBonus != 0)
            {
                hitGain = (hitGain * Furniture.HealBonus) / 100;
                moveGain = (moveGain * Furniture.HealBonus) / 100;
            }
            if (Furniture != null && Furniture?.ResourceBonus != 0)
                manaGain = (manaGain * Furniture.ResourceBonus) / 100;
            if (CharacterFlags.HasFlag(CharacterFlags.Poison))
            {
                hitGain /= 4;
                moveGain /= 4;
                manaGain /= 4;
            }
            if (CharacterFlags.HasFlag(CharacterFlags.Plague))
            {
                hitGain /= 8;
                moveGain /= 8;
                manaGain /= 8;
            }
            if (CharacterFlags.HasFlag(CharacterFlags.Haste) || CharacterFlags.HasFlag(CharacterFlags.Slow))
            {
                hitGain /= 2;
                moveGain /= 2;
                manaGain /= 2;
            }
            HitPoints = Math.Min(HitPoints + hitGain, MaxHitPoints);
            MovePoints = Math.Min(MovePoints + moveGain, MaxMovePoints);
            UpdateResource(ResourceKinds.Mana, manaGain);

            // Other resources
        }

        public void AddBaseCharacterFlags(CharacterFlags characterFlags)
        {
            BaseCharacterFlags |= characterFlags;
            Recompute();
        }

        public void RemoveBaseCharacterFlags(CharacterFlags characterFlags)
        {
            BaseCharacterFlags &= ~characterFlags;
            Recompute();
        }

        // Form
        public bool ChangeForm(Forms form)
        {
            if (form == Form)
                return false;

            if (form == Forms.Normal)
                Send("You regain your normal form");

            Form = form;

            RecomputeKnownAbilities();
            Recompute();
            RecomputeCurrentResourceKinds();

            // Start values  TODO: depends on class ?
            this[ResourceKinds.Mana] = 100;
            this[ResourceKinds.Psy] = 0;

            return true;
        }

        // Recompute
        public override void Recompute()
        {
            // Reset current attributes
            ResetCurrentAttributes();

            // 1) Apply room auras
            if (Room != null)
                ApplyAuras(Room);

            // 2) Apply equipment auras
            foreach (EquipedItem equipment in Equipments.Where(x => x.Item != null))
                ApplyAuras(equipment.Item);

            // 3) Apply equipment armor
            foreach (EquipedItem equipment in Equipments.Where(x => x.Item is IItemArmor))
            {
                IItemArmor armor = equipment.Item as IItemArmor;
                int equipmentSlotMultiplier = TableValues.EquipmentSlotMultiplier(equipment.Slot);
                // TODO: IItemArmor: Bash/Pierce/Slash/Magic 4 values instead of one value
                int armorValue = armor.Armor * equipmentSlotMultiplier;
                this[CharacterAttributes.ArmorBash] -= armorValue;
                this[CharacterAttributes.ArmorPierce] -= armorValue;
                this[CharacterAttributes.ArmorSlash] -= armorValue;
                this[CharacterAttributes.ArmorMagic] -= armorValue;
            }

            // 4) Apply own auras
            ApplyAuras(this);

            // Keep attributes in valid range
            HitPoints = Math.Min(HitPoints, MaxHitPoints);
        }

        // Move
        public bool Move(ExitDirections direction, bool checkFighting, bool follow = false)
        {
            IRoom fromRoom = Room;
            IExit exit = fromRoom.Exit(direction);
            IRoom toRoom = exit?.Destination;

            // TODO: act_move.C:133
            // drunk
            // exit flags such as climb, door closed, ...
            // private room, size, swim room, guild room

            if (checkFighting && Fighting != null)
            {
                Send("No way! You are still fighting!");
                return false;
            }
            if (ControlledBy != null && ControlledBy.Room == Room)
            { // Slave cannot leave a room without Master
                Send("What?  And leave your beloved master?");
                return false;
            }
            if (exit == null || toRoom == null) // Check if existing exit
            {
                Send("You almost goes {0}, but suddenly realize that there's no exit there.", direction);
                Act(ActOptions.ToRoom, "{0} looks like {0:e}'s about to go {1}, but suddenly stops short and looks confused.", this, direction);
                return false;
            }
            if (exit.IsClosed)
            {
                Act(ActOptions.ToCharacter, "The {0} is closed.", exit);
                return false;
            }

            bool beforeMove = BeforeMove(direction, fromRoom, toRoom);
            if (!beforeMove)
                return false;

            //
            if (!CharacterFlags.HasFlag(CharacterFlags.Sneak))
                Act(ActOptions.ToRoom, "{0} leaves {1}.", this, direction);
            ChangeRoom(toRoom);

            // Autolook if impersonated/incarnated
            AutoLook();

            if (!CharacterFlags.HasFlag(CharacterFlags.Sneak))
                Act(ActOptions.ToRoom, "{0} has arrived.", this);

            // Followers: no circular follows
            if (fromRoom != toRoom)
                MoveFollow(fromRoom, toRoom, direction);

            return true;
        }

        public bool Enter(IItemPortal portal, bool follow = false)
        {
            IRoom destination = portal?.Destination;
            if (portal == null || destination == null || destination == Room)
            {
                Act(ActOptions.ToCharacter, "{0} doesn't seem to go anywhere.");
                return true;
            }

            IRoom wasRoom = Room;

            Act(ActOptions.ToRoom, "{0:N} steps into {1}.", this, portal);
            Act(ActOptions.ToCharacter, "You walk through {0} and find yourself somewhere else...", portal);

            ChangeRoom(destination);

            if (AutomaticallyDisplayRoom)
            {
                StringBuilder sb = new StringBuilder();
                AppendRoom(sb, Room);
                Send(sb);
            }

            Act(ActOptions.ToRoom, "{0:N} arrived through {1}.", this, portal);

            // Followers: no circular follows
            if (wasRoom != destination)
                EnterFollow(wasRoom, destination, portal);

            return true;
        }

        public void ChangeRoom(IRoom destination)
        {
            if (!IsValid)
            {
                Log.Default.WriteLine(LogLevels.Error, "ICharacter.ChangeRoom: {0} is not valid anymore", DebugName);
                return;
            }

            Log.Default.WriteLine(LogLevels.Debug, "ICharacter.ChangeRoom: {0} from: {1} to {2}", DebugName, Room == null ? "<<no room>>" : Room.DebugName, destination == null ? "<<no room>>" : destination.DebugName);
            Room?.Leave(this);
            Room = destination;
            destination?.Enter(this);
        }

        // Move
        public void AutoLook() // TODO: will be replaced with abstract method once DisplayRoom will be added in IRoom
        {
            if (this is IPlayableCharacter || IncarnatedBy != null)
            {
                StringBuilder sb = new StringBuilder();
                AppendRoom(sb, Room);
                Send(sb);
            }
        }

        // Combat
        public ResistanceLevels CheckResistance(SchoolTypes damageType)
        {
            IRVFlags irvFlags;
            // Generic resistance
            ResistanceLevels defaultResistance = ResistanceLevels.Normal;
            if (damageType <= SchoolTypes.Slash) // Physical
            {
                if (Immunities.HasFlag(IRVFlags.Weapon))
                    defaultResistance = ResistanceLevels.Immune;
                else if (Resistances.HasFlag(IRVFlags.Weapon))
                    defaultResistance = ResistanceLevels.Resistant;
                else if (Vulnerabilities.HasFlag(IRVFlags.Weapon))
                    defaultResistance = ResistanceLevels.Normal;
            }
            else // Magic
            {
                if (Immunities.HasFlag(IRVFlags.Magic))
                    defaultResistance = ResistanceLevels.Immune;
                else if (Resistances.HasFlag(IRVFlags.Magic))
                    defaultResistance = ResistanceLevels.Resistant;
                else if (Vulnerabilities.HasFlag(IRVFlags.Magic))
                    defaultResistance = ResistanceLevels.Normal;
            }
            switch (damageType)
            {
                case SchoolTypes.None:
                    return ResistanceLevels.None; // no Resistance
                case SchoolTypes.Bash:
                case SchoolTypes.Pierce:
                case SchoolTypes.Slash:
                    irvFlags = IRVFlags.Weapon;
                    break;
                case SchoolTypes.Fire:
                    irvFlags = IRVFlags.Fire;
                    break;
                case SchoolTypes.Cold:
                    irvFlags = IRVFlags.Cold;
                    break;
                case SchoolTypes.Lightning:
                    irvFlags = IRVFlags.Lightning;
                    break;
                case SchoolTypes.Acid:
                    irvFlags = IRVFlags.Acid;
                    break;
                case SchoolTypes.Poison:
                    irvFlags = IRVFlags.Poison;
                    break;
                case SchoolTypes.Negative:
                    irvFlags = IRVFlags.Negative;
                    break;
                case SchoolTypes.Holy:
                    irvFlags = IRVFlags.Holy;
                    break;
                case SchoolTypes.Energy:
                    irvFlags = IRVFlags.Energy;
                    break;
                case SchoolTypes.Mental:
                    irvFlags = IRVFlags.Mental;
                    break;
                case SchoolTypes.Disease:
                    irvFlags = IRVFlags.Disease;
                    break;
                case SchoolTypes.Drowning:
                    irvFlags = IRVFlags.Drowning;
                    break;
                case SchoolTypes.Light:
                    irvFlags = IRVFlags.Light;
                    break;
                case SchoolTypes.Other: // no specific IRV
                    return defaultResistance;
                case SchoolTypes.Harm: // no specific IRV
                    return defaultResistance;
                case SchoolTypes.Charm:
                    irvFlags = IRVFlags.Charm;
                    break;
                case SchoolTypes.Sound:
                    irvFlags = IRVFlags.Sound;
                    break;
                default:
                    Log.Default.WriteLine(LogLevels.Error, $"CharacterBase.CheckResistance: Unknown {nameof(SchoolTypes)}.{damageType}");
                    return defaultResistance;
            }
            // Following code has been reworked because Rom24 was testing on currently computed resistance (imm) instead of defaultResistance (def)
            ResistanceLevels resistance = ResistanceLevels.None;
            if (Immunities.HasFlag(irvFlags))
                resistance = ResistanceLevels.Immune;
            else if (Resistances.HasFlag(irvFlags) && defaultResistance != ResistanceLevels.Immune)
                resistance = ResistanceLevels.Resistant;
            else if (Vulnerabilities.HasFlag(irvFlags))
            {
                if (defaultResistance == ResistanceLevels.Immune)
                    resistance = ResistanceLevels.Resistant;
                else if (defaultResistance == ResistanceLevels.Resistant)
                    resistance = ResistanceLevels.Normal;
                else
                    resistance = ResistanceLevels.Vulnerable;
            }
            // if no specific resistance found, return generic one
            if (resistance == ResistanceLevels.None)
                return defaultResistance;
            // else, return specific resistance
            return resistance;
        }

        public bool Heal(IEntity source, IAbility ability, int amount, bool visible)
        {
            if (!IsValid)
            {
                Log.Default.WriteLine(LogLevels.Error, "ICharacter.Heal: {0} is not valid anymore", DebugName);
                return false;
            }

            // Modify heal (resist, vuln, invul, absorb)
            bool fullyAbsorbed;
            amount = ModifyHeal(amount, out fullyAbsorbed);

            Log.Default.WriteLine(LogLevels.Info, "{0} healed by {1} {2} for {3}", DebugName, source == null ? "<<??>>" : source.DebugName, ability == null ? "<<??>>" : ability.Name, amount);
            if (amount <= 0)
                Log.Default.WriteLine(LogLevels.Warning, "ICharacter.Heal: invalid amount {0} on {1}", amount, DebugName);
            else
                HitPoints = Math.Min(HitPoints + amount, MaxHitPoints);

            Log.Default.WriteLine(LogLevels.Debug, "{0} HP left: {1}", DebugName, HitPoints);

            // Display heal
            if (visible)
            {
                if (fullyAbsorbed)
                    DisplayHealAbsorbPhrase(ability?.Name ?? "Something", source);
                else
                    DisplayHealPhrase(ability, amount, source);
            }

            return true;
        }

        public bool UnknownSourceHeal(IAbility ability, int amount, bool visible)
        {
            if (!IsValid)
            {
                Log.Default.WriteLine(LogLevels.Error, "ICharacter.UnknownSourceHeal: {0} is not valid anymore", DebugName);
                return false;
            }

            // Modify heal (resist, vuln, invul, absorb)
            bool fullyAbsorbed;
            amount = ModifyHeal(amount, out fullyAbsorbed);

            Log.Default.WriteLine(LogLevels.Info, "{0} healed by {1} for {2}", DebugName, ability == null ? "<<??>>" : ability.Name, amount);
            if (amount <= 0)
                Log.Default.WriteLine(LogLevels.Warning, "ICharacter.UnknownSourceHeal: invalid amount {0} on {1}", amount, DebugName);
            else
                HitPoints = Math.Min(HitPoints + amount, MaxHitPoints);

            Log.Default.WriteLine(LogLevels.Debug, "{0} HP left: {1}", DebugName, HitPoints);

            // Display heal
            if (visible)
            {
                if (fullyAbsorbed)
                    DisplayUnknownSourceHealAbsorbPhrase(ability?.Name ?? "Something");
                else
                    DisplayUnknownSourceHealPhrase(ability, amount);
            }
            return true;
        }

        public bool MultiHit(ICharacter enemy)
        {
            // TODO: read http://wowwiki.wikia.com/wiki/Combat
            if (!IsValid)
            {
                Log.Default.WriteLine(LogLevels.Error, "ICharacter.MultiHit: {0} is not valid anymore", DebugName);
                return false;
            }

            if (!IsValid)
                return false;

            Log.Default.WriteLine(LogLevels.Debug, "ICharacter.MultiHit: {0} -> {1}", DebugName, enemy.DebugName);

            if (this == enemy || Room != enemy.Room)
                return false;

            // TODO
            return false;

            //// TODO: more haste -> more attacks (should depend on weapon speed, attack speed, server speed[PulseViolence])

            //// TODO: TEST purpose
            ////int attackCount = Math.Max(1, 1 + this[SecondaryAttributeTypes.AttackSpeed] / 21);
            //int attackCount = 1;//Math.Max(1, 1 + this[SecondaryAttributeTypes.AttackSpeed]);

            //// Main hand
            //for (int i = 0; i < attackCount; i++)
            //{
            //    // Cannot store wielded between hit (disarm anyone ?)
            //    IItemWeapon wielded = Equipments.FirstOrDefault(x => x.Slot == EquipmentSlots.MainHand)?.Item as IItemWeapon;
            //    SchoolTypes damageType = wielded?.DamageType ?? SchoolTypes.Slash;
            //    OneHit(enemy, wielded, damageType, false);

            //    if (Fighting != enemy) // stop multihit if different enemy or no enemy
            //        return true;
            //}

            //// Off hand
            //if (KnownAbilities.Any(x => x.Ability == AbilityManager.DualWieldAbility))
            //{
            //    IItemWeapon wielded2 = Equipments.FirstOrDefault(x => x.Slot == EquipmentSlots.OffHand)?.Item as IItemWeapon;
            //    if (wielded2 != null)
            //        OneHit(enemy, wielded2, wielded2.DamageType, true);
            //}
            //if (Fighting != enemy) // stop multihit if different enemy or no enemy
            //    return true;

            //// Second main hand
            //if (KnownAbilities.Any(x => x.Ability == AbilityManager.ThirdWieldAbility))
            //{
            //    IItemWeapon wielded3 = Equipments.Where(x => x.Slot == EquipmentSlots.MainHand).ElementAtOrDefault(1)?.Item as IItemWeapon;
            //    if (wielded3 != null)
            //        OneHit(enemy, wielded3, wielded3.DamageType, true);
            //}
            //if (Fighting != enemy) // stop multihit if different enemy or no enemy
            //    return true;

            //// Second off hand
            //if (KnownAbilities.Any(x => x.Ability == AbilityManager.FourthWieldAbility))
            //{
            //    IItemWeapon wielded4 = Equipments.Where(x => x.Slot == EquipmentSlots.OffHand).ElementAtOrDefault(1)?.Item as IItemWeapon;
            //    if (wielded4 != null)
            //        OneHit(enemy, wielded4, wielded4.DamageType, true);
            //}
            //if (Fighting != enemy) // stop multihit if different enemy or no enemy
            //    return true;

            //return true;
        }

        public bool StartFighting(ICharacter enemy) // equivalent to set_fighting in fight.C:3441
        {
            if (!IsValid)
            {
                Log.Default.WriteLine(LogLevels.Error, "StartFighting: {0} is not valid anymore", DebugName);
                return false;
            }

            Log.Default.WriteLine(LogLevels.Debug, "{0} starts fighting {1}", DebugName, enemy.DebugName);

            ChangePosition(Positions.Fighting);
            Fighting = enemy;
            return true;
        }

        public bool StopFighting(bool both) // equivalent to stop_fighting in fight.C:3441
        {
            Log.Default.WriteLine(LogLevels.Debug, "{0} stops fighting {1}", Name, Fighting == null ? "<<no enemy>>" : Fighting.Name);

            Fighting = null;
            ChangePosition(Positions.Standing);
            if (both)
            {
                foreach (ICharacter enemy in World.Characters.Where(x => x.Fighting == this))
                    enemy.StopFighting(false);
            }
            return true;
        }

        public bool WeaponDamage(ICharacter source, IItemWeapon weapon, int damage, SchoolTypes damageType, bool visible) // damage from weapon(or bare hands) of known source
        {
            return GenericDamage(source, weapon?.RelativeDisplayName(weapon.EquipedBy), damage, damageType, visible);
        }

        public bool AbilityDamage(IEntity source, IAbility ability, int damage, SchoolTypes damageType, bool visible) // damage from ability of known source
        {
            return GenericDamage(source, ability?.Name, damage, damageType, visible);
        }

        // TODO: refactor: GenericDamage is almost identical
        public bool UnknownSourceDamage(IAbility ability, int damage, SchoolTypes damageType, bool visible) // damage with unknown source or no source
        {
            if (!IsValid)
            {
                Log.Default.WriteLine(LogLevels.Error, "UnknownSourceDamage: {0} is not valid anymore", DebugName);
                return false;
            }

            // Modify damage (resist, vuln, invul, absorb)
            bool fullyAbsorbed;
            damage = ModifyDamage(damage, int.MaxValue, damageType, out fullyAbsorbed);

            // Display damage
            if (visible) // equivalent to dam_message in fight.C:4381
            {
                if (fullyAbsorbed)
                    DisplayUnknownSourceAbsorbPhrase(ability?.Name);
                else
                    DisplayUnknownSourceDamagePhrase(ability?.Name, damage);
            }

            // No damage -> stop here
            if (damage == 0)
            {
                Log.Default.WriteLine(LogLevels.Debug, "{0} does no damage to {1}", ability, DebugName);

                return false;
            }

            Log.Default.WriteLine(LogLevels.Debug, "{0} does {1} damage to {2}", ability, damage, DebugName);

            bool dead = ApplyDamageAndDisplayStatus(damage);

            Log.Default.WriteLine(LogLevels.Debug, "{0} HP left: {1}", DebugName, HitPoints);

            // If dead, create corpse, xp gain/loss, remove character from world if needed
            if (dead) // TODO: fight.C:2246
            {
                Log.Default.WriteLine(LogLevels.Debug, "{0} has been killed by {1}", DebugName, ability);

                StopFighting(false);
                RawKilled(null, false);
                return true;
            }
            return true;
        }

        public void Slay(IPlayableCharacter killer)
        {
            RawKilled(killer, false);
        }

        public abstract void KillingPayoff(ICharacter victim);

        public bool SavesSpell(int level, SchoolTypes damageType)
        {
            ICharacter victim = this;
            int save = 50 + (victim.Level - level) * 5 - victim[CharacterAttributes.SavingThrow] * 2;
            if (victim.CharacterFlags.HasFlag(CharacterFlags.Berserk))
                save += victim.Level / 2;
            ResistanceLevels resist = victim.CheckResistance(damageType);
            switch (resist)
            {
                case ResistanceLevels.Immune:
                    return true;
                case ResistanceLevels.Resistant:
                    save += 2;
                    break;
                case ResistanceLevels.Vulnerable:
                    save -= 2;
                    break;
            }
            if (victim.Class?.CurrentResourceKinds(victim.Form).Contains(ResourceKinds.Mana) == true)
                save = (save * 9) / 10;
            save = save.Range(5, 95);
            return RandomManager.Chance(save);
        }

        public bool IsSafeSpell(ICharacter caster, bool area)
        {
            ICharacter victim = this;
            if (!victim.IsValid || victim.Room == null || !caster.IsValid || caster.Room == null)
                return true;
            if (area && caster == victim)
                return true;
            if (victim.Fighting == caster || victim == caster)
                return false;
            if (!area && (caster is IPlayableCharacter pcCaster && pcCaster.ImpersonatedBy is IAdmin))
                return false;
            // Killing npc
            if (victim is INonPlayableCharacter npcVictim)
            {
                // safe room ?
                if (victim.Room.CurrentRoomFlags.HasFlag(RoomFlags.Safe))
                    return true;
                // TODO: No fight in a shop -> send_to_char("The shopkeeper wouldn't like that.\n\r",ch);
                // TODO: Can't killer trainer, practicer, healer, changer, questor  -> send_to_char("I don't think Mota would approve.\n\r",ch);

                // Npc doing the killing
                if (caster is INonPlayableCharacter)
                {
                    // no pets
                    if (npcVictim.ActFlags.HasFlag(ActFlags.Pet))
                        return true;
                    // no charmed creatures unless owner
                    if (victim.CharacterFlags.HasFlag(CharacterFlags.Charm) && (area || caster != victim.ControlledBy))
                        return true;
                    // TODO: legal kill? -- cannot hit mob fighting non-group member
                    //if (victim->fighting != NULL && !is_same_group(ch,victim->fighting)) -> true
                }
                // Player doing the killing
                else
                {
                    // TODO: area effect spells do not hit other mobs
                    //if (area && !is_same_group(victim,ch->fighting)) -> true
                }
            }
            // Killing players
            else
            {
                if (area && (victim is IPlayableCharacter pcVictim && pcVictim.ImpersonatedBy is IAdmin))
                    return true;
                // Npc doing the killing
                if (caster is INonPlayableCharacter npcCaster)
                {
                    // charmed mobs and pets cannot attack players while owned
                    if (caster.CharacterFlags.HasFlag(CharacterFlags.Charm) && caster.ControlledBy != null && caster.ControlledBy.Fighting != victim)
                        return true;
                    // safe room
                    if (victim.Room.CurrentRoomFlags.HasFlag(RoomFlags.Safe))
                        return true;
                    // TODO:  legal kill? -- mobs only hit players grouped with opponent
                    //if (ch->fighting != NULL && !is_same_group(ch->fighting, victim))
                }
                // Player doing the killing
                else
                {
                    // TODO: PK
                    //if (!is_clan(ch))
                    //    return true;

                    //if (IS_SET(victim->act, PLR_KILLER) || IS_SET(victim->act, PLR_THIEF))
                    //    return FALSE;

                    //if (!is_clan(victim))
                    //    return true;

                    //if (ch->level > victim->level + 8)
                    //    return true;
                }
            }
            return false;
        }

        public bool IsSafe(ICharacter character)
        {
            ICharacter victim = this;
            if (!victim.IsValid || victim.Room == null || !character.IsValid || character.Room == null)
                return true;
            if (victim.Fighting == character || victim == character)
                return false;
            if (character is IPlayableCharacter pcCaster && pcCaster.ImpersonatedBy is IAdmin)
                return false;
            // Killing npc
            if (victim is INonPlayableCharacter npcVictim)
            {
                if (victim.Room.CurrentRoomFlags.HasFlag(RoomFlags.Safe))
                {
                    character.Send("Not in the room.");
                    return true;
                }
                // TODO: No fight in a shop -> send_to_char("The shopkeeper wouldn't like that.\n\r",ch);
                // TODO: Can't killer trainer, practicer, healer, changer, questor -> send_to_char("I don't think Mota would approve.\n\r",ch);

                // Player doing the killing
                if (character is IPlayableCharacter)
                {
                    // no pets
                    if (npcVictim.ActFlags.HasFlag(ActFlags.Pet))
                    {
                        character.Act(ActOptions.ToCharacter, "But {0} looks so cute and cuddly...", victim);
                        return true;
                    }
                    // no charmed creatures unless owner
                    if (victim.CharacterFlags.HasFlag(CharacterFlags.Charm) && character != victim.ControlledBy)
                    {
                        character.Send("You don't own that monster.");
                        return true;
                    }
                }
            }
            // Killing player
            else
            {
                // Npc doing the killing
                if (character is INonPlayableCharacter npcCaster)
                {
                    // safe room
                    if (victim.Room.CurrentRoomFlags.HasFlag(RoomFlags.Safe))
                    {
                        character.Send("Not in the room.");
                        return true;
                    }
                    // charmed mobs and pets cannot attack players while owned
                    if (character.CharacterFlags.HasFlag(CharacterFlags.Charm) && character.ControlledBy != null && character.ControlledBy.Fighting != victim)
                    {
                        character.Send("Players are your friends!");
                        return true;
                    }
                }
                // Player doing the killing
                else
                {
                    //if (!is_clan(ch))
                    //{
                    //    send_to_char("Join a clan if you want to kill players.\n\r", ch);
                    //    return true;
                    //}

                    //if (IS_SET(victim->act, PLR_KILLER) || IS_SET(victim->act, PLR_THIEF))
                    //    return FALSE;

                    //if (!is_clan(victim))
                    //{
                    //    send_to_char("They aren't in a clan, leave them alone.\n\r", ch);
                    //    return true;
                    //}

                    //if (ch->level > victim->level + 8)
                    //{
                    //    send_to_char("Pick on someone your own size.\n\r", ch);
                    //    return true;
                    //}
                }
            }
            return false;
        }

        // Ability
        public KnownAbility GetKnownAbility(string name) => _knownAbilities.SingleOrDefault(x => StringCompareHelpers.StringEquals(x.Ability.Name, name));

        public IDictionary<IAbility, DateTime> AbilitiesInCooldown => _cooldowns;

        public bool HasAbilitiesInCooldown => _cooldowns.Any();

        public int CooldownSecondsLeft(IAbility ability)
        {
            DateTime nextAvailability;
            if (_cooldowns.TryGetValue(ability, out nextAvailability))
            {
                TimeSpan diff = nextAvailability - TimeHandler.CurrentTime;
                int secondsLeft = (int) Math.Ceiling(diff.TotalSeconds);
                return secondsLeft;
            }
            return int.MinValue;
        }

        public void SetCooldown(IAbility ability)
        {
            // TODO
            //DateTime nextAvailability = TimeHandler.CurrentTime.AddSeconds(ability.Cooldown);
            //_cooldowns[ability] = nextAvailability;
        }

        public void ResetCooldown(IAbility ability, bool verbose)
        {
            _cooldowns.Remove(ability);
            if (verbose)
                Send("%c%{0} is available.%x%", ability.Name);
        }

        // Equipment
        public EquipedItem SearchEquipmentSlot(IEquipableItem item, bool replace)
        {
            switch (item.WearLocation)
            {
                case WearLocations.None:
                    return null;
                case WearLocations.Light:
                    return SearchEquipmentSlot(EquipmentSlots.Light, replace);
                case WearLocations.Head:
                    return SearchEquipmentSlot(EquipmentSlots.Head, replace);
                case WearLocations.Amulet:
                    return SearchEquipmentSlot(EquipmentSlots.Amulet, replace);
                case WearLocations.Shoulders:
                    return SearchEquipmentSlot(EquipmentSlots.Shoulders, replace);
                case WearLocations.Chest:
                    return SearchEquipmentSlot(EquipmentSlots.Chest, replace);
                case WearLocations.Cloak:
                    return SearchEquipmentSlot(EquipmentSlots.Cloak, replace);
                case WearLocations.Waist:
                    return SearchEquipmentSlot(EquipmentSlots.Waist, replace);
                case WearLocations.Wrists:
                    return SearchEquipmentSlot(EquipmentSlots.Wrists, replace);
                case WearLocations.Arms:
                    return SearchEquipmentSlot(EquipmentSlots.Arms, replace);
                case WearLocations.Hands:
                    return SearchEquipmentSlot(EquipmentSlots.Hands, replace);
                case WearLocations.Ring:
                    return SearchEquipmentSlot(EquipmentSlots.Ring, replace);
                case WearLocations.Legs:
                    return SearchEquipmentSlot(EquipmentSlots.Legs, replace);
                case WearLocations.Feet:
                    return SearchEquipmentSlot(EquipmentSlots.Feet, replace);
                case WearLocations.Trinket:
                    return SearchEquipmentSlot(EquipmentSlots.Trinket, replace);
                case WearLocations.Wield:
                    // Search empty mainhand, then empty offhand, TODO use offhand only if mainhand is not wielding a 2H
                    return SearchOneHandedWeaponEquipmentSlot(replace);
                case WearLocations.Hold:
                    // only if mainhand is not wielding a 2H
                    return SearchOffhandEquipmentSlot(replace);
                case WearLocations.Shield:
                    // only if mainhand is not wielding a 2H
                    return SearchOffhandEquipmentSlot(replace);
                case WearLocations.Wield2H:
                    // Search empty mainhand + empty offhand (no autoreplace) // TODO can wield 2H on one hand if giant or specific ability
                    var mainHand = Equipments.FirstOrDefault(x => x.Slot == EquipmentSlots.MainHand && x.Item == null);
                    var offHand = Equipments.FirstOrDefault(x => x.Slot == EquipmentSlots.OffHand && x.Item == null);
                    if (mainHand != null && offHand != null && mainHand.Item == null && offHand.Item == null)
                        return mainHand;
                    break;
            }
            return null;
        }

        // Affects
        public void ApplyAffect(CharacterFlagsAffect affect)
        {
            switch (affect.Operator)
            {
                case AffectOperators.Add:
                case AffectOperators.Or:
                    CharacterFlags |= affect.Modifier;
                    break;
                case AffectOperators.Assign:
                    CharacterFlags = affect.Modifier;
                    break;
                case AffectOperators.Nor:
                    CharacterFlags &= ~affect.Modifier;
                    break;
                default:
                    break;
            }
        }

        public void ApplyAffect(CharacterIRVAffect affect)
        {
            switch (affect.Location)
            {
                case IRVAffectLocations.Immunities:
                    switch (affect.Operator)
                    {
                        case AffectOperators.Add:
                        case AffectOperators.Or:
                            Immunities |= affect.Modifier;
                            break;
                        case AffectOperators.Assign:
                            Immunities = affect.Modifier;
                            break;
                        case AffectOperators.Nor:
                            Immunities &= ~affect.Modifier;
                            break;
                        default:
                            break;
                    }
                    break;
                case IRVAffectLocations.Resistances:
                    switch (affect.Operator)
                    {
                        case AffectOperators.Add:
                        case AffectOperators.Or:
                            Resistances |= affect.Modifier;
                            break;
                        case AffectOperators.Assign:
                            Resistances = affect.Modifier;
                            break;
                        case AffectOperators.Nor:
                            Resistances &= ~affect.Modifier;
                            break;
                        default:
                            break;
                    }
                    break;
                case IRVAffectLocations.Vulnerabilities:
                    switch (affect.Operator)
                    {
                        case AffectOperators.Add:
                        case AffectOperators.Or:
                            Resistances |= affect.Modifier;
                            break;
                        case AffectOperators.Assign:
                            Resistances = affect.Modifier;
                            break;
                        case AffectOperators.Nor:
                            Resistances &= ~affect.Modifier;
                            break;
                        default:
                            break;
                    }
                    break;
            }
        }

        public void ApplyAffect(CharacterAttributeAffect affect)
        {
            if (affect.Location == CharacterAttributeAffectLocations.None)
                return;
            if (affect.Location == CharacterAttributeAffectLocations.Characteristics)
            {
                switch (affect.Operator)
                {
                    case AffectOperators.Add:
                        _currentAttributes[(int)CharacterAttributes.Strength] += affect.Modifier;
                        _currentAttributes[(int)CharacterAttributes.Intelligence] += affect.Modifier;
                        _currentAttributes[(int)CharacterAttributes.Wisdom] += affect.Modifier;
                        _currentAttributes[(int)CharacterAttributes.Dexterity] += affect.Modifier;
                        _currentAttributes[(int)CharacterAttributes.Constitution] += affect.Modifier;
                        break;
                    case AffectOperators.Assign:
                        _currentAttributes[(int)CharacterAttributes.Strength] = affect.Modifier;
                        _currentAttributes[(int)CharacterAttributes.Intelligence] = affect.Modifier;
                        _currentAttributes[(int)CharacterAttributes.Wisdom] = affect.Modifier;
                        _currentAttributes[(int)CharacterAttributes.Dexterity] = affect.Modifier;
                        _currentAttributes[(int)CharacterAttributes.Constitution] = affect.Modifier;
                        break;
                    case AffectOperators.Or:
                    case AffectOperators.Nor:
                    default:
                        // Error
                        break;
                }
                return;
            }
            if (affect.Location == CharacterAttributeAffectLocations.AllArmor)
            {
                switch (affect.Operator)
                {
                    case AffectOperators.Add:
                        _currentAttributes[(int)CharacterAttributes.ArmorBash] += affect.Modifier;
                        _currentAttributes[(int)CharacterAttributes.ArmorPierce] += affect.Modifier;
                        _currentAttributes[(int)CharacterAttributes.ArmorSlash] += affect.Modifier;
                        _currentAttributes[(int)CharacterAttributes.ArmorMagic] += affect.Modifier;
                        break;
                    case AffectOperators.Assign:
                        _currentAttributes[(int)CharacterAttributes.ArmorBash] = affect.Modifier;
                        _currentAttributes[(int)CharacterAttributes.ArmorPierce] = affect.Modifier;
                        _currentAttributes[(int)CharacterAttributes.ArmorSlash] = affect.Modifier;
                        _currentAttributes[(int)CharacterAttributes.ArmorMagic] = affect.Modifier;
                        break;
                    case AffectOperators.Or:
                    case AffectOperators.Nor:
                    default:
                        // Error
                        break;
                }
                return;
            }
            CharacterAttributes attribute;
            switch (affect.Location)
            {
                case CharacterAttributeAffectLocations.Strength: attribute = CharacterAttributes.Strength; break;
                case CharacterAttributeAffectLocations.Intelligence: attribute = CharacterAttributes.Intelligence; break;
                case CharacterAttributeAffectLocations.Wisdom: attribute = CharacterAttributes.Wisdom; break;
                case CharacterAttributeAffectLocations.Dexterity: attribute = CharacterAttributes.Dexterity; break;
                case CharacterAttributeAffectLocations.Constitution: attribute = CharacterAttributes.Constitution; break;
                case CharacterAttributeAffectLocations.MaxHitPoints: attribute = CharacterAttributes.MaxHitPoints; break;
                case CharacterAttributeAffectLocations.SavingThrow: attribute = CharacterAttributes.SavingThrow; break;
                case CharacterAttributeAffectLocations.HitRoll: attribute = CharacterAttributes.HitRoll; break;
                case CharacterAttributeAffectLocations.DamRoll: attribute = CharacterAttributes.DamRoll; break;
                case CharacterAttributeAffectLocations.MaxMovePoints: attribute = CharacterAttributes.MaxMovePoints; break;
                case CharacterAttributeAffectLocations.ArmorBash: attribute = CharacterAttributes.ArmorBash; break;
                case CharacterAttributeAffectLocations.ArmorPierce: attribute = CharacterAttributes.ArmorPierce; break;
                case CharacterAttributeAffectLocations.ArmorSlash: attribute = CharacterAttributes.ArmorSlash; break;
                case CharacterAttributeAffectLocations.ArmorMagic: attribute = CharacterAttributes.ArmorMagic; break;
                default:
                    Log.Default.WriteLine(LogLevels.Error, $"CharacterBase.ApplyAffect: Unexpected CharacterAttributeAffectLocations {affect.Location}");
                    return;
            }
            switch (affect.Operator)
            {
                case AffectOperators.Add:
                    _currentAttributes[(int)attribute] += affect.Modifier;
                    break;
                case AffectOperators.Assign:
                    _currentAttributes[(int)attribute] = affect.Modifier;
                    break;
                case AffectOperators.Or:
                case AffectOperators.Nor:
                default:
                    // Error
                    break;
            }
        }

        public void ApplyAffect(CharacterSexAffect affect)
        {
            Sex = affect.Value;
        }

        #endregion

        protected abstract int NoWeaponDamage { get; }

        protected abstract int HitPointMinValue { get; }

        protected abstract int ModifyCriticalDamage(int damage);

        protected abstract bool BeforeMove(ExitDirections direction, IRoom fromRoom, IRoom toRoom);

        protected abstract (int hitGain, int moveGain, int manaGain) RegenBaseValues();

        protected virtual bool AutomaticallyDisplayRoom => IncarnatedBy != null;

        protected virtual void MoveFollow(IRoom fromRoom, IRoom toRoom, ExitDirections direction)
        {
            if (fromRoom != toRoom)
            {
                if (Slave != null)
                {
                    Slave.Send("You follow {0}.", DebugName);
                    Slave.Move(direction, true);
                }
            }
        }

        protected virtual void EnterFollow(IRoom wasRoom, IRoom destination, IItemPortal portal)
        {
            if (wasRoom != destination)
            {
                if (Slave != null)
                {
                    Slave.Send("You follow {0}.", DebugName);
                    Slave.Enter(portal, true);
                }
            }
        }

        protected virtual IEnumerable<ICharacter> GetActTargets(ActOptions option)
        {
            switch (option)
            {
                case ActOptions.ToAll:
                    return Room.People;
                case ActOptions.ToRoom:
                    return Room.People.Where(x => x != this);
                case ActOptions.ToGroup:
                    Log.Default.WriteLine(LogLevels.Warning, "Act with option ToGroup used on generic CharacterBase");
                    return Enumerable.Empty<ICharacter>(); // defined only for PlayableCharacter
                case ActOptions.ToCharacter:
                    return Enumerable.Repeat(this, 1);
                default:
                    Log.Default.WriteLine(LogLevels.Error, "Act with invalid option: {0}", option);
                    return Enumerable.Empty<ICharacter>();
            }
        }

        protected abstract bool RawKilled(IEntity killer, bool killingPayoff);

        protected void ResetCooldowns()
        {
            _cooldowns.Clear();
        }

        protected void DeleteInventory()
        {
            _inventory.Clear();
        }

        protected void DeleteEquipments()
        {
            _equipments.Clear();
        }

        protected void BuildEquipmentSlots()
        {
            // TODO: depend on race+affects+...
            if (Race != null)
            {
                // TODO: take care of existing equipment (add only new slot, if slot is removed put equipment in inventory)
                _equipments.Clear();
                _equipments.AddRange(Race.EquipmentSlots.Select(x => new EquipedItem(x)));
            }
            else
            {
                _equipments.Add(new EquipedItem(EquipmentSlots.Light));
                _equipments.Add(new EquipedItem(EquipmentSlots.Head));
                _equipments.Add(new EquipedItem(EquipmentSlots.Amulet));
                _equipments.Add(new EquipedItem(EquipmentSlots.Shoulders));
                _equipments.Add(new EquipedItem(EquipmentSlots.Chest));
                _equipments.Add(new EquipedItem(EquipmentSlots.Cloak));
                _equipments.Add(new EquipedItem(EquipmentSlots.Waist));
                _equipments.Add(new EquipedItem(EquipmentSlots.Wrists));
                _equipments.Add(new EquipedItem(EquipmentSlots.Arms));
                _equipments.Add(new EquipedItem(EquipmentSlots.Hands));
                _equipments.Add(new EquipedItem(EquipmentSlots.Ring)); // 2 rings
                _equipments.Add(new EquipedItem(EquipmentSlots.Ring));
                _equipments.Add(new EquipedItem(EquipmentSlots.Legs));
                _equipments.Add(new EquipedItem(EquipmentSlots.Feet));
                _equipments.Add(new EquipedItem(EquipmentSlots.Trinket)); // 2 trinkets
                _equipments.Add(new EquipedItem(EquipmentSlots.Trinket));
                _equipments.Add(new EquipedItem(EquipmentSlots.MainHand)); // 2 hands
                _equipments.Add(new EquipedItem(EquipmentSlots.OffHand));
            }
        }

        protected EquipedItem SearchEquipmentSlot(EquipmentSlots equipmentSlot, bool replace)
        {
            if (replace) // search empty slot, if not found, return first matching slot
                return Equipments.FirstOrDefault(x => x.Slot == equipmentSlot && x.Item == null) ?? Equipments.FirstOrDefault(x => x.Slot == equipmentSlot);
            return Equipments.FirstOrDefault(x => x.Slot == equipmentSlot && x.Item == null);
        }

        protected EquipedItem SearchOneHandedWeaponEquipmentSlot(bool replace)
        {
            // Search empty mainhand, then empty offhand only if mainhand is not wielding a 2H
            if (replace)
            {
                // Search empty main hand
                var mainHand = Equipments.FirstOrDefault(x => x.Slot == EquipmentSlots.MainHand && x.Item == null);
                if (mainHand != null)
                    return mainHand;
                // Search empty off hand
                var offHand = SearchOffhandEquipmentSlot(false);
                if (offHand != null)
                    return offHand;
                // If not empty main/off hand, search an slot to replace
                return Equipments.FirstOrDefault(x => x.Slot == EquipmentSlots.MainHand) ?? SearchOffhandEquipmentSlot(true);
            }
            else
            {
                // Search empty main hand
                var mainHand = Equipments.FirstOrDefault(x => x.Slot == EquipmentSlots.MainHand && x.Item == null);
                if (mainHand != null)
                    return mainHand;
                // If not empty main hand found, search off hand
                return SearchOffhandEquipmentSlot(false);
            }

            //if (replace)
            //    return Equipments.FirstOrDefault(x => x.Slot == EquipmentSlots.MainHand && x.Item == null) ?? Equipments.FirstOrDefault(x => x.Slot == EquipmentSlots.OffHand && x.Item == null)
            //           ?? Equipments.FirstOrDefault(x => x.Slot == EquipmentSlots.MainHand) ?? Equipments.FirstOrDefault(x => x.Slot == EquipmentSlots.OffHand);
            //return Equipments.FirstOrDefault(x => x.Slot == EquipmentSlots.MainHand && x.Item == null) ?? Equipments.FirstOrDefault(x => x.Slot == EquipmentSlots.OffHand && x.Item == null);
        }

        protected EquipedItem SearchOffhandEquipmentSlot(bool replace)
        {
            // This leads to strange looking equipments:
            // wield 1-H weapon -> first main hand
            // wield 2-H weapon -> second main hand
            // hold shield -> second off hand (should be first off hand)
            // Return offhand only if related mainhand is not wielding 2H
            int countMainhand2H = Equipments.Count(x => x.Slot == EquipmentSlots.MainHand && x.Item?.WearLocation == WearLocations.Wield2H);
            if (replace)
                return Equipments.Where(x => x.Slot == EquipmentSlots.OffHand && x.Item == null).ElementAtOrDefault(countMainhand2H) ?? Equipments.Where(x => x.Slot == EquipmentSlots.OffHand).ElementAtOrDefault(countMainhand2H);
            return Equipments.Where(x => x.Slot == EquipmentSlots.OffHand && x.Item == null).ElementAtOrDefault(countMainhand2H);
        }

        protected bool ApplyDamageAndDisplayStatus(int damage)
        {
            // Apply damage
            bool dead = false;
            HitPoints -= damage;
            if (HitPoints < 1)
            {
                HitPoints = HitPointMinValue;
                dead = true;
            }

            //update_pos(victim);c
            //position_msg(victim, dam);

            // position_msg
            if (damage > MaxHitPoints/4)
                Send("That really did HURT!");
            if (!dead && HitPoints < MaxHitPoints/4)
                Send("You sure are BLEEDING!");
            if (dead)
            {
                Act(ActOptions.ToRoom, "{0} is dead.", this);
                Send("You have been KILLED!!");
            }

            return dead;
        }

        protected int ModifyDamage(int damage, int sourceLevel, SchoolTypes damageTypes, out bool fullyAbsorbed)
        {
            // TODO: perform crushing blow, critical, block computation here instead of OneHit and AbilityEffect
            // TODO: check combat_damage in fight.C:1940
            // TODO: damage reduction

            // TODO: if invisible, remove invisibility
            // TODO: damage modifier
            // TODO: check immunity/resist/vuln

            fullyAbsorbed = false;
            //// Check absorb
            //fullyAbsorbed = false;
            //if (damage > 0 && _auras.Any(x => x.Modifier == AuraModifiers.DamageAbsorb))
            //{
            //    bool needsRecompute = false;
            //    // Process every absorb aura until 0 damage left or 0 absorb aura left
            //    IReadOnlyCollection<IAura> absorbs = new ReadOnlyCollection<IAura>(_auras.Where(x => x.Modifier == AuraModifiers.DamageAbsorb).ToList());
            //    foreach (IAura absorb in absorbs)
            //    {
            //        // Process absorb
            //        damage = absorb.Absorb(damage);
            //        if (damage == 0) // full absorb
            //        {
            //            fullyAbsorbed = true;
            //            Log.Default.WriteLine(LogLevels.Debug, "Damage [{0}] totally absorbed by {1}", damage, absorb.Ability == null ? "<<??>>" : absorb.Ability.Name);
            //            break; // no need to check other absorb
            //        }
            //        else // partial absorb
            //        {
            //            Log.Default.WriteLine(LogLevels.Debug, "Damage [{0}] partially absorbed [{1}] by {2}", damage, absorb.Amount, absorb.Ability == null ? "<<??>>" : absorb.Ability.Name);
            //            needsRecompute = true;
            //            RemoveAura(absorb, false); // recompute when everything is done
            //        }
            //    }
            //    if (needsRecompute)
            //        Recompute();
            //}

            // Armor reduce physical damage (http://wow.gamepedia.com/Armor#Armor_damage_reduction_formula)
            if (damageTypes <= SchoolTypes.Slash) // Physical
            {
                // 1 -> 59
                //decimal damageReduction = (decimal)this[ComputedAttributeTypes.Armor] / (this[ComputedAttributeTypes.Armor] + 400 + 85 * sourceLevel);
                decimal denominator = _currentAttributes[(int)CharacterAttributes.ArmorBash]/*TODO other armor*/ + 400 + 85*sourceLevel;
                if (sourceLevel >= 60)
                    denominator += 4.5m*(sourceLevel - 59);
                if (sourceLevel >= 80)
                    denominator += 20*(sourceLevel - 80);
                if (sourceLevel >= 85)
                    denominator += 22*(sourceLevel - 85);
                decimal damageReduction = (decimal)_currentAttributes[(int)CharacterAttributes.ArmorBash]/ denominator;/*TODO other armor*/
                if (damageReduction > 0)
                {
                    //decimal damageAbsorption = HitPoints/(1.0m - damageReduction);
                    damage = damage - (int) (damage*damageReduction);
                }
            }

            // TODO: resistances (see http://wow.gamepedia.com/Resistance/

            return damage;
        }

        protected int ModifyHeal(int heal, out bool fullyAbsorbed)
        {
            fullyAbsorbed = false;
            //if (heal > 0 && _auras.Any(x => x.Modifier == AuraModifiers.HealAbsorb))
            //{
            //    bool needsRecompute = false;
            //    // Process every absorb aura until 0 damage left or 0 absorb aura left
            //    IReadOnlyCollection<IAura> absorbs = new ReadOnlyCollection<IAura>(_auras.Where(x => x.Modifier == AuraModifiers.HealAbsorb).ToList());
            //    foreach (IAura absorb in absorbs)
            //    {
            //        // Process absorb
            //        heal = absorb.Absorb(heal);
            //        if (heal == 0) // full absorb
            //        {
            //            fullyAbsorbed = true;
            //            Log.Default.WriteLine(LogLevels.Debug, "Heal [{0}] totally absorbed by {1}", heal, absorb.Ability == null ? "<<??>>" : absorb.Ability.Name);
            //            break; // no need to check other absorb
            //        }
            //        else // partial absorb
            //        {
            //            Log.Default.WriteLine(LogLevels.Debug, "Heal [{0}] partially absorbed [{1}] by {2}", heal, absorb.Amount, absorb.Ability == null ? "<<??>>" : absorb.Ability.Name);
            //            needsRecompute = true;
            //            RemoveAura(absorb, false); // recompute when everything is done
            //        }
            //    }
            //    if (needsRecompute)
            //        Recompute();
            //}

            return heal;
        }

        protected bool OneHit(ICharacter victim, IItemWeapon weapon, SchoolTypes damageType, bool notMainWield) // TODO: check fight.C:1394
        {
            if (this == victim || Room != victim.Room)
                return false;

            if (Position == Positions.Stunned)
                return false;

            // Starts fight if needed (if A attacks B, A fights B and B fights A)
            if (this != victim)
            {
                if (Fighting == null)
                    StartFighting(victim);
                if (victim.Fighting == null)
                    victim.StartFighting(this);
                // TODO: Cannot attack slave without breaking slavery
            }

            // http://wow.gamepedia.com/Attack_power
            int damage;
            if (weapon != null)
                damage = RandomManager.Dice(weapon.DiceCount, weapon.DiceValue) + _currentAttributes[(int)CharacterAttributes.DamRoll]; // TODO: use weapon dps and weapon speed
            else
            {
                // TEST
                damage = NoWeaponDamage;
            }
            if (notMainWield)
                damage /= 2;
            // TODO: damage modifier  fight.C:1693

            // Miss, dodge, parry, ...
            CombatHelpers.AttackResults attackResult = CombatHelpers.WhiteMeleeAttack(this, victim, notMainWield);
            Log.Default.WriteLine(LogLevels.Debug, $"{DebugName} -> {victim.DebugName} : attack result = {attackResult} for {weapon?.DebugName ?? "???"}");
            switch (attackResult)
            {
                case CombatHelpers.AttackResults.Miss:
                    victim.Act(ActOptions.ToCharacter, "{0} misses you.", this);
                    Act(ActOptions.ToCharacter, "You miss {0}.", victim);
                    return false;
                case CombatHelpers.AttackResults.Dodge:
                    victim.Act(ActOptions.ToCharacter, "You dodge {0}'s attack.", this);
                    Act(ActOptions.ToCharacter, "{0} dodges your attack.", victim);
                    return false;
                case CombatHelpers.AttackResults.Parry:
                    victim.Act(ActOptions.ToCharacter, "You parry {0}'s attack.", this);
                    Act(ActOptions.ToCharacter, "{0} parries your attack.", victim);
                    return false;
                case CombatHelpers.AttackResults.GlancingBlow:
                    // http://wow.gamepedia.com/Glancing_Blow
                    damage = (damage * 75) / 100;
                    break;
                case CombatHelpers.AttackResults.Block:
                    EquipedItem victimShield = victim.Equipments.FirstOrDefault(x => x.Item is IItemShield && x.Slot == EquipmentSlots.OffHand);
                    if (victimShield != null) // will never be null because MeleeAttack will not return Block if no shield
                    {
                        victim.Act(ActOptions.ToCharacter, "You block {0}'s attack with {1}.", this, victimShield.Item);
                        Act(ActOptions.ToCharacter, "{0} blocks your attack with {1}.", victim, victimShield.Item);
                    }
                    damage = (damage * 7) / 10;
                    break;
                case CombatHelpers.AttackResults.Critical:
                    damage = ModifyCriticalDamage(damage);
                    break;
                case CombatHelpers.AttackResults.CrushingBlow:
                    // http://wow.gamepedia.com/Crushing_Blow
                    damage = (damage * 150) / 200;
                    break;
                case CombatHelpers.AttackResults.Hit:
                    // NOP
                    break;
                default:
                    Log.Default.WriteLine(LogLevels.Error, "Unknown MeleeAttack result: {0}", attackResult);
                    break;
            }

            return victim.WeaponDamage(this, weapon, damage, damageType, true);
        }

        protected void DisplayDamageAbsorbPhrase(string name, IEntity source)
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                if (this == source)
                {
                    Act(ActOptions.ToAll, "{0:P} {1} is absorbed.", source, name);
                }
                else
                {
                    Act(ActOptions.ToAll, "{0:P} {1} is absorbed by {2}.", source, name, this);
                }
            }
            else
            {
                if (this == source)
                {
                    Act(ActOptions.ToAll, "{0:P} absorb{0:v} some damage.", this);
                }
                else
                {
                    Act(ActOptions.ToAll, "{0:P} absorb{0:v} damage from {1}", this, source);
                }
            }
        }

        protected void DisplayUnknownSourceAbsorbPhrase(string name)
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                Act(ActOptions.ToAll, "{0} absorb{0:v} damage from {1}.", this, name);
            }
            else
            {
                Act(ActOptions.ToAll, "{0} absorb{0:v} some damage.", this);
            }
        }

        protected void DisplayDamagePhrase(string name, int damage, IEntity source)
        {
            string damagePhraseSelf = StringHelpers.DamagePhraseSelf(damage);
            string damagePhraseOther = StringHelpers.DamagePhraseOther(damage);

            if (!string.IsNullOrWhiteSpace(name))
            {
                if (this == source)
                {
                    Act(ActOptions.ToAll, "{0:P} {1} {2} {0:f}.[dmg:{3}]", source, name, damagePhraseOther, damage);
                }
                else
                {
                    Act(ActOptions.ToCharacter, "{0}'s {1} {2} you.[dmg:{3}]", source, name, damagePhraseOther, damage);
                    if (source is ICharacter characterSource && Room == characterSource.Room)
                    {
                        characterSource.Act(ActOptions.ToCharacter, "Your {0} {1} {2}.[dmg:{3}]", name, damagePhraseSelf, this, damage);
                        ActToNotVictim(characterSource, "{0}'s {1} {2} {3}.[dmg:{4}]", source, name, damagePhraseOther, this, damage);
                    }
                    else
                        Act(ActOptions.ToRoom, "{0}'s {1} {2} {3}.[dmg:{4}]", source, name, damagePhraseOther, this, damage);
                    // TODO: damagePhraseOther and damagePhraseSelf should be merge and include {0:v}
                    //Act(ActOptions.ToAll, "{0:P} {1} {2} {3}.[{4}]", source, name, damagePhraseOther, this, damage);
                }
            }
            else
            {
                if (this == source)
                {
                    Act(ActOptions.ToCharacter, "You {0} yourself.[dmg:{1}]", damagePhraseSelf, damage);
                    Act(ActOptions.ToRoom, "{0} {1} {0:m}self.[dmg:{2}]", source, damagePhraseOther, damage);
                }
                else
                {
                    Act(ActOptions.ToCharacter, "{0} {1} you.[dmg:{2}]", source, damagePhraseOther, damage);
                    if (source is ICharacter characterSource && Room == characterSource.Room)
                    {
                        characterSource.Act(ActOptions.ToCharacter, "You {0} {1}.[dmg:{2}]", damagePhraseSelf, this, damage);
                        ActToNotVictim(characterSource, "{0} {1} {2}.[dmg:{3}]", source, damagePhraseOther, this, damage);
                    }
                    Act(ActOptions.ToRoom, "{0} {1} {2}.[dmg:{3}]", source, damagePhraseOther, this, damage);
                    // TODO: damagePhraseOther and damagePhraseSelf should be merge and include {0:v}
                }
            }
        }

        protected void DisplayUnknownSourceDamagePhrase(string name, int damage)
        {
            string damagePhraseSelf = StringHelpers.DamagePhraseSelf(damage);
            string damagePhraseOther = StringHelpers.DamagePhraseOther(damage);

            if (!string.IsNullOrWhiteSpace(name))
            {
                Act(ActOptions.ToCharacter, "{0} {1} you.[dmg:{2}]", name, damagePhraseSelf, damage);
                Act(ActOptions.ToRoom, "{0} {1} {2}.[dmg:{3}]", name, damagePhraseOther, this, damage);
                // TODO: damagePhraseOther and damagePhraseSelf should be merge and include {0:v}
            }
            else
            {
                Log.Default.WriteLine(LogLevels.Warning, "ICharacter.NonCombatDamage: no ability");
                Act(ActOptions.ToCharacter, "Something {0} you.[dmg:{1}]", damagePhraseOther, damage);
                Act(ActOptions.ToRoom, "Something {0} {1}.[dmg:{2}]", damagePhraseOther, this, damage);
                // TODO: damagePhraseOther and damagePhraseSelf should be merge and include {0:v}
            }
        }

        protected void DisplayHealPhrase(IAbility ability, int amount, IEntity source)
        {
            if (ability != null)
            {
                if (this == source)
                {
                    Act(ActOptions.ToCharacter, "Your {0} %W%heals%x% yourself.[heal:{1}]", ability, amount);
                    Act(ActOptions.ToRoom, "{0} {1} %W%heals%x% {0:m}self.[heal:{2}]", this, ability, amount);
                }
                else
                {
                    Act(ActOptions.ToCharacter, "{0}'s {1} %W%heals%x% you.[heal:{2}]", source, ability, amount);
                    if (source is ICharacter characterSource && Room == characterSource.Room)
                    {
                        characterSource.Act(ActOptions.ToCharacter, "Your {0} %W%heals%x% {1}.[heal:{2}]", ability, this, amount);
                        ActToNotVictim(characterSource, "{0}'s {1} %W%heals%x% {2}.[heal:{3}]", source, ability, this, amount);
                    }
                    else
                        Act(ActOptions.ToRoom, "{0}'s {1} %W%heals%x% {2}.[heal:{3}]", source, ability, this, amount);
                }
            }
            else
            {
                if (this == source)
                {
                    Act(ActOptions.ToCharacter, "You %W%heal%x% yourself.[heal:{0}]", amount);
                    Act(ActOptions.ToRoom, "{0} %W%heals%x% {0:m}self.[heal:{1}]", this, amount);
                }
                else
                {
                    Act(ActOptions.ToCharacter, "{0} heals you.[heal:{1}]", source, amount);
                    if (source is ICharacter characterSource && Room == characterSource.Room)
                    {
                        characterSource.Act(ActOptions.ToCharacter, "You heal {0}.[heal:{1}]", this, amount);
                        ActToNotVictim(characterSource, "{0} heals {1}.[heal:{2}]", source, this, amount);
                    }
                    else
                        Act(ActOptions.ToRoom, "{0} heals {1}.[heal:{2}]", source, this, amount);
                }
            }
        }

        protected void DisplayHealAbsorbPhrase(string name, IEntity source)
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                if (this == source)
                {
                    Act(ActOptions.ToCharacter, "Your {0} is absorbed.", name);
                    Act(ActOptions.ToRoom, "{0} {1} is absorbed.", source, name);
                }
                else
                {
                    Act(ActOptions.ToCharacter, "{0}'s {1} is absorbed.", source, name);
                    if (source is ICharacter characterSource && Room == characterSource.Room)
                    {
                        characterSource.Act(ActOptions.ToCharacter, "Your {0} is absorbed.", name);
                        ActToNotVictim(characterSource, "{0}'s {1} is absorbed by {2}.", source, name, this);
                    }
                    else
                        Act(ActOptions.ToRoom, "{0}'s {1} is absorbed by {2}.", source, name, this);
                }
            }
            else
            {
                if (this == source)
                {
                    Act(ActOptions.ToCharacter, "You absorb some heal.");
                    Act(ActOptions.ToRoom, "{0} absorbs some heal.", source);
                }
                else
                {
                    Act(ActOptions.ToCharacter, "You absorb heal from {0}.", source);
                    if (source is ICharacter characterSource && Room == characterSource.Room)
                    {
                        characterSource.Act(ActOptions.ToCharacter, "{0} absorbs your heal.", this);
                        ActToNotVictim(characterSource, "{0} absorbs heal from {1}.", this, source);
                    }
                    else
                        Act(ActOptions.ToRoom, "{0} absorbs heal from {1}.", this, source);
                }
            }
        }

        protected void DisplayUnknownSourceHealPhrase(IAbility ability, int amount)
        {
            if (ability != null)
            {
                Act(ActOptions.ToRoom, "{0} {1} %W%heals%x% {0:m}self.[heal:{2}]", this, ability, amount);
            }
            else
            {
                Act(ActOptions.ToRoom, "{0} %W%heals%x% {0:m}self.[heal:{1}]", this, amount);
            }
        }

        protected void DisplayUnknownSourceHealAbsorbPhrase(string name)
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                Act(ActOptions.ToRoom, "{0} is absorbed.", name);
            }
            else
            {
                Act(ActOptions.ToRoom, "Some heal are absorbed.");
            }
        }

        protected void RecomputeKnownAbilities()
        {
            // Add abilities from Class/Race/...

            // Admins know every abilities
            //if (ImpersonatedBy is IAdmin)
            //    _knownAbilities.AddRange(AbilityManager.Abilities.Select(x => new AbilityAndLevel(1,x)));
            //else
            if (Class != null)
                MergeAbilities(Class.Abilities, false);
            if (Race != null)
                MergeAbilities(Race.Abilities, true);
        }

        protected void RecomputeCurrentResourceKinds()
        {
            // Get current resource kind from class if any, every resource otherwise
            CurrentResourceKinds = (Class?.CurrentResourceKinds(Form) ?? EnumHelpers.GetValues<ResourceKinds>()).ToList();
        }

        protected int ComputeArmorFromEquipments()
        {
            int armorValue = 0;
            foreach (EquipedItem equipedItem in Equipments.Where(x => x.Item != null))
            {
                IItemArmor armor = equipedItem.Item as IItemArmor;
                if (armor != null)
                    armorValue += armor.Armor;
                else
                {
                    IItemShield shield = equipedItem.Item as IItemShield;
                    if (shield != null)
                        armorValue += shield.Armor;
                }
            }
            return armorValue;
        }

        protected bool GenericDamage(IEntity source, string sourceName /*can be source.DisplayName or source.Weapon.DisplayName*/, int damage, SchoolTypes damageType, bool visible)
        {
            if (!IsValid)
            {
                Log.Default.WriteLine(LogLevels.Error, "CombatDamage: {0} is not valid anymore", DebugName);
                return false;
            }

            ICharacter characterSource = source as ICharacter;
            // Starts fight if needed (if A attacks B, A fights B and B fights A)
            if (this != source)
            {
                if (characterSource != null)
                {
                    if (Fighting == null)
                        StartFighting(characterSource);
                    if (characterSource.Fighting == null)
                        characterSource.StartFighting(this);
                }
                // TODO: Cannot attack slave without breaking slavery
            }

            // Modify damage (resist, vuln, invul, absorb)
            bool fullyAbsorbed;
            damage = ModifyDamage(damage, characterSource?.Level ?? 0, damageType, out fullyAbsorbed);

            // Display damage
            if (visible) // equivalent to dam_message in fight.C:4381
            {
                if (fullyAbsorbed)
                    DisplayDamageAbsorbPhrase(sourceName, source);
                else
                    DisplayDamagePhrase(sourceName, damage, source);
            }

            // No damage -> stop here
            if (damage == 0)
            {
                Log.Default.WriteLine(LogLevels.Debug, "{0} does no damage to {1}", source.DebugName, DebugName);

                return false;
            }

            Log.Default.WriteLine(LogLevels.Debug, "{0} does {1} damage to {2}", source.DebugName, damage, DebugName);

            // Apply damage
            bool dead = ApplyDamageAndDisplayStatus(damage);

            Log.Default.WriteLine(LogLevels.Debug, "{0} HP: {1}", DebugName, HitPoints);

            // If dead, create corpse, xp gain/loss, remove character from world if needed
            if (dead) // TODO: fight.C:2246
            {
                Log.Default.WriteLine(LogLevels.Debug, "{0} has been killed by {1}", DebugName, source.DebugName);

                StopFighting(false);
                RawKilled(source, true);
                return true;
            }

            // TODO: wimpy, ... // fight.C:2264

            return true;
        }

        protected void ResetCurrentAttributes()
        {
            CharacterFlags = BaseCharacterFlags;
            Immunities = BaseImmunities;
            Resistances = BaseResistances;
            Vulnerabilities = BaseVulnerabilities;
            for (int i = 0; i < _baseAttributes.Length; i++)
                _currentAttributes[i] = _baseAttributes[i];
            Sex = BaseSex;
        }

        protected void SetMaxResource(ResourceKinds resourceKind, int value, bool checkCurrent)
        {
            int index = (int)resourceKind;
            if (index >= _maxResources.Length)
            {
                Log.Default.WriteLine(LogLevels.Error, "Trying to set max resource for resource {0} (index {1}) but max resource length is smaller", resourceKind, index);
                return;
            }
            _maxResources[index] = value;
            if (checkCurrent)
            {
                if (index >= _currentResources.Length)
                {
                    Log.Default.WriteLine(LogLevels.Error, "Trying to set current resource for resource {0} (index {1}) but current resource length is smaller", resourceKind, index);
                    return;
                }
                _currentResources[index] = Math.Min(_currentResources[index], _maxResources[index]);
            }
        }

        protected void SetBaseAttributes(CharacterAttributes attribute, int value, bool checkCurrent)
        {
            int index = (int)attribute;
            if (index >= _baseAttributes.Length)
            {
                Log.Default.WriteLine(LogLevels.Error, "Trying to set base attribute for attribute {0} (index {1}) but max attribute length is smaller", attribute, index);
                return;
            }
            _baseAttributes[index] = value;
            if (checkCurrent)
            {
                if (index >= _currentAttributes.Length)
                {
                    Log.Default.WriteLine(LogLevels.Error, "Trying to base current attribute for attribute {0} (index {1}) but current attribute length is smaller", attribute, index);
                    return;
                }
                _currentAttributes[index] = Math.Min(_currentAttributes[index], _baseAttributes[index]);
            }
        }

        protected void AddKnownAbility(KnownAbility knownAbility)
        {
            if (knownAbility.Ability != null && _knownAbilities.All(x => x.Ability?.Id != knownAbility.Ability.Id))
                _knownAbilities.Add(knownAbility);
        }

        private void ApplyAuras(IEntity entity)
        {
            if (!entity.IsValid)
                return;
            foreach (IAura aura in entity.Auras.Where(x => x.IsValid))
            {
                foreach (ICharacterAffect affect in aura.Affects.OfType<ICharacterAffect>())
                {
                    affect.Apply(this);
                }
            }
        }

        private void MergeAbilities(IEnumerable<AbilityUsage> abilities, bool naturalBorn)
        {
            // If multiple identical abilities, keep only one with lowest level
            foreach (AbilityUsage abilityUsage in abilities)
            {
                KnownAbility knownAbility = this[abilityUsage.Ability];
                if (knownAbility != null)
                {
                    Log.Default.WriteLine(LogLevels.Debug, "Merging KnownAbility with AbilityUsage for {0} Ability {1}", DebugName, abilityUsage.Ability.Name);
                    knownAbility.Level = Math.Min(knownAbility.Level, abilityUsage.Level);
                    knownAbility.Rating = Math.Min(knownAbility.Rating, abilityUsage.Rating);
                    knownAbility.CostAmount = Math.Min(knownAbility.CostAmount, abilityUsage.CostAmount);
                    // TODO: what should be we if multiple resource kind or operator ?
                }
                else
                {
                    Log.Default.WriteLine(LogLevels.Debug, "Adding KnownAbility from AbilityUsage for {0} Ability {1}", DebugName, abilityUsage.Ability.Name);
                    knownAbility = new KnownAbility
                    {
                        Ability = abilityUsage.Ability,
                        Level = abilityUsage.Level,
                        Learned = 0, // can be gained but not yet learned
                        ResourceKind = abilityUsage.ResourceKind,
                        CostAmount = abilityUsage.CostAmount,
                        CostAmountOperator = abilityUsage.CostAmountOperator,
                        Rating = abilityUsage.Rating
                    };
                    AddKnownAbility(knownAbility);
                }
                if (naturalBorn)
                {
                    knownAbility.Level = 1;
                    knownAbility.Rating = 1;
                    knownAbility.Learned = 100;
                }
            }
        }

        #region Act

        // Recreate behaviour of String.Format with maximum 10 arguments
        // If an argument is ICharacter, IItem, IExit special formatting is applied (depending on who'll receive the message)
        private enum ActParsingStates
        {
            Normal,
            OpeningBracketFound,
            ArgumentFound,
            FormatSeparatorFound,
        }

        private static string FormatActOneLine(ICharacter target, string format, params object[] arguments)
        {
            StringBuilder result = new StringBuilder();

            ActParsingStates state = ActParsingStates.Normal;
            object currentArgument = null;
            StringBuilder argumentFormat = null;
            foreach (char c in format)
            {
                switch (state)
                {
                    case ActParsingStates.Normal: // searching for {
                        if (c == '{')
                        {
                            state = ActParsingStates.OpeningBracketFound;
                            currentArgument = null;
                            argumentFormat = new StringBuilder();
                        }
                        else
                            result.Append(c);
                        break;
                    case ActParsingStates.OpeningBracketFound: // searching for a number
                        if (c == '{') // {{ -> {
                        {
                            result.Append('{');
                            state = ActParsingStates.Normal;
                        }
                        else if (c == '}') // {} -> nothing
                        {
                            state = ActParsingStates.Normal;
                        }
                        else if (c >= '0' && c <= '9') // {x -> argument found
                        {
                            currentArgument = arguments[c - '0'];
                            state = ActParsingStates.ArgumentFound;
                        }
                        break;
                    case ActParsingStates.ArgumentFound: // searching for } or :
                        if (c == '}')
                        {
                            FormatActOneArgument(target, result, null, currentArgument);
                            state = ActParsingStates.Normal;
                        }
                        else if (c == ':')
                            state = ActParsingStates.FormatSeparatorFound;
                        break;
                    case ActParsingStates.FormatSeparatorFound: // searching for }
                        if (c == '}')
                        {
                            Debug.Assert(argumentFormat != null);
                            FormatActOneArgument(target, result, argumentFormat.ToString(), currentArgument);
                            state = ActParsingStates.Normal;
                        }
                        else
                        {
                            // argumentFormat cannot be null
                            Debug.Assert(argumentFormat != null);
                            argumentFormat.Append(c);
                        }
                        break;
                }
            }
            if (result.Length > 0)
                result[0] = char.ToUpperInvariant(result[0]);
            return result.ToString();
        }

        // Formatting
        //  ICharacter
        //      default: same as n, N
        //      p, P: same as n but you is replaced with your
        //      n, N: argument.name if visible by target, someone otherwise
        //      e, E: you/he/she/it, depending on argument.sex
        //      m, M: you/him/her/it, depending on argument.sex
        //      s, S: your/his/her/its, depending on argument.sex
        //      f, F: yourself/himself/herself/itself
        //      b, B: are/is
        //      h, H: have/has
        //      v, V: add 's' at the end of a verb if argument is different than target (take care of verb ending with y/o/h
        // IItem
        //      argument.Name if visible by target, something otherwhise
        // IExit
        //      exit name
        // IAbility
        //      ability name
        private static void FormatActOneArgument(ICharacter target, StringBuilder result, string format, object argument)
        {
            // Character ?
            if (argument is ICharacter character)
            {
                char letter = format?[0] ?? 'n'; // if no format, n
                switch (letter)
                {
                    // your/name
                    case 'p':
                        if (target == character)
                            result.Append("your");
                        else
                        {
                            result.Append(character.RelativeDisplayName(target));
                            if (result[result.Length - 1] == 's')
                                result.Append('\'');
                            else
                                result.Append("'s");
                        }
                        break;
                    case 'P':
                        if (target == character)
                            result.Append("Your");
                        else
                        {
                            result.Append(character.RelativeDisplayName(target));
                            if (result[result.Length - 1] == 's')
                                result.Append('\'');
                            else
                                result.Append("'s");
                        }
                        break;
                    // you/name
                    case 'n':
                        if (target == character)
                            result.Append("you");
                        else
                            result.Append(character.RelativeDisplayName(target));
                        break;
                    case 'N':
                        if (target == character)
                            result.Append("You");
                        else
                            result.Append(character.RelativeDisplayName(target));
                        break;
                    // you/he/she/it
                    case 'e':
                        if (target == character)
                            result.Append("you");
                        else
                            result.Append(StringHelpers.Subjects[character.Sex]);
                        break;
                    case 'E':
                        if (target == character)
                            result.Append("You");
                        else
                            result.Append(StringHelpers.Subjects[character.Sex].UpperFirstLetter());
                        break;
                    // you/him/her/it
                    case 'm':
                        if (target == character)
                            result.Append("you");
                        else
                            result.Append(StringHelpers.Objectives[character.Sex]);
                        break;
                    case 'M':
                        if (target == character)
                            result.Append("You");
                        else
                            result.Append(StringHelpers.Objectives[character.Sex].UpperFirstLetter());
                        break;
                    // your/his/her/its
                    case 's':
                        if (target == character)
                            result.Append("your");
                        else
                            result.Append(StringHelpers.Possessives[character.Sex]);
                        break;
                    case 'S':
                        if (target == character)
                            result.Append("Your");
                        else
                            result.Append(StringHelpers.Possessives[character.Sex].UpperFirstLetter());
                        break;
                    // yourself/himself/herself/itself (almost same as 'm' + self)
                    case 'f':
                        if (target == character)
                            result.Append("your");
                        else
                            result.Append(StringHelpers.Objectives[character.Sex]);
                        result.Append("self");
                        break;
                    case 'F':
                        if (target == character)
                            result.Append("your");
                        else
                            result.Append(StringHelpers.Objectives[character.Sex].UpperFirstLetter());
                        result.Append("self");
                        break;
                    // is/are
                    case 'b':
                        if (target == character)
                            result.Append("are");
                        else
                            result.Append("is");
                        break;
                    case 'B':
                        if (target == character)
                            result.Append("Are");
                        else
                            result.Append("Is");
                        break;
                    // has/have
                    case 'h':
                        if (target == character)
                            result.Append("have");
                        else
                            result.Append("has");
                        break;
                    case 'H':
                        if (target == character)
                            result.Append("Have");
                        else
                            result.Append("Has");
                        break;
                    // verb
                    case 'v':
                    case 'V':
                        // nothing to do if target is actor
                        if (target != character)
                        {
                            //http://www.grammar.cl/Present/Verbs_Third_Person.htm
                            if (result.Length > 0)
                            {
                                char verbLastLetter = result[result.Length - 1];
                                char verbBeforeLastLetter = result.Length > 1 ? result[result.Length - 2] : ' ';
                                if (verbLastLetter == 'y' && verbBeforeLastLetter != ' ' && verbBeforeLastLetter != 'a' && verbBeforeLastLetter != 'e' && verbBeforeLastLetter != 'i' && verbBeforeLastLetter != 'o' && verbBeforeLastLetter != 'u')
                                {
                                    result.Remove(result.Length - 1, 1);
                                    result.Append("ies");
                                }
                                else if (verbLastLetter == 'o' || verbLastLetter == 'h' || verbLastLetter == 's' || verbLastLetter == 'x')
                                    result.Append("es");
                                else
                                    result.Append("s");
                            }
                            else
                                Log.Default.WriteLine(LogLevels.Error, "Act: v-format on an empty string");
                        }
                        break;
                    default:
                        Log.Default.WriteLine(LogLevels.Error, "Act: invalid format {0} for ICharacter", format);
                        result.Append("<???>");
                        break;
                }
                return;
            }
            // Item ?
            if (argument is IItem item)
            {
                // no specific format
                result.Append(item.RelativeDisplayName(target));
                return;
            }
            // Exit ?
            if (argument is IExit exit)
            {
                // no specific format
                result.Append(exit.Keywords.FirstOrDefault() ?? "door");
                return;
            }
            // Ability ?
            if (argument is IAbility ability)
            {
                // no specific format
                result.Append(ability.Name);
                return;
            }
            // Other (int, string, ...)
            if (format == null)
                result.Append(argument);
            else
            {
                if (argument is IFormattable formattable)
                    result.Append(formattable.ToString(format, null));
                else
                    result.Append(argument);
            }
        }

        #endregion

        private class CompareIAbility : IEqualityComparer<IAbility>
        {
            public bool Equals(IAbility x, IAbility y)
            {
                if (x == null && y == null)
                    return true;
                if (x == null || y == null)
                    return false;
                return x.Id == y.Id;
            }

            public int GetHashCode(IAbility obj)
            {
                if (obj == null)
                    return -1;
                else
                    return obj.Id;
            }
        }
    }
}
