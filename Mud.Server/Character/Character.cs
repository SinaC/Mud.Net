using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Mud.DataStructures.Trie;
using Mud.Logger;
using Mud.Server.Abilities;
using Mud.Server.Blueprints;
using Mud.Server.Constants;
using Mud.Server.Entity;
using Mud.Server.Helpers;
using Mud.Server.Input;
using Mud.Server.Item;
using Mud.Server.Server;

namespace Mud.Server.Character
{
    public partial class Character : EntityBase, ICharacter
    {
        private static readonly IReadOnlyTrie<CommandMethodInfo> CharacterCommands;

        private readonly List<IItem> _inventory;
        private readonly List<EquipedItem> _equipments;
        private readonly List<IPeriodicAura> _periodicAuras;
        private readonly List<IAura> _auras;
        private readonly int[] _basePrimaryAttributes; // modified when levelling
        private readonly int[] _currentPrimaryAttributes; // = base attribute + buff
        private readonly int[] _computedAttributes; // computed attributes (in recompute)
        private readonly int[] _maxResources;
        private readonly int[] _currentResources;
        private readonly List<ICharacter> _groupMembers;
        private readonly Dictionary<IAbility, DateTime> _cooldowns; // Key: ability.Id, Value: Next ability availability
        private readonly List<AbilityAndLevel> _knownAbilities;

        protected int MaxHitPoints => _computedAttributes[(int) ComputedAttributeTypes.MaxHitPoints];

        static Character()
        {
            CharacterCommands = CommandHelpers.GetCommands(typeof (Character));
        }

        public Character(Guid guid, string name, IClass pcClass, IRace pcRace, Sex pcSex, IRoom room) // PC
            : base(guid, name)
        {
            _inventory = new List<IItem>();
            _equipments = new List<EquipedItem>();
            _periodicAuras = new List<IPeriodicAura>();
            _auras = new List<IAura>();
            _basePrimaryAttributes = new int[EnumHelpers.GetCount<PrimaryAttributeTypes>()];
            _currentPrimaryAttributes = new int[EnumHelpers.GetCount<PrimaryAttributeTypes>()];
            _computedAttributes = new int[EnumHelpers.GetCount<ComputedAttributeTypes>()];
            _maxResources = new int[EnumHelpers.GetCount<ResourceKinds>()];
            _currentResources = new int[EnumHelpers.GetCount<ResourceKinds>()];
            _groupMembers = new List<ICharacter>();
            _cooldowns = new Dictionary<IAbility, DateTime>(new CompareIAbility());

            BuildEquipmentSlots();

            Class = pcClass;
            Race = pcRace;
            Sex = pcSex;
            Level = 30 + RandomizeHelpers.Instance.Randomizer.Next(-10, 10); // TODO: parameter
            for (int i = 0; i < _basePrimaryAttributes.Length; i++)
                _basePrimaryAttributes[i] = 150 + RandomizeHelpers.Instance.Randomizer.Next(-20,20);
            _knownAbilities = new List<AbilityAndLevel>(); // handled by RecomputeKnownAbilities

            Impersonable = true; // Playable
            Room = room;
            room.Enter(this);

            ResetAttributes();
            RecomputeKnownAbilities();
        }

        public Character(Guid guid, CharacterBlueprint blueprint, IRoom room) // NPC
            : base(guid, blueprint.Name, blueprint.Description)
        {
            _inventory = new List<IItem>();
            _equipments = new List<EquipedItem>();
            _periodicAuras = new List<IPeriodicAura>();
            _auras = new List<IAura>();
            _basePrimaryAttributes = new int[EnumHelpers.GetCount<PrimaryAttributeTypes>()]; // TODO: blueprint
            _currentPrimaryAttributes = new int[EnumHelpers.GetCount<PrimaryAttributeTypes>()]; // TODO: blueprint
            _computedAttributes = new int[EnumHelpers.GetCount<ComputedAttributeTypes>()];
            _maxResources = new int[EnumHelpers.GetCount<ResourceKinds>()];
            _currentResources = new int[EnumHelpers.GetCount<ResourceKinds>()];
            _groupMembers = new List<ICharacter>();
            _cooldowns = new Dictionary<IAbility, DateTime>(new CompareIAbility());

            Blueprint = blueprint;
            BuildEquipmentSlots();

            // TODO: mob class/race ???
            Sex = blueprint.Sex;
            Level = blueprint.Level;
            for (int i = 0; i < _basePrimaryAttributes.Length; i++)
                _basePrimaryAttributes[i] = 10;
            _knownAbilities = new List<AbilityAndLevel>(); // handled by RecomputeKnownAbilities

            Impersonable = false; // Non-playable
            Room = room;
            room.Enter(this);

            ResetAttributes();
            RecomputeKnownAbilities();
        }

        #region ICharacter

        #region IEntity

        #region IActor

        public override IReadOnlyTrie<CommandMethodInfo> Commands => CharacterCommands;

        public override void Send(string message)
        {
            // TODO: use Act formatter ?
            base.Send(message);
            if (ImpersonatedBy != null)
            {
                if (ServerOptions.PrefixForwardedMessages)
                    message = "<IMP|" + Name + ">" + message;
                ImpersonatedBy.Send(message);
            }
            // TODO: do we really need to receive message sent to slave ?
            if (ServerOptions.ForwardSlaveMessages && ControlledBy != null)
            {
                if (ServerOptions.PrefixForwardedMessages)
                    message = "<CTRL|" + Name + ">" + message;
                ControlledBy.Send(message);
            }
        }

        public override void Page(StringBuilder text)
        {
            base.Page(text);
            ImpersonatedBy?.Page(text);
            if (ServerOptions.ForwardSlaveMessages && ControlledBy != null)
                ControlledBy.Page(text);
        }

        #endregion

        public override string DisplayName => Blueprint == null ? StringHelpers.UpperFirstLetter(Name) : Blueprint.ShortDescription;

        public override void OnRemoved() // called before removing an item from the game
        {
            base.OnRemoved();

            StopFighting(true);
            Slave?.ChangeController(null);
            ImpersonatedBy = null; // TODO: warn ImpersonatedBy ?
            ControlledBy = null; // TODO: warn ControlledBy ?
            Leader = null; // TODO: warn Leader
            _inventory.Clear();
            _equipments.Clear();
            Blueprint = null;
            Room = null;
        }

        #endregion

        #region IContainer

        public IReadOnlyCollection<IItem> Content => _inventory.AsReadOnly();

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

        public CharacterBlueprint Blueprint { get; private set; }

        public IRoom Room { get; private set; }

        public ICharacter Fighting { get; private set; }

        public IReadOnlyCollection<EquipedItem> Equipments => _equipments.AsReadOnly();

        // Class/Race
        public IClass Class { get; }
        public IRace Race { get; }

        // Attributes
        public Sex Sex { get; }
        public int Level { get; }
        public int HitPoints { get; private set; }

        public int this[ResourceKinds resource]
        {
            get { return _currentResources[(int) resource]; }
            private set
            {
                _currentResources[(int) resource] = value;
            }
        }

        public int this[PrimaryAttributeTypes attribute]
        {
            get { return _currentPrimaryAttributes[(int) attribute]; }
            private set
            {
                _currentPrimaryAttributes[(int) attribute] = value;
            }
        }

        public int this[ComputedAttributeTypes attribute]
        {
            get { return _computedAttributes[(int) attribute]; }
            private set
            {
                _computedAttributes[(int) attribute] = value;
            }
        }

        public IReadOnlyCollection<AbilityAndLevel> KnownAbilities => _knownAbilities.AsReadOnly();

        // Periodic Auras
        public IReadOnlyCollection<IPeriodicAura> PeriodicAuras => _periodicAuras.AsReadOnly();

        public IReadOnlyCollection<IAura> Auras => _auras.AsReadOnly();

        public ICharacter Leader { get; private set; }

        public IReadOnlyCollection<ICharacter> GroupMembers => _groupMembers.AsReadOnly();

        // Impersonation/Controller
        public bool Impersonable { get; }
        public IPlayer ImpersonatedBy { get; private set; }

        public ICharacter Slave { get; private set; } // who is our slave (related to charm command/spell)
        public ICharacter ControlledBy { get; private set; } // who is our master (related to charm command/spell)

        // Group
        public bool ChangeLeader(ICharacter newLeader)
        {
            if (!IsValid)
            {
                Log.Default.WriteLine(LogLevels.Error, "ICharacter.ChangeLeader: {0} is not valid anymore", Name);
                return false;
            }
            if (newLeader != null && !newLeader.IsValid)
            {
                Log.Default.WriteLine(LogLevels.Error, "ICharacter.ChangeLeader: {0} is not valid anymore", newLeader.Name);
                return false;
            }
            Log.Default.WriteLine(LogLevels.Debug, "ICharacter.ChangeLeader: {0} old= {1}; new {2}", Name, Leader == null ? "<<none>>" : Leader.Name, newLeader == null ? "<<none>>" : newLeader.Name);
            Leader = newLeader;
            return true;
        }

        public bool AddGroupMember(ICharacter newMember)
        {
            if (!IsValid)
            {
                Log.Default.WriteLine(LogLevels.Error, "ICharacter.AddGroupMember: {0} is not valid anymore", Name);
                return false;
            }
            if (Leader != null)
            {
                Log.Default.WriteLine(LogLevels.Warning, "ICharacter.AddGroupMember: {0} cannot add member because leader is not null", Name);
                return false;
            }
            if (!newMember.IsValid)
            {
                Log.Default.WriteLine(LogLevels.Error, "ICharacter.AddGroupMember: new member {0} is not valid anymore");
                return false;
            }
            if (_groupMembers.Any(x => x == newMember))
            {
                Log.Default.WriteLine(LogLevels.Error, "ICharacter.AddGroupMember: {0} already in group of {1}", newMember.Name, Name);
                return false;
            }
            Log.Default.WriteLine(LogLevels.Debug, "ICharacter.AddGroupMember: {0} joined by {1}", Name, newMember.Name);
            // TODO: act to warn room ?
            Send("{0} joins group." + Environment.NewLine, newMember.DisplayName);
            newMember.ChangeLeader(this); // this is not mandatory (should be done by caller)
            foreach(ICharacter member in _groupMembers)
                member.Send("{0} joins group." + Environment.NewLine, newMember.DisplayName);
            _groupMembers.Add(newMember);
            return true;
        }

        public bool RemoveGroupMember(ICharacter oldMember)  // TODO: what if leader leaves group!!!
        {
            Log.Default.WriteLine(LogLevels.Debug, "ICharacter.RemoveGroupMember: {0} leaves {1}", oldMember.Name, Name);
            bool removed = _groupMembers.Remove(oldMember);
            if (!removed)
                Log.Default.WriteLine(LogLevels.Debug, "ICharacter.RemoveGroupMember: {0} not in group of {1}", oldMember.Name, Name);
            else
            {
                oldMember.ChangeLeader(null); // this is not mandatory (should be done by caller)
                Send("{0} leaves group." + Environment.NewLine, oldMember.DisplayName);
                // TODO: act to warn room ?
                foreach (ICharacter member in _groupMembers)
                    member.Send("{0} leaves group." + Environment.NewLine, member.DisplayName);
            }
            return true;
        }

        public bool AddFollower(ICharacter follower)
        {
            follower.ChangeLeader(this);
            if (CanSee(follower))
                Act(ActOptions.ToCharacter, "{0} now follows you.", follower);
            follower.Act(ActOptions.ToCharacter, "You now follow {0}.", this);
            return true;
        }

        public bool StopFollower(ICharacter follower)
        {
            if (follower.Leader == null)
            {
                Log.Default.WriteLine(LogLevels.Warning, "ICharacter:StopFollower: {0} is not following anyone", follower.DisplayName);
                return false;
            }
            if (follower.Leader != this)
            {
                Log.Default.WriteLine(LogLevels.Error, "ICharacter:StopFollower: {0} is not following {1} but {2}", follower.DisplayName, DisplayName, follower.Leader == null ? "<<none>>" : follower.Leader.DisplayName);
                return false;
            }
            follower.ChangeLeader(null);
            if (CanSee(follower))
                Act(ActOptions.ToCharacter, "{0} stops following you.", follower);
            follower.Act(ActOptions.ToCharacter, "You stop following {0}.", this);
            return true;
        }

        // Impersonation/Controller
        public bool ChangeImpersonation(IPlayer player) // if non-null, start impersonation, else, stop impersonation
        {
            if (!IsValid)
            {
                Log.Default.WriteLine(LogLevels.Error, "ICharacter.ChangeImpersonation: {0} is not valid anymore", Name);
                return false;
            }

            Log.Default.WriteLine(LogLevels.Debug, "ICharacter.ChangeImpersonation: {0} old: {1}; new {2}", Name, ImpersonatedBy == null ? "<<none>>" : ImpersonatedBy.Name, player == null ? "<<none>>" : player.Name);
            // TODO: check if not already impersonated, if impersonable, ...
            ImpersonatedBy = player;
            return true;
        }

        public bool ChangeController(ICharacter master) // if non-null, start slavery, else, stop slavery
        {
            if (!IsValid)
            {
                Log.Default.WriteLine(LogLevels.Error, "ICharacter.ChangeController: {0} is not valid anymore", Name);
                return false;
            }

            Log.Default.WriteLine(LogLevels.Debug, "ICharacter.ChangeController: {0} master: old: {1}; new {2}", Name, ControlledBy == null ? "<<none>>" : ControlledBy.Name, master == null ? "<<none>>" : master.Name);
            // TODO: check if already slave, ...
            if (master == null) // TODO: remove display ???
            {
                if (ControlledBy != null)
                {
                    Act(ActOptions.ToCharacter, "You stop following {0}.", ControlledBy);
                    ControlledBy.Act(ActOptions.ToCharacter, "{0} stops following you.", this);
                }
            }
            else
            {
                Act(ActOptions.ToCharacter, "You now follow {0}.", master);
                master.Act(ActOptions.ToCharacter, "{0} now follows you.", this);
            }
            ControlledBy = master;
            return true;
        }

        // Act
        // IFormattable cannot be used because formatting depends on who'll receive the message (CanSee check)
        public void Act(ActOptions options, string format, params object[] arguments)
        {
            if (options == ActOptions.ToAll || options == ActOptions.ToRoom)
                foreach (ICharacter to in Room.People)
                {
                    if (options == ActOptions.ToAll
                        || (options == ActOptions.ToRoom && to != this))
                    {
                        string phrase = FormatActOneLine(to, format, arguments);
                        to.Send(phrase);
                    }
                }
            else if (options == ActOptions.ToCharacter)
            {
                string phrase = FormatActOneLine(this, format, arguments);
                Send(phrase);
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
        public bool Unequip(IEquipable item)
        {
            foreach (EquipedItem equipmentSlot in _equipments.Where(x => x.Item == item))
                equipmentSlot.Item = null;
            RecomputeAttributes();
            return true;
        }

        // Visibility
        public bool CanSee(ICharacter character)
        {
            return true; // TODO
        }

        public bool CanSee(IItem obj)
        {
            return true; // TODO
        }

        // Attributes
        public int GetBasePrimaryAttribute(PrimaryAttributeTypes attribute)
        {
            return _basePrimaryAttributes[(int) attribute];
        }

        public int GetMaxResource(ResourceKinds resource)
        {
            return _maxResources[(int) resource];
        }

        public void SpendResource(ResourceKinds resource, int amount)
        {
            this[resource] = Math.Max(0, this[resource] - amount);
        }

        // Auras
        public void AddPeriodicAura(IPeriodicAura aura)
        {
            if (!IsValid)
            {
                Log.Default.WriteLine(LogLevels.Error, "ICharacter.AddPeriodicAura: {0} is not valid anymore", Name);
                return;
            }
            //IPeriodicAura same = _periodicAuras.FirstOrDefault(x => ReferenceEquals(x.Ability, aura.Ability) && x.AuraType == aura.AuraType && x.School == aura.School && x.Source == aura.Source);
            IPeriodicAura same = _periodicAuras.FirstOrDefault(x => x.Ability == aura.Ability && x.AuraType == aura.AuraType && x.School == aura.School && x.Source == aura.Source);
            if (same != null)
            {
                Log.Default.WriteLine(LogLevels.Info, "ICharacter.AddPeriodicAura: Refresh: {0} {1}", Name, aura.Ability == null ? "<<??>>" : aura.Ability.Name);
                same.Refresh(aura);
            }
            else
            {
                Log.Default.WriteLine(LogLevels.Info, "ICharacter.AddPeriodicAura: Add: {0} {1}", Name, aura.Ability == null ? "<<??>>" : aura.Ability.Name);
                _periodicAuras.Add(aura);
                if (aura.Ability == null || (aura.Ability.Flags & AbilityFlags.AuraIsHidden) != AbilityFlags.AuraIsHidden)
                    Send("You are now affected by {0}." + Environment.NewLine, aura.Ability == null ? "Something" : aura.Ability.Name);
                if (aura.Source != null && aura.Source != this)
                {
                    if (aura.Ability == null || (aura.Ability.Flags & AbilityFlags.AuraIsHidden) != AbilityFlags.AuraIsHidden)
                        aura.Source.Act(ActOptions.ToCharacter, "{0} is now affected by {1}", this, aura.Ability == null ? "Something" : aura.Ability.Name);
                    if (aura.AuraType == PeriodicAuraTypes.Damage)
                    {
                        if (Fighting == null)
                            StartFighting(aura.Source);
                        if (aura.Source.Fighting == null)
                            aura.Source.StartFighting(this);
                    }
                }
            }
        }

        public void RemovePeriodicAura(IPeriodicAura aura)
        {
            Log.Default.WriteLine(LogLevels.Info, "ICharacter.RemovePeriodicAura: {0} {1}", Name, aura.Ability == null ? "<<??>>" : aura.Ability.Name);
            bool removed = _periodicAuras.Remove(aura);
            if (!removed)
                Log.Default.WriteLine(LogLevels.Warning, "ICharacter.RemovePeriodicAura: Trying to remove unknown PeriodicAura");
            else
            {
                if (aura.Ability == null || (aura.Ability.Flags & AbilityFlags.AuraIsHidden) != AbilityFlags.AuraIsHidden)
                {
                    Send("{0} vanishes." + Environment.NewLine, aura.Ability == null ? "Something" : aura.Ability.Name);
                    if (aura.Source != null && aura.Source != this)
                        aura.Source.Act(ActOptions.ToCharacter, "{0} vanishes on {1}.", aura.Ability == null ? "Something" : aura.Ability.Name, this);
                }
                aura.ResetSource();
            }
        }

        public void AddAura(IAura aura, bool recompute)
        {
            if (!IsValid)
            {
                Log.Default.WriteLine(LogLevels.Error, "ICharacter.AddAura: {0} is not valid anymore", Name);
                return;
            }
            //IAura same = _auras.FirstOrDefault(x => ReferenceEquals(x.Ability, aura.Ability) && x.Modifier == aura.Modifier && x.Source == aura.Source);
            IAura same = _auras.FirstOrDefault(x => x.Ability == aura.Ability && x.Modifier == aura.Modifier && x.Source == aura.Source);
            if (same != null)
            {
                Log.Default.WriteLine(LogLevels.Info, "ICharacter.AddAura: Refresh: {0} {1}| recompute: {2}", Name, aura.Ability == null ? "<<??>>" : aura.Ability.Name, recompute);
                same.Refresh(aura);
            }
            else
            {
                Log.Default.WriteLine(LogLevels.Info, "ICharacter.AddAura: Add: {0} {1}| recompute: {2}", Name, aura.Ability == null ? "<<??>>" : aura.Ability.Name, recompute);
                _auras.Add(aura);
                if (aura.Ability == null || (aura.Ability.Flags & AbilityFlags.AuraIsHidden) != AbilityFlags.AuraIsHidden)
                    Send("You are now affected by {0}." + Environment.NewLine, aura.Ability == null ? "Something" : aura.Ability.Name);
            }
            if (recompute)
                RecomputeAttributes();
        }

        public void RemoveAura(IAura aura, bool recompute)
        {
            Log.Default.WriteLine(LogLevels.Info, "ICharacter.RemoveAura: {0} {1} | recompute: {2}", Name, aura.Ability == null ? "<<??>>" : aura.Ability.Name, recompute);
            bool removed = _auras.Remove(aura);
            if (!removed)
                Log.Default.WriteLine(LogLevels.Warning, "ICharacter.RemoveAura: Trying to remove unknown aura");
            else if (aura.Ability == null || (aura.Ability.Flags & AbilityFlags.AuraIsHidden) != AbilityFlags.AuraIsHidden)
                Send("{0} vanishes." + Environment.NewLine, aura.Ability == null ? "Something" : aura.Ability.Name);
            if (recompute && removed)
                RecomputeAttributes();
        }

        // Recompute
        public void ResetAttributes()
        {
            if (!IsValid)
            {
                Log.Default.WriteLine(LogLevels.Error, "ResetAttributes: {0} is not valid anymore", Name);
                return;
            }

            RecomputeAttributes();

            HitPoints = MaxHitPoints;
            foreach (ResourceKinds resource in EnumHelpers.GetValues<ResourceKinds>())
                this[resource] = _maxResources[(int)resource];
                //this[resource] = 10; // TEST
        }

        public void RecomputeAttributes()
        {
            // Reset current attributes
            for (int i = 0; i < _currentPrimaryAttributes.Length; i++)
                _currentPrimaryAttributes[i] = _basePrimaryAttributes[i];
            // Apply auras on primary attributes
            // TODO: aura from equipment/room/group
            foreach (IAura aura in Auras)
            {
                switch (aura.Modifier)
                {
                    case AuraModifiers.Strength:
                        ModifyPrimaryAttribute(PrimaryAttributeTypes.Strength, aura.AmountOperator, aura.Amount);
                        break;
                    case AuraModifiers.Agility:
                        ModifyPrimaryAttribute(PrimaryAttributeTypes.Agility, aura.AmountOperator, aura.Amount);
                        break;
                    case AuraModifiers.Stamina:
                        ModifyPrimaryAttribute(PrimaryAttributeTypes.Stamina, aura.AmountOperator, aura.Amount);
                        break;
                    case AuraModifiers.Intellect:
                        ModifyPrimaryAttribute(PrimaryAttributeTypes.Intellect, aura.AmountOperator, aura.Amount);
                        break;
                    case AuraModifiers.Spirit:
                        ModifyPrimaryAttribute(PrimaryAttributeTypes.Spirit, aura.AmountOperator, aura.Amount);
                        break;
                    case AuraModifiers.Characteristics:
                        ModifyPrimaryAttribute(PrimaryAttributeTypes.Strength, aura.AmountOperator, aura.Amount);
                        ModifyPrimaryAttribute(PrimaryAttributeTypes.Agility, aura.AmountOperator, aura.Amount);
                        ModifyPrimaryAttribute(PrimaryAttributeTypes.Stamina, aura.AmountOperator, aura.Amount);
                        ModifyPrimaryAttribute(PrimaryAttributeTypes.Intellect, aura.AmountOperator, aura.Amount);
                        ModifyPrimaryAttribute(PrimaryAttributeTypes.Spirit, aura.AmountOperator, aura.Amount);
                        break;
                }

            }
            // Recompute datas depending on attributes
            // TODO: correct formulas
            this[ComputedAttributeTypes.MaxHitPoints] = this[PrimaryAttributeTypes.Stamina] * 50 + 1000;
            this[ComputedAttributeTypes.AttackPower] = this[PrimaryAttributeTypes.Strength] * 2 + 50;
            this[ComputedAttributeTypes.SpellPower] = this[PrimaryAttributeTypes.Intellect] + 50;
            this[ComputedAttributeTypes.AttackSpeed] = 20;
            this[ComputedAttributeTypes.Armor] = ComputeArmorFromEquipments();
            //
            // Recompute max resources
            // TODO: correct values
            _maxResources[(int)ResourceKinds.Mana] = Level * 100;
            _maxResources[(int)ResourceKinds.Energy] = 100;
            _maxResources[(int)ResourceKinds.Rage] = 120;
            _maxResources[(int)ResourceKinds.Runic] = 130;
            // TODO: runes
            // Apply aura on compute attributes
            foreach (IAura aura in Auras)
            {
                switch (aura.Modifier)
                {
                    case AuraModifiers.MaxHitPoints:
                        ModifyComputedAttribute(ComputedAttributeTypes.MaxHitPoints, aura.AmountOperator, aura.Amount);
                        break;
                    case AuraModifiers.AttackPower:
                        ModifyComputedAttribute(ComputedAttributeTypes.AttackPower, aura.AmountOperator, aura.Amount);
                        break;
                    case AuraModifiers.SpellPower:
                        ModifyComputedAttribute(ComputedAttributeTypes.SpellPower, aura.AmountOperator, aura.Amount);
                        break;
                    case AuraModifiers.AttackSpeed:
                        ModifyComputedAttribute(ComputedAttributeTypes.AttackSpeed, aura.AmountOperator, aura.Amount);
                        break;
                    case AuraModifiers.Armor:
                        ModifyComputedAttribute(ComputedAttributeTypes.Armor, aura.AmountOperator, aura.Amount);
                        break;
                }
            }
            // Keep attributes in valid range
            HitPoints = Math.Min(HitPoints, MaxHitPoints);
        }

        // Move
        public bool Move(ExitDirections direction, bool follow = false)
        {
            IRoom fromRoom = Room;
            IExit exit = fromRoom.Exit(direction);
            IRoom toRoom = exit?.Destination;

            // TODO: act_move.C:133
            // cannot move while in combat -> should be handled by POSITION in command
            // drunk
            // exit flags such as climb, door closed, ...
            // private room, size, swim room, guild room

            if (ControlledBy != null && ControlledBy.Room == Room) // Slave cannot leave a room without Master
                Send("What?  And leave your beloved master?" + Environment.NewLine);
            else if (exit == null || toRoom == null) // Check if existing exit
            {
                Send("You almost goes {0}, but suddenly realize that there's no exit there." + Environment.NewLine, direction);
                Act(ActOptions.ToRoom, "{0} looks like {0:e}'s about to go {1}, but suddenly stops short and looks confused.", this, direction);
            }
            else
            {
                Act(ActOptions.ToRoom, "{0} leaves {1}.", this, direction); // TODO: sneak

                SetGlobalCooldown(1);
                ChangeRoom(toRoom);

                // Autolook if impersonated/incarnated
                if (ImpersonatedBy != null || IncarnatedBy != null)
                    DisplayRoom();

                Act(ActOptions.ToRoom, "{0} has arrived", this); // TODO: sneak

                // Followers: no circular follows
                if (fromRoom != toRoom)
                {
                    if (Slave != null)
                    {
                        Slave.Send("You follow {0}" + Environment.NewLine, DisplayName);
                        Slave.Move(direction, true);
                    }
                    IReadOnlyCollection<ICharacter> followers = new ReadOnlyCollection<ICharacter>(fromRoom.People.Where(x => x.Leader == this).ToList());
                    foreach (ICharacter follower in followers)
                    {
                        follower.Send("You follow {0}" + Environment.NewLine, DisplayName);
                        follower.Move(direction, true);
                    }
                }
            }
            return true;
        }

        public void ChangeRoom(IRoom destination)
        {
            if (!IsValid)
            {
                Log.Default.WriteLine(LogLevels.Error, "ICharacter.ChangeRoom: {0} is not valid anymore", Name);
                return;
            }

            Log.Default.WriteLine(LogLevels.Debug, "ICharacter.ChangeRoom: {0} from: {1} to {2}", Name, Room == null ? "<<no room>>" : Room.Name, destination == null ? "<<no room>>" : destination.Name);
            Room?.Leave(this);
            Room = destination;
            destination?.Enter(this);
        }

        // Combat
        public bool Heal(ICharacter source, IAbility ability, int amount, bool visible)
        {
            if (!IsValid)
            {
                Log.Default.WriteLine(LogLevels.Error, "ICharacter.Heal: {0} is not valid anymore", Name);
                return false;
            }

            Log.Default.WriteLine(LogLevels.Info, "{0} healed by {1} {2} for {3}", Name, source == null ? "<<??>>" : source.Name, ability == null ? "<<??>>" : ability.Name, amount);
            if (amount <= 0)
            {
                Log.Default.WriteLine(LogLevels.Warning, "ICharacter.Heal: invalid amount {0} on {1}", amount, Name);
                return false;
            }
            HitPoints = Math.Min(HitPoints + amount, MaxHitPoints);

            Log.Default.WriteLine(LogLevels.Debug, "{0} HP left: {1}", Name, HitPoints);

            // TODO: send message to this and source
            //if (!String.IsNullOrWhiteSpace(ability))
            //    Send("You are %w%healed%x% for {0} by {1} from {2}"+Environment.NewLine, amount, ability, source == null ? "(none)" : source.DisplayName);
            //else
            //    Send("You are %w%healed%x% for {0} from {2}" + Environment.NewLine, amount, source == null ? "(none)" : source.DisplayName);
            if (visible)
                DisplayHealPhrase(ability, amount, source);
            return true;
        }

        public bool MultiHit(ICharacter enemy)
        {
            if (!IsValid)
            {
                Log.Default.WriteLine(LogLevels.Error, "ICharacter.MultiHit: {0} is not valid anymore", Name);
                return false;
            }

            if (!IsValid)
                return false;

            Log.Default.WriteLine(LogLevels.Debug, "ICharacter.MultiHit: {0} -> {1}", Name, enemy.Name);

            if (this == enemy || Room != enemy.Room)
                return false;

            // TODO: more haste -> more attacks (should depend on weapon speed, attack speed, server speed[PulseViolence])

            // TEST purpose
            int attackCount = Math.Max(1, 1 + this[ComputedAttributeTypes.AttackSpeed] / 21);

            // Main hand
            for (int i = 0; i < attackCount; i++)
            {
                // Cannot store wielded between hit (disarm anyone ?)
                IItemWeapon wielded = (Equipments.FirstOrDefault(x => x.Slot == EquipmentSlots.Wield) ?? Equipments.FirstOrDefault(x => x.Slot == EquipmentSlots.Wield2H) ?? EquipedItem.NullObject).Item as IItemWeapon;
                SchoolTypes damageType = wielded?.DamageType ?? SchoolTypes.Physical;
                OneHit(enemy, wielded, damageType, false);

                if (Fighting != enemy) // stop multihit if different enemy or no enemy
                    return true;
            }

            // Off hand (if off-hand, loop is exited)
            for (int i = 0; i < attackCount; i++)
            {
                IItemWeapon wielded2 = (Equipments.FirstOrDefault(x => x.Slot == EquipmentSlots.Wield2) ?? EquipedItem.NullObject).Item as IItemWeapon; 
                if (wielded2 == null)
                    break;
                OneHit(enemy, wielded2, wielded2.DamageType, true);

                if (Fighting != enemy) // stop multihit if different enemy or no enemy
                    return true;
            }
            return true;
        }

        public bool StartFighting(ICharacter enemy) // equivalent to set_fighting in fight.C:3441
        {
            if (!IsValid)
            {
                Log.Default.WriteLine(LogLevels.Error, "StartFighting: {0} is not valid anymore", Name);
                return false;
            }

            Log.Default.WriteLine(LogLevels.Debug, "{0} starts fighting {1}", Name, enemy.Name);

            Fighting = enemy;
            return true;
        }

        public bool StopFighting(bool both) // equivalent to stop_fighting in fight.C:3441
        {
            Log.Default.WriteLine(LogLevels.Debug, "{0} stops fighting {1}", Name, Fighting == null ? "<<no enemy>>" : Fighting.Name);

            Fighting = null;
            // TODO: change/update pos
            if (both)
                foreach (ICharacter enemy in Repository.World.GetCharacters().Where(x => x.Fighting == this))
                {
                    enemy.StopFighting(false);
                    // TODO: change/update pos
                }
            return true;
        }

        public bool WeaponDamage(ICharacter source, IItemWeapon weapon, int damage, SchoolTypes damageType, bool visible) // damage from weapon(or bare hands) of known source
        {
            if (!IsValid)
            {
                Log.Default.WriteLine(LogLevels.Error, "CombatDamage: {0} is not valid anymore", Name);
                return false;
            }

            // Starts fight if needed (if A attacks B, A fights B and B fights A)
            if (this != source)
            {
                if (Fighting == null)
                    StartFighting(source);
                if (source.Fighting == null)
                    source.StartFighting(this);
                // TODO: Cannot attack slave without breaking slavery
            }

            // Modify damage (resist, vuln, invul, absorb)
            bool fullyAbsorbed;
            damage = ModifyDamage(damage, source.Level, damageType, out fullyAbsorbed);

            // Display damage
            if (visible) // equivalent to dam_message in fight.C:4381
            {
                if (fullyAbsorbed)
                    DisplayAbsorbPhrase(weapon?.DisplayName, source);
                else
                    DisplayDamagePhrase(weapon?.DisplayName, damage, source);
            }

            // No damage -> stop here
            if (damage == 0)
            {
                Log.Default.WriteLine(LogLevels.Debug, "{0} does no damage to {1}", source.Name, Name);

                return false;
            }

            Log.Default.WriteLine(LogLevels.Debug, "{0} does {1} damage to {2}", source.Name, damage, Name);

            // Apply damage
            bool dead = ApplyDamageAndDisplayStatus(damage);

            Log.Default.WriteLine(LogLevels.Debug, "{0} HP: {1}", Name, HitPoints);

            // If dead, create corpse, xp gain/loss, remove character from world if needed
            if (dead) // TODO: fight.C:2246
            {
                Log.Default.WriteLine(LogLevels.Debug, "{0} has been killed by {1}", Name, source.Name);

                StopFighting(false);
                RawKill(this, true);
                return true;
            }

            // TODO: wimpy, ... // fight.C:2264

            return true;
        }

        public bool Damage(ICharacter source, IAbility ability, int damage, SchoolTypes damageType, bool visible) // damage with known source
        {
            if (!IsValid)
            {
                Log.Default.WriteLine(LogLevels.Error, "CombatDamage: {0} is not valid anymore", Name);
                return false;
            }

            // Starts fight if needed (if A attacks B, A fights B and B fights A)
            if (this != source)
            {
                if (Fighting == null)
                    StartFighting(source);
                if (source.Fighting == null)
                    source.StartFighting(this);
                // TODO: Cannot attack slave without breaking slavery
            }

            // Modify damage (resist, vuln, invul, absorb)
            bool fullyAbsorbed;
            damage = ModifyDamage(damage, source.Level, damageType, out fullyAbsorbed);

            // Display damage
            if (visible) // equivalent to dam_message in fight.C:4381
            {
                if (fullyAbsorbed)
                    DisplayAbsorbPhrase(ability?.Name, source);
                else
                    DisplayDamagePhrase(ability?.Name, damage, source);
            }

            // No damage -> stop here
            if (damage == 0)
            {
                Log.Default.WriteLine(LogLevels.Debug, "{0} does no damage to {1}", source.Name, Name);

                return false;
            }

            Log.Default.WriteLine(LogLevels.Debug, "{0} does {1} damage to {2}", source.Name, damage, Name);

            // Apply damage
            bool dead = ApplyDamageAndDisplayStatus(damage);

            Log.Default.WriteLine(LogLevels.Debug, "{0} HP: {1}", Name, HitPoints);

            // If dead, create corpse, xp gain/loss, remove character from world if needed
            if (dead) // TODO: fight.C:2246
            {
                Log.Default.WriteLine(LogLevels.Debug, "{0} has been killed by {1}", Name, source.Name);

                StopFighting(false);
                RawKill(this, true);
                return true;
            }

            // TODO: wimpy, ... // fight.C:2264

            return true;
        }

        // TODO: damage reduction, damage modifier, immunity, absorb should be in a seperate method (same code un CombatSourceDamage)
        public bool UnknownSourceDamage(IAbility ability, int damage, SchoolTypes damageType, bool visible) // damage with unknown source or no source
        {
            if (!IsValid)
            {
                Log.Default.WriteLine(LogLevels.Error, "UnknownSourceDamage: {0} is not valid anymore", Name);
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
                Log.Default.WriteLine(LogLevels.Debug, "{0} does no damage to {1}", ability, Name);

                return false;
            }

            Log.Default.WriteLine(LogLevels.Debug, "{0} does {1} damage to {2}", ability, damage, Name);

            bool dead = ApplyDamageAndDisplayStatus(damage);

            Log.Default.WriteLine(LogLevels.Debug, "{0} HP left: {1}", Name, HitPoints);

            // If dead, create corpse, xp gain/loss, remove character from world if needed
            if (dead) // TODO: fight.C:2246
            {
                Log.Default.WriteLine(LogLevels.Debug, "{0} has been killed by {1}", Name, ability);

                StopFighting(false);
                RawKill(this, true);
                return true;
            }
            return true;
        }

        public bool RawKill(ICharacter victim, bool killingPayoff) // returns ItemCorpse
        {
            if (!IsValid)
            {
                Log.Default.WriteLine(LogLevels.Error, "RawKill: {0} is not valid anymore", Name);
                return false;
            }

            victim.StopFighting(true);
            // Remove periodic auras on victim
            List<IPeriodicAura> periodicAuras = new List<IPeriodicAura>(victim.PeriodicAuras); // clone
            foreach (IPeriodicAura pa in periodicAuras)
                victim.RemovePeriodicAura(pa);
            // Remove auras on victim
            List<IAura> auras = new List<IAura>(victim.Auras); // clone
            foreach (IAura aura in auras)
                victim.RemoveAura(aura, false);
            // no need to recompute

            // Death cry
            if (this != victim)
                Act(ActOptions.ToCharacter, "You hear {0}'s death cry.", victim);
            ActToNotVictim(victim, "You hear {0}'s death cry.", victim);

            // TODO: gain/lose xp/reputation   damage.C:32

            // Create corpse
            IItemCorpse corpse = Repository.World.AddItemCorpse(Guid.NewGuid(), ServerOptions.CorpseBlueprint, Room, victim);
            if (victim.ImpersonatedBy != null) // If impersonated, no real death
            {
                // TODO: teleport player to hall room/graveyard  see fight.C:3952
                victim.ResetAttributes();
            }
            else // If not impersonated, remove from game
            {
                Repository.World.RemoveCharacter(victim);
            }

            // TODO: autoloot, autosac  damage.C:96
            return true;
        }

        // Ability
        public IReadOnlyCollection<KeyValuePair<IAbility, DateTime>> AbilitiesInCooldown => _cooldowns.ToList().AsReadOnly();

        public bool HasAbilitiesInCooldown => _cooldowns.Any();

        public int CooldownSecondsLeft(IAbility ability)
        {
            DateTime nextAvailability;
            if (_cooldowns.TryGetValue(ability, out nextAvailability))
            {
                TimeSpan diff = nextAvailability - Repository.Server.CurrentTime;
                int secondsLeft = (int) Math.Ceiling(diff.TotalSeconds);
                return secondsLeft;
            }
            return Int32.MinValue;
        }

        public void SetCooldown(IAbility ability)
        {
            DateTime nextAvailability = Repository.Server.CurrentTime.AddSeconds(ability.Cooldown);
            _cooldowns[ability] = nextAvailability;
        }

        public void ResetCooldown(IAbility ability)
        {
            _cooldowns.Remove(ability);
            Send("{0} is available again."+Environment.NewLine, ability.Name);
        }

        #endregion

        protected void ModifyPrimaryAttribute(PrimaryAttributeTypes attribute, AmountOperators op, int amount)
        {
            if (op == AmountOperators.Percentage)
                amount = _basePrimaryAttributes[(int) attribute]*amount/100;
            this[attribute] += amount;
        }

        protected void ModifyComputedAttribute(ComputedAttributeTypes attribute, AmountOperators op, int amount)
        {
            if (op == AmountOperators.Percentage)
                amount = _computedAttributes[(int)attribute] * amount / 100;
            this[attribute] += amount;
        }

        protected int ModifyAttribute(int baseValue, AmountOperators op, int amount)
        {
            if (op == AmountOperators.Percentage)
                amount = baseValue*amount/100;
            return baseValue + amount;
        }

        protected void BuildEquipmentSlots()
        {
            // TODO: depend on race+affects+...
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
            _equipments.Add(new EquipedItem(EquipmentSlots.RingLeft));
            _equipments.Add(new EquipedItem(EquipmentSlots.RingRight));
            _equipments.Add(new EquipedItem(EquipmentSlots.Legs));
            _equipments.Add(new EquipedItem(EquipmentSlots.Feet));
            _equipments.Add(new EquipedItem(EquipmentSlots.Trinket1));
            _equipments.Add(new EquipedItem(EquipmentSlots.Trinket2));
            _equipments.Add(new EquipedItem(EquipmentSlots.Wield));
            _equipments.Add(new EquipedItem(EquipmentSlots.Wield2));
            _equipments.Add(new EquipedItem(EquipmentSlots.Shield));
            _equipments.Add(new EquipedItem(EquipmentSlots.Hold));
        }

        protected void SetGlobalCooldown(int pulseCount) // set GCD (in pulse) if impersonated by
        {
            ImpersonatedBy?.SetGlobalCooldown(pulseCount);
        }

        protected bool ApplyDamageAndDisplayStatus(int damage)
        {
            // Apply damage
            bool dead = false;
            HitPoints -= damage;
            if (HitPoints < 1)
            {
                if (ImpersonatedBy != null) // Impersonated character cannot be totally killed
                    HitPoints = 1;
                else
                    HitPoints = 0;
                dead = true;
            }

            //update_pos(victim);
            //position_msg(victim, dam);

            // position_msg
            if (damage > MaxHitPoints/4)
                Send("That really did HURT!" + Environment.NewLine);
            if (!dead && HitPoints < MaxHitPoints/4)
                Send("You sure are BLEEDING!" + Environment.NewLine);
            if (dead)
            {
                Act(ActOptions.ToRoom, "{0} is dead.", this);
                Send("You have been KILLED!!" + Environment.NewLine);
            }

            return dead;
        }

        protected int ModifyDamage(int damage, int sourceLevel, SchoolTypes damageTypes, out bool fullyAbsorbed)
        {
            // TODO: check combat_damage in fight.C:1940
            // TODO: damage reduction

            // TODO: if invisible, remove invisibility
            // TODO: damage modifier
            // TODO: check immunity/resist/vuln

            // Check absorb
            fullyAbsorbed = false;
            if (damage > 0 && _auras.Any(x => x.Modifier == AuraModifiers.Absorb))
            {
                bool needsRecompute = false;
                // Process every absorb aura until 0 damage left or 0 absorb aura left
                IReadOnlyCollection<IAura> absorbs = new ReadOnlyCollection<IAura>(_auras.Where(x => x.Modifier == AuraModifiers.Absorb).ToList());
                foreach (IAura absorb in absorbs)
                {
                    // Process absorb
                    damage = absorb.Absorb(damage);
                    if (damage == 0) // full absorb
                    {
                        fullyAbsorbed = true;
                        Log.Default.WriteLine(LogLevels.Debug, "Damage [{0}] totally absorbed by {1}", damage, absorb.Ability == null ? "<<??>>" : absorb.Ability.Name);
                        break; // no need to check other absorb
                    }
                    else // partial absorb
                    {
                        Log.Default.WriteLine(LogLevels.Debug, "Damage [{0}] partially absorbed [{1}] by {2}", damage, absorb.Amount, absorb.Ability == null ? "<<??>>" : absorb.Ability.Name);
                        needsRecompute = true;
                        RemoveAura(absorb, false); // recompute when everything is done
                    }
                }
                if (needsRecompute)
                    RecomputeAttributes();
            }

            // Armor reduce physical damage (http://wow.gamepedia.com/Armor#Armor_damage_reduction_formula)
            if (damageTypes == SchoolTypes.Physical)
            {
                // 1 -> 59
                //double damageReduction = (double)this[ComputedAttributeTypes.Armor] / (this[ComputedAttributeTypes.Armor] + 400 + 85 * sourceLevel);
                double denominator = this[ComputedAttributeTypes.Armor] + 400 + 85*sourceLevel;
                if (sourceLevel >= 60)
                    denominator += 4.5*(sourceLevel - 59);
                if (sourceLevel >= 80)
                    denominator += 20 * (sourceLevel - 80);
                if (sourceLevel >= 85)
                    denominator += 22 * (sourceLevel - 85);
                double damageReduction = this[ComputedAttributeTypes.Armor]/denominator;
                if (damageReduction > 0)
                {
                    //double damageAbsorption = HitPoints/(1.0 - damageReduction);
                    damage = damage - (int)(damage*damageReduction);
                }
            }

            // TODO: resistances (see http://wow.gamepedia.com/Resistance/

            return damage;
        }

        protected bool OneHit(ICharacter victim, IItemWeapon weapon, SchoolTypes damageType, bool dualWield) // TODO: check fight.C:1394
        {
            if (this == victim || Room != victim.Room)
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

            int damage;
            if (weapon != null)
                damage = RandomizeHelpers.Instance.Dice(weapon.DiceCount, weapon.DiceValue);
            else
            {
                // TEST
                if (ImpersonatedBy != null)
                    damage = 75;
                else
                    damage = 100;
            }
            // TODO: damage modifier  fight.C:1693

            // Miss, dodge, parry, ...
            CombatHelpers.AttackResults attackResult = CombatHelpers.MeleeAttack(this, victim, dualWield);
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
                    damage = (damage*80)/100; // TODO: http://wow.gamepedia.com/Glancing_Blow
                    break;
                case CombatHelpers.AttackResults.Block:
                    EquipedItem victimShield = victim.Equipments.FirstOrDefault(x => x.Item != null && x.Slot == EquipmentSlots.Shield);
                    if (victimShield != null) // will never be null because MeleeAttack will not return Block if no shield
                    {
                        victim.Act(ActOptions.ToCharacter, "You block {0}'s attack with {1}.", this, victimShield.Item);
                        Act(ActOptions.ToCharacter, "{0} blocks your attack with {1}.", victim, victimShield.Item);
                    }
                    damage = (damage*7)/10;
                    break;
                case CombatHelpers.AttackResults.Critical:
                    damage *= 2; // TODO: http://wow.gamepedia.com/Critical_strike
                    break;
                case CombatHelpers.AttackResults.CrushingBlow:
                    damage = (damage*150)/200; // TODO: http://wow.gamepedia.com/Crushing_Blow
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

        protected void DisplayAbsorbPhrase(string name, ICharacter source)
        {
            if (!String.IsNullOrWhiteSpace(name))
            {
                if (this == source)
                {
                    Act(ActOptions.ToCharacter, "Your {0} is absorbed.", name);
                    Act(ActOptions.ToRoom, "{0} {1} is absorbed.", source, name);
                }
                else
                {
                    Act(ActOptions.ToCharacter, "{0}'s {1} is absorbed.", source, name);
                    if (Room == source.Room)
                        source.Act(ActOptions.ToCharacter, "Your {0} is absorbed.", name);
                    ActToNotVictim(source, "{0}'s {1} is absorbed by {2}.", source, name, this);
                }
            }
            else
            {
                if (this == source)
                {
                    Act(ActOptions.ToCharacter, "You absorb some damage.");
                    Act(ActOptions.ToRoom, "{0} absorbs some damage.", source);
                }
                else
                {
                    Act(ActOptions.ToCharacter, "You absorb damage from {0}.", source);
                    if (Room == source.Room)
                        source.Act(ActOptions.ToCharacter, "{0} absorbs your damage.", this);
                    ActToNotVictim(source, "{0} absorbs damage from {0}.", this, source);
                }
            }
        }

        protected void DisplayUnknownSourceAbsorbPhrase(string name)
        {
            if (!String.IsNullOrWhiteSpace(name))
            {
                Act(ActOptions.ToCharacter, "{0} is absorbed.", name);
                Act(ActOptions.ToRoom, "{0} is absorbed by {1}.", name, this);
            }
            else
            {
                Act(ActOptions.ToCharacter, "You absorb damage.");
                Act(ActOptions.ToRoom, "{0} absorbs damage.", this);
            }
        }

        protected void DisplayDamagePhrase(string name, int damage, ICharacter source)
        {
            string damagePhraseSelf = StringHelpers.DamagePhraseSelf(damage);
            string damagePhraseOther = StringHelpers.DamagePhraseOther(damage);

            if (!String.IsNullOrWhiteSpace(name))
            {
                if (this == source)
                {
                    Act(ActOptions.ToCharacter, "Your {0} {1} yourself.[{2}]", name, damagePhraseOther, damage);
                    Act(ActOptions.ToRoom, "{0} {1} {2} {0:m}self.[{3}]", source, name, damagePhraseOther, damage);
                }
                else
                {
                    Act(ActOptions.ToCharacter, "{0}'s {1} {2} you.[{3}]", source, name, damagePhraseOther, damage);
                    if (Room == source.Room)
                        source.Act(ActOptions.ToCharacter, "Your {0} {1} {2}.[{3}]", name, damagePhraseOther, this, damage);
                    ActToNotVictim(source, "{0}'s {1} {2} {3}.[{4}]", source, name, damagePhraseOther, this, damage);
                }
            }
            else
            {
                if (this == source)
                {
                    Act(ActOptions.ToCharacter, "You {0} yourself.[{1}]", damagePhraseSelf, damage);
                    Act(ActOptions.ToRoom, "{0} {1} {0:m}self.[{2}]", source, damagePhraseOther, damage);
                }
                else
                {
                    Act(ActOptions.ToCharacter, "{0} {1} you.[{2}]", source, damagePhraseOther, damage);
                    if (Room == source.Room)
                        source.Act(ActOptions.ToCharacter, "You {0} {1}.[{2}]", damagePhraseSelf, this, damage);
                    ActToNotVictim(source, "{0} {1} {2}.[{3}]", source, damagePhraseOther, this, damage);
                }
            }
        }

        protected void DisplayUnknownSourceDamagePhrase(string name, int damage)
        {
            string damagePhraseSelf = StringHelpers.DamagePhraseSelf(damage);
            string damagePhraseOther = StringHelpers.DamagePhraseOther(damage);

            if (!String.IsNullOrWhiteSpace(name))
            {
                Act(ActOptions.ToCharacter, "{0} {1} you.[{2}]", name, damagePhraseSelf, damage);
                Act(ActOptions.ToRoom, "{0} {1} {2}.[{3}]", name, damagePhraseOther, this, damage);
            }
            else
            {
                Log.Default.WriteLine(LogLevels.Warning, "ICharacter.NonCombatDamage: no ability");
                Act(ActOptions.ToCharacter, "Something {0} you.[{1}]", damagePhraseOther, damage);
                Act(ActOptions.ToRoom, "Something {0} {1}.[{2}]", damagePhraseOther, this, damage);
            }
        }

        protected void DisplayHealPhrase(IAbility ability, int amount, ICharacter source)
        {
            if (ability != null)
            {
                if (this == source)
                {
                    Act(ActOptions.ToCharacter, "Your {0} %W%heals%x% yourself.[{1}]", ability, amount);
                    Act(ActOptions.ToRoom, "{0} {1} %W%heals%x% {0:m}self.[{2}]", this, ability, amount);
                }
                else
                {
                    Act(ActOptions.ToCharacter, "{0}'s {1} %W%heals%x% you.[{2}]", source, ability, amount);
                    if (Room == source.Room)
                        source.Act(ActOptions.ToCharacter, "Your {0} %W%heals%x% {1}.[{2}]", ability, this, amount);
                    ActToNotVictim(source, "{0}'s {1} %W%heals%x% {2}.[{3}]", source, ability, this, amount);
                }
            }
            else
            {
                if (this == source)
                {
                    Act(ActOptions.ToCharacter, "You %W%heal%x% yourself.[{0}]", amount);
                    Act(ActOptions.ToRoom, "{0} %W%heals%x% {0:m}self.[{1}]", this, amount);
                }
                else
                {
                    Act(ActOptions.ToCharacter, "{0} heals you.[{1}]", source, amount);
                    if (Room == source.Room)
                        source.Act(ActOptions.ToCharacter, "You heal {0}.[{1}]", this, amount);
                    ActToNotVictim(source, "{0} heals {1}.[{2}]", source, this, amount);
                }
            }
        }

        protected void RecomputeKnownAbilities()
        {
            // Add abilities from Class/Race/...
            _knownAbilities.Clear();

            if (Class != null)
                _knownAbilities.AddRange(Class.Abilities);
            if (Race != null)
                _knownAbilities.AddRange(Race.Abilities);
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
                            FormatActOneArgument(target, result, argumentFormat.ToString(), currentArgument);
                            state = ActParsingStates.Normal;
                        }
                        else
                        {
                            // argumentFormat cannot be null
                            argumentFormat.Append(c);
                        }
                        break;
                }
            }
            if (result.Length > 0)
                result[0] = Char.ToUpperInvariant(result[0]);
            result.AppendLine();
            return result.ToString();
        }

        // Formatting
        //  ICharacter
        //      default: same as n, N
        //      n, N: argument.name if visible by target, someone otherwise
        //      e, E: he/she/it, depending on argument.sex
        //      m, M: him/her/it, depending on argument.sex
        //      s, S: his/her/its, depending on argument.sex
        ////      r, R: you or argument.name if visible by target, someone otherwise
        ////      v, V: add 's' at the end of a verb if argument is different than target
        // IItem
        //      argument.Name if visible by target, something otherwhise
        // IExit
        //      exit name
        // IAbility
        //      ability name
        private static void FormatActOneArgument(ICharacter target, StringBuilder result, string format, object argument)
        {
            // Character ?
            ICharacter character = argument as ICharacter;
            if (character != null)
            {
                char letter = format == null ? 'n' : format[0]; // if no format, n
                switch (letter)
                {
                    case 'n':
                    case 'N':
                        if (target == character)
                            result.Append("you");
                        else
                            result.Append(target.CanSee(character)
                                ? character.DisplayName
                                : "someone");
                        break;
                    case 'e':
                    case 'E':
                        if (target == character)
                            result.Append("you");
                        else
                            result.Append(StringHelpers.Subjects[character.Sex]);
                        break;
                    case 'm':
                    case 'M':
                        if (target == character)
                            result.Append("you");
                        else
                            result.Append(StringHelpers.Objectives[character.Sex]);
                        break;
                    case 's':
                    case 'S':
                        if (target == character)
                            result.Append("your");
                        else
                            result.Append(StringHelpers.Possessives[character.Sex]);
                        break;
                        // TODO:
                        //case 'r':
                        //case 'R': // transforms '$r' into 'you' or '<name>' depending if target is the same as argument
                        //    if (character == target)
                        //        result.Append("you");
                        //    else
                        //        result.Append(target.CanSee(character)
                        //            ? character.DisplayName
                        //            : "someone");
                        //    break;
                        //case 'v':
                        //case 'V': // transforms 'look$s' into 'look' and 'looks' depending if target is the same as argument
                        //    if (character != target)
                        //        result.Append('s');
                        //    break;
                    default:
                        Log.Default.WriteLine(LogLevels.Error, "Act: invalid format {0} for ICharacter", format);
                        result.Append("<???>");
                        break;
                }
                return;
            }
            // Item ?
            IItem item = argument as IItem;
            if (item != null)
            {
                // no specific format
                result.Append(target.CanSee(item)
                    ? item.DisplayName
                    : "something");
                return;
            }
            // Exit ?
            IExit exit = argument as IExit;
            if (exit != null)
            {
                result.Append(exit.Name);
                return;
            }
            // Ability ?
            IAbility ability = argument as IAbility;
            if (ability != null)
            {
                result.Append(ability.Name);
                return;
            }
            // Other (int, string, ...)
            if (format == null)
                result.Append(argument);
            else if (argument is IFormattable)
            {
                IFormattable formattable = argument as IFormattable;
                result.Append(formattable.ToString(format, null));
            }
            else
                result.Append(argument);
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

        [Command("test")]
        protected virtual bool DoTest(string rawParameters, params CommandParameter[] parameters)
        {
            Send("Character: DoTest" + Environment.NewLine);

            Send("==> TESTING DAMAGE/PERIODIC AURAS/AURAS/ABILITIES" + Environment.NewLine);
            if (parameters.Length == 0)
                Damage(this, null, 500, SchoolTypes.Fire, true);
            else
            {
                ICharacter victim = null;
                if (parameters.Length > 1)
                    victim = FindHelpers.FindByName(Room.People, parameters[1]);
                victim = victim ?? this;
                //CommandParameter abilityTarget = victim == this
                //    ? new CommandParameter
                //    {
                //        Count = 1,
                //        Value = Name
                //    }
                //    : parameters[1];
                if (parameters[0].Value == "a")
                {
                    foreach (IAbility ability in Repository.AbilityManager.Abilities)
                    {
                        Send("[{0}]{1} [{2}] [{3}|{4}|{5}] [{6}|{7}|{8}] [{9}|{10}|{11}] {12} {13}" + Environment.NewLine,
                            ability.Id, ability.Name,
                            ability.Target,
                            ability.ResourceKind, ability.CostType, ability.CostAmount,
                            ability.GlobalCooldown, ability.Cooldown, ability.Duration,
                            ability.School, ability.Mechanic, ability.DispelType,
                            ability.Flags,
                            ability.Effects == null || ability.Effects.Count == 0 
                            ? String.Empty
                            : String.Join(" | ", ability.Effects.Select(x => "[" + x.GetType().Name + "]")));
                    }
                }
                else if (parameters[0].Value == "0")
                {
                    Repository.World.AddPeriodicAura(victim, null, this, SchoolTypes.Arcane, 75, AmountOperators.Fixed, true, 3, 8);
                    Repository.World.AddPeriodicAura(victim, null, this, SchoolTypes.Arcane, 75, AmountOperators.Fixed, true, 3, 8);
                    Repository.World.AddPeriodicAura(victim, null, this, SchoolTypes.Arcane, 75, AmountOperators.Fixed, true, 3, 8);
                    Repository.World.AddPeriodicAura(victim, null, this, 10, AmountOperators.Percentage, true, 3, 8);
                    Repository.World.AddPeriodicAura(victim, null, this, 10, AmountOperators.Percentage, true, 3, 8);
                    Repository.World.AddPeriodicAura(victim, null, this, 10, AmountOperators.Percentage, true, 3, 8);
                }
                else if (parameters[0].Value == "1")
                    victim.UnknownSourceDamage(null, 100, SchoolTypes.Frost, true);
                else if (parameters[0].Value == "2")
                    victim.UnknownSourceDamage(null, 100, SchoolTypes.Frost, true);
                else if (parameters[0].Value == "3")
                    Repository.World.AddPeriodicAura(victim, null, this, SchoolTypes.Arcane, 75, AmountOperators.Fixed, true, 3, 8);
                else if (parameters[0].Value == "4")
                    Repository.World.AddPeriodicAura(victim, null, this, 10, AmountOperators.Percentage, true, 3, 8);
                else if (parameters[0].Value == "5")
                {
                    Repository.World.AddAura(victim, null, this, AuraModifiers.Stamina, 15, AmountOperators.Percentage, 70, true);
                    Repository.World.AddAura(victim, null, this, AuraModifiers.Characteristics, -10, AmountOperators.Fixed, 30, true);
                    Repository.World.AddAura(victim, null, this, AuraModifiers.AttackPower, 150, AmountOperators.Fixed, 90, true);
                }
                else if (parameters[0].Value == "6")
                {
                    Repository.AbilityManager.Process(this, victim, Repository.AbilityManager["Shadow Word: Pain"]);
                }
                else if (parameters[0].Value == "7")
                {
                    Repository.AbilityManager.Process(this, victim, Repository.AbilityManager["Rupture"]);
                }
                else if (parameters[0].Value == "8")
                {
                    Repository.AbilityManager.Process(this, victim, Repository.AbilityManager["Trash"]);
                }
                else
                {
                    Repository.AbilityManager.Process(this, parameters);
                }
            }
            return true;
        }
    }
}
