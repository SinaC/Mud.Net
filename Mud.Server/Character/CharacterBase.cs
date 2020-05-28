using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Mud.Container;
using Mud.DataStructures.Trie;
using Mud.Domain;
using Mud.Logger;
using Mud.Server.Abilities;
using Mud.Server.Aura;
using Mud.Server.Blueprints.Character;
using Mud.Server.Blueprints.Item;
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
        private readonly List<EquippedItem> _equipments;
        // TODO: replace int[] with Dictionary<enum,int> ?
        private readonly int[] _baseAttributes;
        private readonly int[] _currentAttributes;
        private readonly int[] _maxResources;
        private readonly int[] _currentResources;
        private readonly Dictionary<IAbility, int> _cooldownsPulseLeft;
        private readonly List<KnownAbility> _knownAbilities;

        protected ITimeManager TimeManager => DependencyContainer.Current.GetInstance<ITimeManager>();
        protected IRandomManager RandomManager => DependencyContainer.Current.GetInstance<IRandomManager>();
        protected ITableValues TableValues => DependencyContainer.Current.GetInstance<ITableValues>();

        protected CharacterBase(Guid guid, string name, string description)
            : base(guid, name, description)
        {
            _inventory = new List<IItem>();
            _equipments = new List<EquippedItem>();
            _baseAttributes = new int[EnumHelpers.GetCount<CharacterAttributes>()];
            _currentAttributes = new int[EnumHelpers.GetCount<CharacterAttributes>()];
            _maxResources = new int[EnumHelpers.GetCount<ResourceKinds>()];
            _currentResources = new int[EnumHelpers.GetCount<ResourceKinds>()];
            _cooldownsPulseLeft = new Dictionary<IAbility, int>(new CompareIAbility());
            _knownAbilities = new List<KnownAbility>(); // handled by RecomputeKnownAbilities

            Position = Positions.Standing;
            Form = Forms.Normal;
        }

        #region ICharacter

        #region IEntity

        #region IActor

        public override IReadOnlyTrie<CommandMethodInfo> Commands => CharacterBaseCommands.Value;

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

        public override void OnRemoved()
        {
            base.OnRemoved();

            // Leave follower
            Leader?.RemoveFollower(this);

            // Release followers
            foreach (ICharacter follower in World.Characters.Where(x => x.Leader == this))
                RemoveFollower(follower);
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

        public IEnumerable<EquippedItem> Equipments => _equipments;
        public IEnumerable<IItem> Inventory => Content;
        public virtual int MaxCarryWeight => TableValues.CarryBonus(this) * 10 + Level * 25;
        public virtual int MaxCarryNumber => Equipments.Count() + 2 * this[BasicAttributes.Dexterity] + Level;
        public int CarryWeight => Inventory.Sum(x => x.Weight) + Equipments.Where(x => x.Item != null).Sum(x => x.Item.Weight);
        public int CarryNumber => Inventory.Sum(x => x.CarryCount) + Equipments.Where(x => x.Item != null).Sum(x => x.Item.CarryCount);

        // Money
        public long SilverCoins { get; protected set; }
        public long GoldCoins { get; protected set; }

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
        public int MaxHitPoints => _currentAttributes[(int)CharacterAttributes.MaxHitPoints];
        public int MovePoints { get; protected set; }
        public int MaxMovePoints => _currentAttributes[(int)CharacterAttributes.MaxMovePoints];

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

        public Sizes BaseSize { get; protected set; }
        public Sizes Size { get; protected set; }

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
                    Wiznet.Wiznet($"Trying to get current attribute for attribute {attribute} (index {index}) but current attribute length is smaller", WiznetFlags.Bugs, AdminLevels.Implementor);
                    return 0;
                }
                return _currentAttributes[index];
            }
            protected set 
            {
                int index = (int)attribute;
                if (index >= _currentAttributes.Length)
                {
                    Wiznet.Wiznet($"Trying to set current attribute for attribute {attribute} (index {index}) but current attribute length is smaller", WiznetFlags.Bugs, AdminLevels.Implementor);
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
                    Wiznet.Wiznet($"Trying to get current resource for resource {resource} (index {index}) but current resource length is smaller", WiznetFlags.Bugs, AdminLevels.Implementor);
                    return 0;
                }
                return _currentResources[index];
            }
            protected set
            {
                int index = (int)resource;
                if (index >= _currentResources.Length)
                {
                    Wiznet.Wiznet($"Trying to set current resource for resource {resource} (index {index}) but current resource length is smaller", WiznetFlags.Bugs, AdminLevels.Implementor);
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

        // Followers
        public ICharacter Leader { get; protected set; }

        public void AddFollower(ICharacter character)
        {
            if (character.Leader == this)
                return;
            // check if A->B->C->A
            ICharacter next = Leader;
            while (next != null)
            {
                if (next == character)
                    return; // found a cycle
                next = next.Leader;
            }

            character.Leader?.RemoveFollower(character);
            character.ChangeLeader(this);
            Act(ActOptions.ToCharacter, "{0:N} starts following you.", character);
            character.Act(ActOptions.ToCharacter, "You start following {0:N}.", this);
        }

        public void RemoveFollower(ICharacter character)
        {
            if (character.Leader != this)
                return;
            Act(ActOptions.ToCharacter, "{0:N} stops following you.", character);
            character.Act(ActOptions.ToCharacter, "You stop following {0:N}.", this);
            character.ChangeLeader(null);
            if (character is INonPlayableCharacter npcCharacter)
            {
                npcCharacter.RemoveBaseCharacterFlags(CharacterFlags.Charm);
                npcCharacter.RemoveAuras(x => x.Ability.Name == "Charm Person", true);
                npcCharacter.ChangeMaster(null);
            }
        }

        public void ChangeLeader(ICharacter character)
        {
            Leader = character;
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

        // Equipments
        public bool Unequip(IItem item)
        {
            if (item is IItemLight itemLight && itemLight.IsLighten)
                Room.DecreaseLight();
            foreach (EquippedItem equipmentSlot in _equipments.Where(x => x.Item == item))
                equipmentSlot.Item = null;
            return true;
        }

        public bool Equip(IItem item)
        {
            if (item is IItemLight itemLight && itemLight.IsLighten)
                Room.IncreaseLight();
            return true;
        }

        // Money
        public void UpdateMoney(long silverCoins, long goldCoins)
        {
            SilverCoins = Math.Max(0, SilverCoins + silverCoins);
            GoldCoins = Math.Max(0, GoldCoins + goldCoins);
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
        public bool CanSee(ICharacter victim)
        {
            if (victim == this)
                return true;
            // blind
            if (CharacterFlags.HasFlag(CharacterFlags.Blind))
                return false;
            // infrared + dark
            if (!CharacterFlags.HasFlag(CharacterFlags.Infrared) && Room.IsDark)
                return false;
            // invis
            if (victim.CharacterFlags.HasFlag(CharacterFlags.Invisible)
                && !CharacterFlags.HasFlag(CharacterFlags.DetectInvis))
                return false;
            // sneaking
            if (victim.CharacterFlags.HasFlag(CharacterFlags.Sneak)
                && !CharacterFlags.HasFlag(CharacterFlags.DetectHidden)
                && victim.Fighting == null)
            {
                var sneakInfo = victim.GetLearnInfo("Sneak"); // TODO: this can be quite slow and CanSee is often used
                int chance = sneakInfo.learned;
                chance += (3 * victim[BasicAttributes.Dexterity]) / 2;
                chance -= this[BasicAttributes.Intelligence] * 2;
                chance -= Level - (3* victim.Level)/ 2;

                if (!RandomManager.Chance(chance))
                    return false;
            }
            // hide
            if (victim.CharacterFlags.HasFlag(CharacterFlags.Hide)
                && !CharacterFlags.HasFlag(CharacterFlags.DetectHidden)
                && victim.Fighting == null)
                return false;
            //
            return true;
        }

        public virtual bool CanSee(IItem item)
        {

            // visible death
            if (item.ItemFlags.HasFlag(ItemFlags.VisibleDeath))
                return false;

            // blind except if potion
            if (CharacterFlags.HasFlag(CharacterFlags.Blind)
            //TODO can see potion
            )
                return false;

            // Light
            if (item is IItemLight light && light.IsLighten)
                return true;

            // invis
            if (item.ItemFlags.HasFlag(ItemFlags.Invis)
                && !CharacterFlags.HasFlag(CharacterFlags.DetectInvis))
                return false;

            // quest item
            IPlayableCharacter pc = this as IPlayableCharacter;
            if (item is IItemQuest questItem && (pc == null || !questItem.IsQuestObjective(pc)))
                return false;

            // glow
            if (item.ItemFlags.HasFlag(ItemFlags.Glowing))
                return true;

            // todo: check on dark room + infrared
            //if (room_is_dark(ch->in_room) && !IS_AFFECTED(ch, AFF_INFRARED))
            //    return FALSE;

            return true;
        }

        public bool CanSee(IExit exit)
        {
            return true; // TODO: Hidden
        }

        public bool CanSee(IRoom room)
        {
            // infrared + dark
            if (!CharacterFlags.HasFlag(CharacterFlags.Infrared) && room.IsDark)
                return false;
            //        if (IS_SET(pRoomIndex->room_flags, ROOM_IMP_ONLY)
            //&& get_trust(ch) < MAX_LEVEL)
            //            return FALSE;

            //        if (IS_SET(pRoomIndex->room_flags, ROOM_GODS_ONLY)
            //        && !IS_IMMORTAL(ch))
            //            return FALSE;

            //        if (IS_SET(pRoomIndex->room_flags, ROOM_HEROES_ONLY)
            //        && !IS_IMMORTAL(ch))
            //            return FALSE;

            if (room.RoomFlags.HasFlag(RoomFlags.NewbiesOnly) && Level > 5)
                return false;

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
                Wiznet.Wiznet($"Trying to get base attribute for attribute {attribute} (index {index}) but base attribute length is smaller", WiznetFlags.Bugs, AdminLevels.Implementor);
                return 0;
            }
            return _baseAttributes[index];
        }

        public int MaxResource(ResourceKinds resource)
        {
            int index = (int)resource;
            if (index >= _maxResources.Length)
            {
                Wiznet.Wiznet($"Trying to get max resource for resource {resource} (index {index}) but max resource length is smaller", WiznetFlags.Bugs, AdminLevels.Implementor);
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
            UpdatePosition();
        }

        public void UpdateMovePoints(int amount)
        {
            MovePoints = (MovePoints + amount).Range(0, MaxMovePoints);
        }

        public void UpdateAlignment(int amount) 
        {
            Alignment = (Alignment + amount).Range(MinAlignment, MaxAlignment);
            // impact on equipment
            bool recompute = false;
            foreach (var item in Equipments.Where(x => x.Item != null).Select(x => x.Item))
            {
                if ((item.ItemFlags.HasFlag(ItemFlags.AntiEvil) && IsEvil)
                    || (item.ItemFlags.HasFlag(ItemFlags.AntiGood) && IsGood)
                    || (item.ItemFlags.HasFlag(ItemFlags.AntiNeutral) && IsNeutral))
                {
                    Act(ActOptions.ToAll, "{0:N} {0:b} zapped by {1}.", this, item);
                    item.ChangeEquippedBy(null, false);
                    item.ChangeContainer(Room);
                    recompute = true;
                }
            }

            if (recompute)
            {
                Recompute();
                Room.Recompute();
            }
        }

        public void Regen()
        {
            // Hp/Move
            (int hitGain, int moveGain, int manaGain) gains = RegenBaseValues();
            int hitGain = gains.hitGain;
            int moveGain = gains.moveGain;
            int manaGain = gains.manaGain;

            // TODO: other resources
            hitGain = hitGain * Room.HealRate / 100;
            manaGain = manaGain * Room.ResourceRate / 100;

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
            Log.Default.WriteLine(LogLevels.Debug, "CharacterBase.Recompute: {0}", DebugName);

            // Reset current attributes
            ResetCurrentAttributes();

            // 1) Apply room auras
            if (Room != null)
                ApplyAuras(Room);

            // 2) Apply equipment auras
            foreach (EquippedItem equipment in Equipments.Where(x => x.Item != null))
                ApplyAuras(equipment.Item);

            // 3) Apply equipment armor
            foreach (EquippedItem equippedItem in Equipments.Where(x => x.Item is IItemArmor || x.Item is IItemShield))
            {
                if (equippedItem.Item is IItemArmor armor)
                {
                    int equipmentSlotMultiplier = TableValues.EquipmentSlotMultiplier(equippedItem.Slot);
                    this[CharacterAttributes.ArmorBash] -= armor.Bash * equipmentSlotMultiplier;
                    this[CharacterAttributes.ArmorPierce] -= armor.Pierce * equipmentSlotMultiplier;
                    this[CharacterAttributes.ArmorSlash] -= armor.Slash * equipmentSlotMultiplier;
                    this[CharacterAttributes.ArmorExotic] -= armor.Exotic * equipmentSlotMultiplier;
                }
                if (equippedItem.Item is IItemShield shield)
                {
                    int equipmentSlotMultiplier = TableValues.EquipmentSlotMultiplier(equippedItem.Slot);
                    this[CharacterAttributes.ArmorBash] -= shield.Armor * equipmentSlotMultiplier;
                    this[CharacterAttributes.ArmorPierce] -= shield.Armor * equipmentSlotMultiplier;
                    this[CharacterAttributes.ArmorSlash] -= shield.Armor * equipmentSlotMultiplier;
                    this[CharacterAttributes.ArmorExotic] -= shield.Armor * equipmentSlotMultiplier;
                }
            }

            // 4) Apply own auras
            ApplyAuras(this);

            // 5) Check if weapon can still be wielded
            if (this is IPlayableCharacter)
            {
                bool shouldRecompute = false;
                foreach (EquippedItem equipedItem in Equipments.Where(x => x.Slot == EquipmentSlots.MainHand && x.Item is IItemWeapon))
                {
                    if (equipedItem.Item is IItemWeapon weapon) // always true
                    {
                        if (this is IPlayableCharacter && weapon.TotalWeight > TableValues.WieldBonus(this) * 10) // TODO: same check in WearItem in ItemCommands.cs
                        {
                            Act(ActOptions.ToAll, "{0:N} can't use {1} anymore.", this, weapon);
                            weapon.ChangeEquippedBy(null, false);
                            weapon.ChangeContainer(this);
                            shouldRecompute = true;
                        }
                    }
                }
                if (shouldRecompute)
                    Recompute();
            }

            // Keep attributes in valid range
            HitPoints = Math.Min(HitPoints, MaxHitPoints);
            MovePoints = Math.Min(MovePoints, MaxMovePoints);
            for (int i = 0; i < _currentResources.Length; i++)
                _currentResources[i] = Math.Min(_currentResources[i], _maxResources[i]);
        }

        // Move
        public bool Move(ExitDirections direction, bool follow)
        {
            IRoom fromRoom = Room;

            //TODO exit flags such as climb, ...

            if (this is INonPlayableCharacter npc && npc.Master != null && npc.Master.Room == Room) // TODO: no more cast like this
            {
                // Slave cannot leave a room without Master
                Send("What?  And leave your beloved master?");
                return false;
            }

            // Under certain circumstances, direction can be modified (Drunk anyone?)
            direction = ChangeDirectionBeforeMove(direction, fromRoom);

            // Get exit and destination room
            IExit exit = fromRoom[direction];
            IRoom toRoom = exit?.Destination;

            // Check if existing exit
            if (exit == null || toRoom == null)
            {
                Send("You almost goes {0}, but suddenly realize that there's no exit there.", direction);
                Act(ActOptions.ToRoom, "{0} looks like {0:e}'s about to go {1}, but suddenly stops short and looks confused.", this, direction);
                return false;
            }
            // Closed ?
            if (exit.IsClosed && (!CharacterFlags.HasFlag(CharacterFlags.PassDoor) || exit.ExitFlags.HasFlag(ExitFlags.NoPass)))
            {
                Act(ActOptions.ToCharacter, "The {0} is closed.", exit);
                return false;
            }
            // Private ?
            if (toRoom.IsPrivate)
            {
                Send("That room is private right now.");
                return false;
            }
            // Size
            if (toRoom.MaxSize.HasValue && toRoom.MaxSize.Value < Size)
            {
                Send("You're too huge to go that direction.");
                return false;
            }
            // Flying
            if ((fromRoom.SectorType == SectorTypes.Air || toRoom.SectorType == SectorTypes.Air)
                && !CharacterFlags.HasFlag(CharacterFlags.Flying))
            {
                Send("You can't fly.");
                return false;
            }
            // Water
            if ((fromRoom.SectorType == SectorTypes.WaterSwim || toRoom.SectorType == SectorTypes.WaterSwim)
                && !CharacterFlags.HasFlag(CharacterFlags.Swim)
                && !CharacterFlags.HasFlag(CharacterFlags.Flying)
                && !Inventory.OfType<IItemBoat>().Any()) // TODO: WalkOnWater
            {
                Send("You need a boat to go there, or be swimming, flying or walking on water.");
                return false;
            }
            // Water no swim or underwater
            if ((fromRoom.SectorType == SectorTypes.WaterNoSwim || toRoom.SectorType == SectorTypes.WaterNoSwim)
                && !CharacterFlags.HasFlag(CharacterFlags.Flying)) // TODO: WalkOnWater
            {
                Send("You need to be flying or walking on water.");
                return false;
            }

            // Check move points left or drunk special phrase
            bool beforeMove = BeforeMove(direction, fromRoom, toRoom);
            if (!beforeMove)
                return false;

            //
            if (!CharacterFlags.HasFlag(CharacterFlags.Sneak))
                Act(ActOptions.ToRoom, "{0} leaves {1}.", this, direction);
            ChangeRoom(toRoom);

            // Display special phrase after entering room
            AfterMove(direction, fromRoom, toRoom);

            //
            if (!CharacterFlags.HasFlag(CharacterFlags.Sneak))
                Act(ActOptions.ToRoom, "{0} has arrived.", this);

            // Followers: no circular follows
            if (fromRoom != toRoom)
                MoveFollow(fromRoom, toRoom, direction);

            return true;
        }

        public bool Enter(IItemPortal portal, bool follow = false)
        {
            if (portal == null)
                return false;

            if (portal.PortalFlags.HasFlag(PortalFlags.Closed))
            {
                Send("You can't seem to find a way in.");
                return false;
            }

            if ((portal.PortalFlags.HasFlag(PortalFlags.NoCurse) && CharacterFlags.HasFlag(CharacterFlags.Curse))
                || Room.RoomFlags.HasFlag(RoomFlags.NoRecall))
            {
                Send("Something prevents you from leaving...");
                return false;
            }

            // Default destination is portal stored destination
            IRoom destination = portal.Destination;
            // Random portal will fix it's destination once used
            if (portal.PortalFlags.HasFlag(PortalFlags.Random) && portal.Destination == null)
            {
                destination = World.GetRandomRoom(this);
                portal.ChangeDestination(destination);
            }
            // Buggy portal has a low chance to lead somewhere else
            if (portal.PortalFlags.HasFlag(PortalFlags.Buggy) && RandomManager.Chance(5))
                destination = World.GetRandomRoom(this);

            if (destination == null
                || destination == Room
                || !CanSee(destination)
                || destination.IsPrivate
                || destination.RoomFlags.HasFlag(RoomFlags.Private))
            {
                Act(ActOptions.ToCharacter, "{0:N} doesn't seem to go anywhere.", portal);
                return false;
            }

            if (this is INonPlayableCharacter npc && npc.ActFlags.HasFlag(ActFlags.Aggressive) && destination.RoomFlags.HasFlag(RoomFlags.Law))
            {
                Send("Something prevents you from leaving...");
                return false;
            }

            if (destination.MaxSize.HasValue && destination.MaxSize.Value < Size)
            {
                Send("You're too huge to enter that.");
                return false;
            }

            IRoom wasRoom = Room;

            Act(ActOptions.ToRoom, "{0:N} steps into {1}.", this, portal);
            Act(ActOptions.ToCharacter, "You walk through {0} and find yourself somewhere else...", portal);

            ChangeRoom(destination);

            // take portal along
            if (portal.PortalFlags.HasFlag(PortalFlags.GoWith) && portal.ContainedInto is IRoom)
            {
                portal.ChangeContainer(Room);
            }

            if (AutomaticallyDisplayRoom)
            {
                StringBuilder sb = new StringBuilder();
                AppendRoom(sb, Room);
                Send(sb);
            }

            Act(ActOptions.ToRoom, "{0:N} arrived through {1}.", this, portal);

            // decrease charge left
            portal.Use();

            // if no charge left, destroy portal and no follow
            if (!portal.HasChargeLeft())
            {
                Act(ActOptions.ToCharacter, "{0:N} fades out of existence.", portal);
                if (portal.ContainedInto is IRoom portalInRoom && portalInRoom == Room)
                    Act(ActOptions.ToRoom, "{0:N} fades out of existence.", portal);
                if (wasRoom.People.Any())
                    Act(ActOptions.ToAll, "{0:N} fades out of existence.", portal);
                World.RemoveItem(portal);
                return true;
            }

            // Followers: no circular follows
            if (wasRoom != destination)
                EnterFollow(wasRoom, destination, portal);

            return true;
        }

        public void ChangeRoom(IRoom destination)
        {
            if (!IsValid)
            {
                Wiznet.Wiznet($"ICharacter.ChangeRoom: {DebugName} is not valid anymore", WiznetFlags.Bugs, AdminLevels.Implementor);
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
        public virtual void UpdatePosition()
        {
            if (HitPoints > 0)
            {
                if (Position <= Positions.Stunned)
                    Position = Positions.Standing;
                return;
            }
            if (HitPoints <= -11)
            {
                Position = Positions.Dead;
                return;
            }
            if (HitPoints <= -6)
                Position = Positions.Mortal;
            else if (HitPoints <= -3)
                Position = Positions.Incap;
            else
                Position = Positions.Stunned;
        }

        public bool StartFighting(ICharacter victim) // equivalent to set_fighting in fight.C:3441
        {
            if (!IsValid)
            {
                Wiznet.Wiznet($"StartFighting: {DebugName} is not valid anymore", WiznetFlags.Bugs, AdminLevels.Implementor);
                return false;
            }

            Log.Default.WriteLine(LogLevels.Debug, "{0} starts fighting {1}", DebugName, victim.DebugName);

            ChangePosition(Positions.Fighting);
            Fighting = victim;
            return true;
        }

        public bool StopFighting(bool both) // equivalent to stop_fighting in fight.C:3441
        {
            Log.Default.WriteLine(LogLevels.Debug, "{0} stops fighting {1}", Name, Fighting?.Name ?? "<<no victim>>");

            Fighting = null;
            ChangePosition(Positions.Standing);
            UpdatePosition();
            if (both)
            {
                foreach (ICharacter victim in World.Characters.Where(x => x.Fighting == this))
                    victim.StopFighting(false);
            }
            return true;
        }

        public void MultiHit(ICharacter victim) // 'this' starts a combat with 'victim'
        {
            MultiHit(victim, null);
        }

        public abstract void MultiHit(ICharacter victim, IMultiHitModifier multiHitModifier); // 'this' starts a combat with 'victim' and has been initiated by an ability

        public bool AbilityDamage(ICharacter source, IAbility ability, int damage, SchoolTypes damageType, bool display) // 'this' is dealt damage by 'source' using an ability
        {
            string damageNoun = ability?.DamageNoun?.ToLowerInvariant() ?? ability?.Name?.ToLowerInvariant() ?? "spell";
            return Damage(source, damage, damageType, damageNoun, display);
        }

        public bool HitDamage(ICharacter source, IItemWeapon wield, int damage, SchoolTypes damageType, bool display) // 'this' is dealt damage by 'source' using a weapon
        {
            string damageNoun;
            // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
            if (wield == null)
                damageNoun = NoWeaponDamageNoun;
            else
                damageNoun = wield.DamageNoun;
            if (string.IsNullOrWhiteSpace(damageNoun))
                damageNoun = "hit";

            return Damage(source, damage, damageType, damageNoun, display);
        }

        public bool Damage(ICharacter source, int damage, SchoolTypes damageType, string damageNoun, bool display) // 'this' is dealt damage by 'source'
        {
            if (Position == Positions.Dead)
                return false;
            // damage reduction
            if (damage > 35)
                damage = (damage - 35) / 2 + 35;
            if (damage > 80)
                damage = (damage - 80) / 2 + 80;

            if (this != source)
            {
                // Certain attacks are forbidden.
                // Most other attacks are returned.
                if (IsSafe(source))
                    return false;
                // TODO: check_killer
                if (Position > Positions.Stunned)
                {
                    if (Fighting == null)
                        StartFighting(source);
                    // TODO: if victim.Timer <= 4 -> victim.Position = Positions.Fighting
                }
                if (source.Position >= Positions.Stunned) // TODO: in original Rom code, test was done on victim (again)
                {
                    if (source.Fighting == null)
                        source.StartFighting(this);
                }
                // more charm stuff
                if (this is INonPlayableCharacter npcVictim && npcVictim.Master == source) // TODO: no more cast like this
                    npcVictim.ChangeMaster(null);
            }
            // inviso attack
            // TODO: remove invis, mass invis, flags, ... + "$n fades into existence."
            // damage modifiers
            if (damage > 1 && this is IPlayableCharacter pcVictim && pcVictim[Conditions.Drunk] > 10)
                damage -= damage / 10;
            if (damage > 1 && CharacterFlags.HasFlag(CharacterFlags.Sanctuary))
                damage /= 2;
            if (damage > 1
                && ((CharacterFlags.HasFlag(CharacterFlags.ProtectEvil) && source.IsEvil)
                    || (CharacterFlags.HasFlag(CharacterFlags.ProtectGood) && source.IsGood)))
                damage -= damage / 4;
            // old code was testing parry/dodge/shield block -> is done on OneHit
            ResistanceLevels resistanceLevel = CheckResistance(damageType);
            switch (resistanceLevel)
            {
                case ResistanceLevels.Immune:
                    damage = 0;
                    break;
                case ResistanceLevels.Resistant:
                    damage -= damage / 3;
                    break;
                case ResistanceLevels.Vulnerable:
                    damage += damage / 2;
                    break;
            }

            if (display)
            {
                // TODO: see dam_message
                string phraseOther; // {0}: source {1}: victim {2}: damage display {3}: damage noun {4}: damage value
                string phraseSource; // {0}: victim {1}: damage display {2}: damage noun {3}: damage value
                string phraseVictim = string.Empty; // {0}: source {1}: damage display {2}: damage noun {3}: damage value
                // build phrases
                if (string.IsNullOrWhiteSpace(damageNoun))
                {
                    if (this == source)
                    {
                        phraseOther = "{0:N} {2} {0:f}.[{4}]";
                        phraseSource = "You {1} yourself.[{3}]";
                    }
                    else
                    {
                        phraseOther = "{0:N} {2} {1}.[{4}]";
                        phraseSource = "You {1} {0}.[{3}]";
                        phraseVictim = "{0:N} {1} you.[{3}]";
                    }
                }
                else
                {
                    if (resistanceLevel == ResistanceLevels.Immune)
                    {
                        if (this == source)
                        {
                            phraseOther = "{0:N} is unaffected by {0:s} own {2}.";
                            phraseSource = "Luckily, you are immune to that.";
                        }
                        else
                        {
                            phraseOther = "{1:N} is unaffected by {0:p} {2}!";
                            phraseSource = "{0} is unaffected by your {1}!";
                            phraseVictim = "{0:p} {1} is powerless against you.";
                        }
                    }
                    else
                    {
                        if (this == source)
                        {
                            phraseOther = "{0:P} {3} {2} {0:m}.[{4}]";
                            phraseSource = "Your {2} {1} you.[{3}]";
                        }
                        else
                        {
                            phraseOther = "{0:P} {3} {2} {1}.[{4}]";
                            phraseSource = "Your {2} {1} {0}.[{3}]";
                            phraseVictim = "{0:P} {2} {1} you.[{3}]";
                        }
                    }
                }

                // display phrases
                string damagePhraseSelf = StringHelpers.DamagePhraseSelf(damage);
                string damagePhraseOther = StringHelpers.DamagePhraseOther(damage);
                if (this == source)
                {
                    source.Act(ActOptions.ToRoom, phraseOther, source, this, damagePhraseOther, damageNoun, damage);
                    source.Act(ActOptions.ToCharacter, phraseSource, this, damagePhraseSelf, damageNoun, damage);
                }
                else
                {
                    source.ActToNotVictim(this, phraseOther, source, this, damagePhraseOther, damageNoun, damage);
                    source.Act(ActOptions.ToCharacter, phraseSource, this, damagePhraseSelf, damageNoun, damage);
                    Act(ActOptions.ToCharacter, phraseVictim, source, damagePhraseOther, damageNoun, damage);
                }
            }

            // no damage done, stops here
            if (damage <= 0)
                return false;

            // hurt the victim
            HitPoints -= damage; // don't use UpdateHitPoints because value will not be allowed to go below 0
            // immortals don't really die TODO
            //if (victim is IPlayableCharacter
            //    && victim.HitPoints < 1)
            //    victim.UpdateHitPoints(1 - victim.HitPoints); // set to 1
            UpdatePosition();
            switch (Position)
            {
                case Positions.Mortal: Act(ActOptions.ToAll, "{0:N} {0:b} mortally wounded, and will die soon, if not aided.", this); break;
                case Positions.Incap: Act(ActOptions.ToAll, "{0:N} {0:b} incapacitated and will slowly die, if not aided.", this); break;
                case Positions.Stunned: Act(ActOptions.ToAll, "{0:N} {0:b} stunned, but will probably recover.", this); break;
                case Positions.Dead:
                    Send("You have been KILLED!!");
                    Act(ActOptions.ToRoom, "{0:N} is dead.", this);
                    break;
                default:
                    if (damage > MaxHitPoints / 4)
                        Send("That really did HURT!");
                    else if (HitPoints < MaxHitPoints / 4)
                        Send("You sure are BLEEDING!");
                    break;
            }

            // sleep spells or extremely wounded folks
            if (Position <= Positions.Sleeping)
                StopFighting(true); // StopFighting will set position to standing then UpdatePosition will set it again to Dead!!!

            // handle dead people
            if (Position == Positions.Dead)
            {
                IItemCorpse corpse = RawKilled(source, true); // group group_gain + dying penalty + raw_kill

                // TODO: autoloot, autosac  damage.C:96
                return true;
            }

            if (this == source)
                return true;

            // TODO: take care of link-dead people
            HandleWimpy(damage);

            return true;
        }

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
                    Wiznet.Wiznet($"CharacterBase.CheckResistance: Unknown {nameof(SchoolTypes)}.{damageType}", WiznetFlags.Bugs, AdminLevels.Implementor);
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

        public void Slay(IPlayableCharacter killer)
        {
            RawKilled(killer, false);
        }

        public abstract void KillingPayoff(ICharacter victim);

        public abstract void DeathPayoff(ICharacter killer);

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
                if (victim.Room.RoomFlags.HasFlag(RoomFlags.Safe))
                    return true;
                // TODO: No fight in a shop -> send_to_char("The shopkeeper wouldn't like that.\n\r",ch);
                if (npcVictim.ActFlags.HasFlag(ActFlags.Train)
                    || npcVictim.ActFlags.HasFlag(ActFlags.Gain)
                    || npcVictim.ActFlags.HasFlag(ActFlags.Practice)
                    || npcVictim.ActFlags.HasFlag(ActFlags.IsHealer)
                    || npcVictim.Blueprint is CharacterQuestorBlueprint)
                    return true;
                // Npc doing the killing
                if (caster is INonPlayableCharacter)
                {
                    // no pets
                    if (npcVictim.ActFlags.HasFlag(ActFlags.Pet))
                        return true;
                    // no charmed creatures unless owner
                    if (victim.CharacterFlags.HasFlag(CharacterFlags.Charm) && (area || caster != npcVictim.Master))
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
                    if (caster.CharacterFlags.HasFlag(CharacterFlags.Charm) && npcCaster.Master!= null && npcCaster.Master.Fighting != victim)
                        return true;
                    // safe room
                    if (victim.Room.RoomFlags.HasFlag(RoomFlags.Safe))
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

                    if (Level > victim.Level + 8)
                        return true;
                }
            }
            return false;
        }

        public bool IsSafe(ICharacter aggressor)
        {
            ICharacter victim = this;
            if (!victim.IsValid || victim.Room == null || !aggressor.IsValid || aggressor.Room == null)
                return true;
            if (victim.Fighting == aggressor || victim == aggressor)
                return false;
            if (aggressor is IPlayableCharacter pcCaster && pcCaster.ImpersonatedBy is IAdmin)
                return false;
            // Killing npc
            if (victim is INonPlayableCharacter npcVictim)
            {
                if (victim.Room.RoomFlags.HasFlag(RoomFlags.Safe))
                {
                    aggressor.Send("Not in the room.");
                    return true;
                }
                // TODO: No fight in a shop -> send_to_char("The shopkeeper wouldn't like that.\n\r",ch);
                if (npcVictim.ActFlags.HasFlag(ActFlags.Train)
                    || npcVictim.ActFlags.HasFlag(ActFlags.Gain)
                    || npcVictim.ActFlags.HasFlag(ActFlags.Practice)
                    || npcVictim.ActFlags.HasFlag(ActFlags.IsHealer)
                    || npcVictim.Blueprint is CharacterQuestorBlueprint)
                {
                    aggressor.Send("I don't think Mota would approve.");
                    return true;
                }

                // Player doing the killing
                if (aggressor is IPlayableCharacter)
                {
                    // no pets
                    if (npcVictim.ActFlags.HasFlag(ActFlags.Pet))
                    {
                        aggressor.Act(ActOptions.ToCharacter, "But {0} looks so cute and cuddly...", victim);
                        return true;
                    }
                    // no charmed creatures unless owner
                    if (victim.CharacterFlags.HasFlag(CharacterFlags.Charm) && aggressor != npcVictim.Master)
                    {
                        aggressor.Send("You don't own that monster.");
                        return true;
                    }
                }
            }
            // Killing player
            else
            {
                // Npc doing the killing
                if (aggressor is INonPlayableCharacter npcAggressor)
                {
                    // safe room
                    if (victim.Room.RoomFlags.HasFlag(RoomFlags.Safe))
                    {
                        aggressor.Send("Not in the room.");
                        return true;
                    }
                    // charmed mobs and pets cannot attack players while owned
                    if (aggressor.CharacterFlags.HasFlag(CharacterFlags.Charm) && npcAggressor.Master != null && npcAggressor.Master.Fighting != victim)
                    {
                        aggressor.Send("Players are your friends!");
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

                    if (Level > victim.Level + 8)
                    {
                        aggressor.Send("Pick on someone your own size.");
                        return true;
                    }
                }
            }
            return false;
        }

        // Abilities
        public abstract (int learned, KnownAbility knownAbility) GetWeaponLearnInfo(IItemWeapon weapon);

        public abstract (int learned, KnownAbility knownAbility) GetLearnInfo(IAbility ability);

        public (int learned, KnownAbility knownAbility) GetLearnInfo(string abilityName) 
        {
            IAbility ability = AbilityManager[abilityName];
            if (ability == null)
            {
                Wiznet.Wiznet($"GetLearned on unknown ability {abilityName}", WiznetFlags.Bugs, AdminLevels.Implementor);
                return (0, null);
            }
            return GetLearnInfo(ability);
        }

        public IDictionary<IAbility, int> AbilitiesInCooldown => _cooldownsPulseLeft;

        public bool HasAbilitiesInCooldown => _cooldownsPulseLeft.Any();

        public int CooldownPulseLeft(IAbility ability)
        {
            int pulseLeft;
            if (_cooldownsPulseLeft.TryGetValue(ability, out pulseLeft))
                return pulseLeft;
            return int.MinValue;
        }

        public void SetCooldown(IAbility ability)
        {
            _cooldownsPulseLeft[ability] = ability.Cooldown;
        }

        public bool DecreaseCooldown(IAbility ability, int pulseCount)
        {
            int pulseLeft;
            if (_cooldownsPulseLeft.TryGetValue(ability, out pulseLeft))
            {
                pulseLeft = Math.Max(0, pulseLeft - pulseCount);
                _cooldownsPulseLeft[ability] = pulseLeft;
                return pulseLeft == 0;
            }
            return false;
        }

        public void ResetCooldown(IAbility ability, bool verbose)
        {
            _cooldownsPulseLeft.Remove(ability);
            if (verbose)
                Send("%c%{0} is available.%x%", ability.Name);
        }

        // Equipment
        public IItem GetEquipment(EquipmentSlots slot) => Equipments.FirstOrDefault(x => x.Slot == slot && x.Item != null)?.Item;

        public T GetEquipment<T>(EquipmentSlots slot)
            where T : IItem => Equipments.Where(x => x.Slot == slot && x.Item is T).Select(x => x.Item).OfType<T>().FirstOrDefault();

        public EquippedItem SearchEquipmentSlot(IItem item, bool replace)
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
                    return SearchTwoHandedWeaponEquipmentSlot(replace);
                case WearLocations.Float:
                    return SearchEquipmentSlot(EquipmentSlots.Float, replace);
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
                        Wiznet.Wiznet($"Invalid AffectOperators {affect.Operator} for CharacterAttributeAffect Characteristics", WiznetFlags.Bugs, AdminLevels.Implementor);
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
                        _currentAttributes[(int)CharacterAttributes.ArmorExotic] += affect.Modifier;
                        break;
                    case AffectOperators.Assign:
                        _currentAttributes[(int)CharacterAttributes.ArmorBash] = affect.Modifier;
                        _currentAttributes[(int)CharacterAttributes.ArmorPierce] = affect.Modifier;
                        _currentAttributes[(int)CharacterAttributes.ArmorSlash] = affect.Modifier;
                        _currentAttributes[(int)CharacterAttributes.ArmorExotic] = affect.Modifier;
                        break;
                    case AffectOperators.Or:
                    case AffectOperators.Nor:
                        Wiznet.Wiznet($"Invalid AffectOperators {affect.Operator} for CharacterAttributeAffect AllArmor", WiznetFlags.Bugs, AdminLevels.Implementor);
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
                case CharacterAttributeAffectLocations.ArmorMagic: attribute = CharacterAttributes.ArmorExotic; break;
                default:
                    Wiznet.Wiznet($"CharacterBase.ApplyAffect: Unexpected CharacterAttributeAffectLocations {affect.Location}", WiznetFlags.Bugs, AdminLevels.Implementor);
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
                    Wiznet.Wiznet($"Invalid AffectOperators {affect.Operator} for CharacterAttributeAffect {affect.Location}", WiznetFlags.Bugs, AdminLevels.Implementor);
                    break;
            }
        }

        public void ApplyAffect(CharacterSexAffect affect)
        {
            Sex = affect.Value;
        }

        public void ApplyAffect(CharacterSizeAffect affect)
        {
            Size = affect.Value;
        }

        #endregion

        #region ActorBase

        protected override bool ExecuteBeforeCommand(CommandMethodInfo methodInfo, string rawParameters, params CommandParameter[] parameters)
        {
            // When hiding, anything will break it
            if (CharacterFlags.HasFlag(CharacterFlags.Hide))
            {
                RemoveBaseCharacterFlags(CharacterFlags.Hide);
                Recompute();
            }

            // Check minimum position
            if (methodInfo.Attribute is CharacterCommandAttribute characterCommandAttribute)
            {
                if (Position < characterCommandAttribute.MinPosition)
                {
                    switch (Position)
                    {
                        case Positions.Dead:
                            Send("Lie still; you are DEAD.");
                            return false;
                        case Positions.Mortal:
                        case Positions.Incap:
                            Send("You are hurt far too bad for that.");
                            return false;
                        case Positions.Stunned:
                            Send("You are too stunned to do that.");
                            return false;
                        case Positions.Sleeping:
                            Send("In your dreams, or what?");
                            return false;
                        case Positions.Resting:
                            Send("Nah... You feel too relaxed...");
                            return false;
                        case Positions.Sitting:
                            Send("Better stand up first.");
                            return false;
                        case Positions.Fighting:
                            Send("No way!  You are still fighting!");
                            return false;
                    }
                }
            }

            return base.ExecuteBeforeCommand(methodInfo, rawParameters, parameters);
        }

        #endregion

        protected abstract ExitDirections ChangeDirectionBeforeMove(ExitDirections direction, IRoom fromRoom);

        protected abstract bool BeforeMove(ExitDirections direction, IRoom fromRoom, IRoom toRoom);

        protected abstract void AfterMove(ExitDirections direction, IRoom fromRoom, IRoom toRoom);

        protected abstract (int hitGain, int moveGain, int manaGain) RegenBaseValues();

        protected virtual bool AutomaticallyDisplayRoom => IncarnatedBy != null;

        protected virtual void MoveFollow(IRoom fromRoom, IRoom toRoom, ExitDirections direction)
        {
            if (fromRoom != toRoom)
            {
                IReadOnlyCollection<ICharacter> followers = new ReadOnlyCollection<ICharacter>(fromRoom.People.Where(x => x.Leader == this && x.Position == Positions.Standing && x.CanSee(toRoom)).ToList()); // clone because Move will modify fromRoom.People
                foreach (ICharacter follower in followers)
                {
                    follower.Send("You follow {0}.", DebugName);
                    follower.Move(direction, true);
                }
            }
        }

        protected virtual void EnterFollow(IRoom wasRoom, IRoom destination, IItemPortal portal)
        {
            if (wasRoom == destination)
                return;
            // Followers will not automatically enter portal
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
                    Wiznet.Wiznet($"Act with invalid option: {option}", WiznetFlags.Bugs, AdminLevels.Implementor);
                    return Enumerable.Empty<ICharacter>();
            }
        }

        protected virtual IItemCorpse RawKilled(IEntity killer, bool payoff)
        {
            if (!IsValid)
            {
                Log.Default.WriteLine(LogLevels.Warning, "RawKilled: {0} is not valid anymore", DebugName);
                return null;
            }

            ICharacter characterKiller = killer as ICharacter;

            Wiznet.Wiznet($"{DebugName} got toasted by {killer?.DebugName ?? "???"} at {Room?.DebugName ?? "???"}", WiznetFlags.Deaths);

            StopFighting(true);
            // Remove periodic auras
            IReadOnlyCollection<IPeriodicAura> periodicAuras = new ReadOnlyCollection<IPeriodicAura>(PeriodicAuras.ToList()); // clone
            foreach (IPeriodicAura pa in periodicAuras)
                RemovePeriodicAura(pa);
            // Remove auras
            RemoveAuras(_ => true, false);

            // Death cry
            ActToNotVictim(this, "You hear {0}'s death cry.", this); // TODO: custom death cry

            // Gain/lose xp/reputation   damage.C:32   was done before removing auras and stop fighting
            if (payoff)
            {
                characterKiller?.KillingPayoff(this);
                DeathPayoff(characterKiller);
            }

            // Create corpse
            ItemCorpseBlueprint itemCorpseBlueprint = World.GetItemBlueprint<ItemCorpseBlueprint>(Settings.CorpseBlueprintId);
            IItemCorpse corpse = null;
            if (itemCorpseBlueprint != null)
            {
                if (characterKiller != null)
                    corpse = World.AddItemCorpse(Guid.NewGuid(), Room, this, characterKiller);
                else
                    corpse = World.AddItemCorpse(Guid.NewGuid(), Room, this);
            }
            else
            {
                Wiznet.Wiznet($"ItemCorpseBlueprint (id:{Settings.CorpseBlueprintId}) doesn't exist !!!", WiznetFlags.Bugs, AdminLevels.Implementor);
            }

            HandleDeath();

            return corpse;
        }

        protected abstract void HandleDeath();

        protected abstract void HandleWimpy(int damage);

        protected abstract (int thac0_00, int thac0_32) GetThac0();

        protected abstract SchoolTypes NoWeaponDamageType { get; }

        protected abstract int NoWeaponBaseDamage { get; }

        protected abstract string NoWeaponDamageNoun { get; }

        protected int GetWeaponBaseDamage(IItemWeapon weapon, int weaponLearned)
        {
            int damage = RandomManager.Dice(weapon.DiceCount, weapon.DiceValue) * weaponLearned / 100;
            if (GetEquipment<IItemShield>(EquipmentSlots.OffHand) == null) // no shield -> more damage
                damage = 11 * damage / 10;
            if (weapon.WeaponFlags.HasFlag(WeaponFlags.Sharp)) // sharpness
            {
                int percent = RandomManager.Range(1, 100);
                if (percent <= weaponLearned / 8)
                    damage = 2 * damage + (2 * damage * percent / 100);
            }

            return damage;
        }

        protected void OneHit(ICharacter victim, IItemWeapon wield, IHitModifier hitModifier) // 'this' hits 'victim' using hitModifier (optional, used only for backstab)
        {
            if (victim == this || victim == null)
                return;
            // can't beat a dead char!
            // guard against weird room-leavings.
            if (victim.Position == Positions.Dead || victim.Room != Room)
                return;
            SchoolTypes damageType = wield?.DamageType ?? NoWeaponDamageType;
            // get weapon skill
            var weaponLearnInfo = GetWeaponLearnInfo(wield);
            int learned = weaponLearnInfo.learned;
            // Calculate to-hit-armor-class-0 versus armor.
            (int thac0_00, int thac0_32) thac0Values = GetThac0();
            int thac0 = IntExtensions.Lerp(thac0Values.thac0_00, thac0Values.thac0_32, Level, 32);
            if (thac0 < 0)
                thac0 /= 2;
            if (thac0 < -5)
                thac0 = -5 + (thac0 + 5) / 2;
            thac0 -= HitRoll * learned / 100;
            thac0 += 5 * (100 - learned) / 100;
            if (hitModifier != null)
                thac0 = hitModifier.Thac0Modifier(thac0);
            int victimAc;
            switch (damageType)
            {
                case SchoolTypes.Bash:
                    victimAc = victim[Armors.Bash];
                    break;
                case SchoolTypes.Pierce:
                    victimAc = victim[Armors.Pierce];
                    break;
                case SchoolTypes.Slash:
                    victimAc = victim[Armors.Slash];
                    break;
                default:
                    victimAc = victim[Armors.Exotic];
                    break;
            }

            if (victimAc < -15)
                victimAc = -15 + (victimAc + 15) / 2;
            if (!CanSee(victim))
                victimAc -= 4;
            if (victim.Position < Positions.Fighting)
                victimAc += 4;
            if (victim.Position < Positions.Resting)
                victimAc += 6;
            // miss ?
            int diceroll = RandomManager.Range(0, 19); // 0->miss 19->success
            if (diceroll == 0
                || (diceroll != 19 && diceroll < thac0 - victimAc))
            {
                if (hitModifier?.Ability != null)
                    victim.AbilityDamage(this, hitModifier.Ability, 0, damageType, true);
                else
                    victim.HitDamage(this, wield, 0, damageType, true);
                return;
            }
            // TODO check parry/dodge/shield block (was in Damage function before)
            // TODO vorpal -> decapitate insta-kill (see fight.C:1642)
            // calculate weapon (or not) damage
            int damage = wield == null
                ? NoWeaponBaseDamage
                : GetWeaponBaseDamage(wield, learned);
            (this as IPlayableCharacter)?.CheckAbilityImprove(weaponLearnInfo.knownAbility, true, 5); // TODO: don't like this cast
            // bonus
            var enhancedDamageLearnInfo = GetLearnInfo("Enhanced damage");
            if (enhancedDamageLearnInfo.learned > 0)
            {
                int enhancedDamageDiceRoll = RandomManager.Range(1, 100);
                if (enhancedDamageDiceRoll <= enhancedDamageLearnInfo.learned)
                {
                    (this as IPlayableCharacter)?.CheckAbilityImprove(enhancedDamageLearnInfo.knownAbility, true, 6); // TODO: don't like this cast
                    damage += 2 * (damage * enhancedDamageDiceRoll) / 300;
                }
            }

            if (victim.Position <= Positions.Sleeping)
                damage *= 2;
            if (victim.Position <= Positions.Resting)
                damage = (damage * 3) / 2;
            if (hitModifier != null)
                damage = hitModifier.DamageModifier(wield, Level, damage);
            damage += DamRoll * learned / 100;
            if (damage <= 0)
                damage = 1; // at least one damage :)

            // perform damage
            bool damageResult;
            if (hitModifier?.Ability != null)
                damageResult = victim.AbilityDamage(this, hitModifier.Ability, damage, damageType, true);
            else
                damageResult = victim.HitDamage(this, wield, damage, damageType, true);
            if (Fighting != victim)
                return;

            // funky weapon ?
            if (damageResult && wield != null)
            {
                if (wield.WeaponFlags.HasFlag(WeaponFlags.Poison))
                {
                    IAbility poison = AbilityManager["Poison"];
                    IAura poisonAura = victim.GetAura(poison);
                    int level = poisonAura?.Level ?? wield.Level;
                    if (!victim.SavesSpell(level/2, SchoolTypes.Poison))
                    {
                        victim.Send("You feel poison coursing through your veins.");
                        victim.Act(ActOptions.ToRoom, "{0:N} is poisoned by the venom on {1}.", victim, wield);
                        if (poisonAura != null)
                        {
                            // weaken when already present
                            poisonAura.DecreaseLevel();
                            bool wornOff = poisonAura.DecreasePulseLeft(Pulse.FromMinutes(1));
                            if (poisonAura.Level < 0 || wornOff)
                            {
                                victim.RemoveAura(poisonAura, true); // in original code, the affect was not removed
                                victim.Act(ActOptions.ToCharacter, "The poison on {0} has worn off.", wield); // TODO: this could lead to strange behavior poisoned by food then worn off by weapon
                            }
                        }
                        else
                        {
                            int duration = level / 2;
                            World.AddAura(victim, poison, this, 3 * level / 4, TimeSpan.FromMinutes(duration), AuraFlags.None, false,
                                new CharacterFlagsAffect {Modifier = CharacterFlags.Poison, Operator = AffectOperators.Or},
                                new CharacterAttributeAffect {Location = CharacterAttributeAffectLocations.Strength, Modifier = -1, Operator = AffectOperators.Add});
                        }
                    }
                }

                if (Fighting != victim)
                    return;

                if (wield.WeaponFlags.HasFlag(WeaponFlags.Vampiric))
                {
                    int specialDamage = RandomManager.Range(1, 1 + wield.Level / 5);
                    victim.Act(ActOptions.ToRoom, "{0} draws life from {1}.", wield, victim);
                    victim.Act(ActOptions.ToCharacter, "You feel $p drawing your life away.", wield);
                    victim.Damage(this, specialDamage, SchoolTypes.Negative, null, false);
                    UpdateHitPoints(specialDamage/2);
                    UpdateAlignment(-1);
                }

                if (Fighting != victim)
                    return;

                if (wield.WeaponFlags.HasFlag(WeaponFlags.Flaming))
                {
                    int specialDamage = RandomManager.Range(1, 1 + wield.Level / 4);
                    victim.Act(ActOptions.ToRoom, "{0} is burned by {1}.", victim, wield);
                    victim.Act(ActOptions.ToCharacter, "{0} sears your flesh.", wield);
                    victim.Damage(this, specialDamage, SchoolTypes.Fire, null, false);
                    AbilityManager.FireEffect(victim, this, wield.Level/2, specialDamage);
                }

                if (Fighting != victim)
                    return;

                if (wield.WeaponFlags.HasFlag(WeaponFlags.Frost))
                {
                    int specialDamage = RandomManager.Range(1, 2 + wield.Level / 6);
                    victim.Act(ActOptions.ToRoom, "{0} freezes {1}.", wield, victim);
                    victim.Act(ActOptions.ToCharacter, "The cold touch of $p surrounds you with ice.", wield);
                    victim.Damage(this, specialDamage, SchoolTypes.Cold, null, false);
                    AbilityManager.ColdEffect(victim, this, wield.Level / 2, specialDamage);
                }

                if (Fighting != victim)
                    return;

                if (wield.WeaponFlags.HasFlag(WeaponFlags.Shocking))
                {
                    int specialDamage = RandomManager.Range(1, 2 + wield.Level / 5);
                    victim.Act(ActOptions.ToRoom, "{0:N} is struck by lightning from {1}.", victim, wield);
                    victim.Act(ActOptions.ToCharacter, "You are shocked by $p.", wield);
                    victim.Damage(this, specialDamage, SchoolTypes.Lightning, null, false);
                    AbilityManager.ShockEffect(victim, this, wield.Level / 2, specialDamage);
                }

                if (Fighting != victim)
                    return;
            }
        }

        protected KnownAbility this[IAbility ability] => _knownAbilities.SingleOrDefault(x => x.Ability == ability);

        protected void SetCooldown(IAbility ability, int pulseLeft)
        {
            _cooldownsPulseLeft[ability] = pulseLeft;
        }

        protected void ResetCooldowns()
        {
            _cooldownsPulseLeft.Clear();
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
                foreach (var item in _equipments.Where(x => x.Item != null).Select(x => x.Item))
                {
                    item.ChangeEquippedBy(null, false);
                    item.ChangeContainer(this);
                }

                _equipments.Clear();
                _equipments.AddRange(Race.EquipmentSlots.Select(x => new EquippedItem(x)));
                //Recompute();
            }
            else
            {
                _equipments.Add(new EquippedItem(EquipmentSlots.Light));
                _equipments.Add(new EquippedItem(EquipmentSlots.Head));
                _equipments.Add(new EquippedItem(EquipmentSlots.Amulet)); // 2 amulets
                _equipments.Add(new EquippedItem(EquipmentSlots.Amulet));
                _equipments.Add(new EquippedItem(EquipmentSlots.Chest));
                _equipments.Add(new EquippedItem(EquipmentSlots.Cloak));
                _equipments.Add(new EquippedItem(EquipmentSlots.Waist));
                _equipments.Add(new EquippedItem(EquipmentSlots.Wrists)); // 2 wrists
                _equipments.Add(new EquippedItem(EquipmentSlots.Wrists));
                _equipments.Add(new EquippedItem(EquipmentSlots.Arms));
                _equipments.Add(new EquippedItem(EquipmentSlots.Hands));
                _equipments.Add(new EquippedItem(EquipmentSlots.Ring)); // 2 rings
                _equipments.Add(new EquippedItem(EquipmentSlots.Ring));
                _equipments.Add(new EquippedItem(EquipmentSlots.Legs));
                _equipments.Add(new EquippedItem(EquipmentSlots.Feet));
                _equipments.Add(new EquippedItem(EquipmentSlots.MainHand)); // 2 hands
                _equipments.Add(new EquippedItem(EquipmentSlots.OffHand));
                _equipments.Add(new EquippedItem(EquipmentSlots.Float));
            }
        }

        protected EquippedItem SearchEquipmentSlot(EquipmentSlots equipmentSlot, bool replace)
        {
            if (replace) // search empty slot, if not found, return first matching slot
                return Equipments.FirstOrDefault(x => x.Slot == equipmentSlot && x.Item == null) ?? Equipments.FirstOrDefault(x => x.Slot == equipmentSlot);
            return Equipments.FirstOrDefault(x => x.Slot == equipmentSlot && x.Item == null);
        }

        protected EquippedItem SearchTwoHandedWeaponEquipmentSlot(bool replace)
        {
            // Search empty mainhand + empty offhand (no autoreplace) // TODO can wield 2H on one hand if size giant or specific ability
            var mainHand = Equipments.FirstOrDefault(x => x.Slot == EquipmentSlots.MainHand && x.Item == null);
            var offHand = Equipments.FirstOrDefault(x => x.Slot == EquipmentSlots.OffHand && x.Item == null);
            if (mainHand != null && offHand != null && mainHand.Item == null && offHand.Item == null)
                return mainHand;
            return null;
        }

        protected EquippedItem SearchOneHandedWeaponEquipmentSlot(bool replace)
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
        }

        protected EquippedItem SearchOffhandEquipmentSlot(bool replace)
        {
            // This can lead to strange looking equipments:
            // wield 1-H weapon -> first main hand
            // wield 2-H weapon -> second main hand
            // hold shield -> second off hand (should be first off hand)
            // Return offhand only if related mainhand is not wielding 2H
            int countMainhand2H = Equipments.Count(x => x.Slot == EquipmentSlots.MainHand && x.Item?.WearLocation == WearLocations.Wield2H);
            if (replace)
                return Equipments.Where(x => x.Slot == EquipmentSlots.OffHand && x.Item == null).ElementAtOrDefault(countMainhand2H) ?? Equipments.Where(x => x.Slot == EquipmentSlots.OffHand).ElementAtOrDefault(countMainhand2H);
            return Equipments.Where(x => x.Slot == EquipmentSlots.OffHand && x.Item == null).ElementAtOrDefault(countMainhand2H);
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

        protected void ResetCurrentAttributes()
        {
            CharacterFlags = BaseCharacterFlags;
            Immunities = BaseImmunities;
            Resistances = BaseResistances;
            Vulnerabilities = BaseVulnerabilities;
            for (int i = 0; i < _baseAttributes.Length; i++)
                _currentAttributes[i] = _baseAttributes[i];
            Sex = BaseSex;
            Size = BaseSize;
        }

        protected void SetMaxResource(ResourceKinds resourceKind, int value, bool checkCurrent)
        {
            int index = (int)resourceKind;
            if (index >= _maxResources.Length)
            {
                Wiznet.Wiznet($"Trying to set max resource for resource {resourceKind} (index {index}) but max resource length is smaller", WiznetFlags.Bugs, AdminLevels.Implementor);
                return;
            }
            _maxResources[index] = value;
            if (checkCurrent)
            {
                if (index >= _currentResources.Length)
                {
                    Wiznet.Wiznet($"Trying to set current resource for resource {resourceKind} (index {index}) but current resource length is smaller", WiznetFlags.Bugs, AdminLevels.Implementor);
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
                Wiznet.Wiznet($"Trying to set base attribute for attribute {attribute} (index {index}) but max attribute length is smaller", WiznetFlags.Bugs, AdminLevels.Implementor);
                return;
            }
            _baseAttributes[index] = value;
            if (checkCurrent)
            {
                if (index >= _currentAttributes.Length)
                {
                    Wiznet.Wiznet($"Trying to base current attribute for attribute {attribute} (index {index}) but current attribute length is smaller", WiznetFlags.Bugs, AdminLevels.Implementor);
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
                    //Log.Default.WriteLine(LogLevels.Debug, "Merging KnownAbility with AbilityUsage for {0} Ability {1}", DebugName, abilityUsage.Ability.Name);
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

            public int GetHashCode(IAbility obj) => obj.Id;
        }
    }
}
