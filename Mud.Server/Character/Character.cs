using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Mud.DataStructures.Trie;
using Mud.Logger;
using Mud.Server.Abilities;
using Mud.Server.Aura;
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
        private readonly List<ICharacter> _groupMembers;

        protected int MaxHitPoints
        {
            get { return _computedAttributes[(int) ComputedAttributeTypes.MaxHitPoints]; }
        }

        static Character()
        {
            CharacterCommands = CommandHelpers.GetCommands(typeof (Character));
        }

        public Character(Guid guid, string name, IRoom room) // playable
            : base(guid, name)
        {
            _inventory = new List<IItem>();
            _equipments = new List<EquipedItem>();
            _periodicAuras = new List<IPeriodicAura>();
            _auras = new List<IAura>();
            _basePrimaryAttributes = new int[EnumHelpers.GetCount<PrimaryAttributeTypes>()];
            _currentPrimaryAttributes = new int[EnumHelpers.GetCount<PrimaryAttributeTypes>()];
            _computedAttributes = new int[EnumHelpers.GetCount<ComputedAttributeTypes>()];
            _groupMembers = new List<ICharacter>();

            BuildEquipmentSlots();

            Sex = Sex.Neutral; // TODO: parameter
            Level = 1; // TODO: parameter
            for (int i = 0; i < _basePrimaryAttributes.Length; i++)
                _basePrimaryAttributes[i] = 150;

            Impersonable = true; // Playable
            Room = room;
            room.Enter(this);

            ResetAttributes();
        }

        public Character(Guid guid, CharacterBlueprint blueprint, IRoom room) // non-playable
            : base(guid, blueprint.Name, blueprint.Description)
        {
            _inventory = new List<IItem>();
            _equipments = new List<EquipedItem>();
            _periodicAuras = new List<IPeriodicAura>();
            _auras = new List<IAura>();
            _basePrimaryAttributes = new int[EnumHelpers.GetCount<PrimaryAttributeTypes>()]; // TODO: blueprint
            _currentPrimaryAttributes = new int[EnumHelpers.GetCount<PrimaryAttributeTypes>()]; // TODO: blueprint
            _computedAttributes = new int[EnumHelpers.GetCount<ComputedAttributeTypes>()];
            _groupMembers = new List<ICharacter>();

            Blueprint = blueprint;
            BuildEquipmentSlots();

            Sex = blueprint.Sex;
            Level = blueprint.Level;
            for (int i = 0; i < _basePrimaryAttributes.Length; i++)
                _basePrimaryAttributes[i] = 10;

            Impersonable = false; // Non-playable
            Room = room;
            room.Enter(this);

            ResetAttributes();
        }

        #region ICharacter

        #region IEntity

        #region IActor

        public override IReadOnlyTrie<CommandMethodInfo> Commands
        {
            get { return CharacterCommands; }
        }

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
            if (ImpersonatedBy != null)
                ImpersonatedBy.Page(text);
            if (ServerOptions.ForwardSlaveMessages && ControlledBy != null)
                ControlledBy.Page(text);
        }

        #endregion

        public override string DisplayName
        {
            get { return Blueprint == null ? StringHelpers.UpperFirstLetter(Name) : Blueprint.ShortDescription; }
        }

        public override void OnRemoved() // called before removing an item from the game
        {
            base.OnRemoved();

            StopFighting(true);
            if (Slave != null)
                Slave.ChangeController(null);
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

        public IReadOnlyCollection<IItem> Content
        {
            get { return _inventory.AsReadOnly(); }
        }

        public bool PutInContainer(IItem obj)
        {
            // TODO: check if already in a container
            _inventory.Add(obj);
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

        public IReadOnlyCollection<EquipedItem> Equipments
        {
            get { return _equipments.AsReadOnly(); }
        }

        // Attributes
        public Sex Sex { get; private set; }
        public int Level { get; private set; }
        public int HitPoints { get; private set; }

        // Periodic Auras
        public IReadOnlyCollection<IPeriodicAura> PeriodicAuras
        {
            get { return _periodicAuras.AsReadOnly(); }
        }

        public IReadOnlyCollection<IAura> Auras
        {
            get { return _auras.AsReadOnly(); }
        }

        public ICharacter Leader { get; private set; }

        public IReadOnlyCollection<ICharacter> GroupMembers
        {
            get { return _groupMembers.AsReadOnly(); }
        }

        // Impersonation/Controller
        public bool Impersonable { get; private set; }
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

        public int GetCurrentPrimaryAttribute(PrimaryAttributeTypes attribute)
        {
            return _currentPrimaryAttributes[(int) attribute];
        }

        public int GetComputedAttribute(ComputedAttributeTypes attribute)
        {
            return _computedAttributes[(int) attribute];
        }

        // Auras
        public void AddPeriodicAura(IPeriodicAura aura)
        {
            if (!IsValid)
            {
                Log.Default.WriteLine(LogLevels.Error, "ICharacter.AddPeriodicAura: {0} is not valid anymore", Name);
                return;
            }

            Log.Default.WriteLine(LogLevels.Info, "ICharacter.AddPeriodicAura: {0} {1}", Name, aura.Name);
            _periodicAuras.Add(aura);
            Send("You are now affected by {0}." + Environment.NewLine, aura.Name);
            if (aura.Source != null && aura.Source != this)
            {
                aura.Source.Act(ActOptions.ToCharacter, "{0} is now affected by {1}", this, aura.Name);
                if (aura.AuraType == PeriodicAuraTypes.Damage)
                {
                    if (Fighting == null)
                        StartFighting(aura.Source);
                    if (aura.Source.Fighting == null)
                        aura.Source.StartFighting(this);
                }
            }
        }

        public void RemovePeriodicAura(IPeriodicAura aura)
        {
            Log.Default.WriteLine(LogLevels.Info, "ICharacter.RemovePeriodicAura: {0} {1}", Name, aura.Name);
            bool removed = _periodicAuras.Remove(aura);
            if (!removed)
                Log.Default.WriteLine(LogLevels.Warning, "ICharacter.RemovePeriodicAura: Trying to remove unknown PeriodicAura");
            else
            {
                Send("{0} vanishes." + Environment.NewLine, aura.Name);
                if (aura.Source != null && aura.Source != this)
                    aura.Source.Act(ActOptions.ToCharacter, "{0} vanishes on {1}.", aura.Name, this);
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

            Log.Default.WriteLine(LogLevels.Info, "ICharacter.AddAura: {0} {1}| recompute: {2}", Name, aura.Name, recompute);
            _auras.Add(aura);
            Send("You are now affected by {0}." + Environment.NewLine, aura.Name);
            if (recompute)
                RecomputeAttributes();
        }

        public void RemoveAura(IAura aura, bool recompute)
        {
            Log.Default.WriteLine(LogLevels.Info, "ICharacter.RemoveAura: {0} {1} | recompute: {2}", Name, aura.Name, recompute);
            bool removed = _auras.Remove(aura);
            if (!removed)
                Log.Default.WriteLine(LogLevels.Warning, "ICharacter.RemoveAura: Trying to remove unknown aura");
            else
                Send("{0} vanishes." + Environment.NewLine, aura.Name);
            if (recompute && removed)
                RecomputeAttributes();
        }

        // Move
        public bool Move(ServerOptions.ExitDirections direction, bool follow = false)
        {
            IRoom fromRoom = Room;
            IExit exit = fromRoom.Exit(direction);
            IRoom toRoom = exit == null ? null : exit.Destination;

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
            if (Room != null)
                Room.Leave(this);
            Room = destination;
            if (destination != null)
                destination.Enter(this);
        }

        // Combat
        public bool Heal(ICharacter source, string ability, int amount, bool visible)
        {
            if (!IsValid)
            {
                Log.Default.WriteLine(LogLevels.Error, "ICharacter.Heal: {0} is not valid anymore", Name);
                return false;
            }

            Log.Default.WriteLine(LogLevels.Info, "{0} healed by {1} {2} for {3}", Name, source == null ? "<<??>>" : source.Name, ability ?? "<<??>>", amount);
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

            // TODO: secondary, haste, ...
            IItemWeapon wielded = (Equipments.FirstOrDefault(x => x.Slot == EquipmentSlots.Wield) ?? Equipments.FirstOrDefault(x => x.Slot == EquipmentSlots.Wield2H) ?? EquipedItem.NullObject).Item as IItemWeapon;
            IItemWeapon wielded2 = (Equipments.FirstOrDefault(x => x.Slot == EquipmentSlots.Wield2) ?? EquipedItem.NullObject).Item as IItemWeapon;
            if (wielded != null)
                OneHit(enemy, wielded.Name, wielded, wielded.DamageType);
            else
                OneHit(enemy, "claws", null, SchoolTypes.Physical);

            if (Fighting != enemy) // stop multihit if different enemy or no enemy
                return true;

            if (wielded2 != null)
                OneHit(enemy, wielded2.Name, wielded2, wielded2.DamageType);
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
                foreach (ICharacter enemy in World.World.Instance.GetCharacters().Where(x => x.Fighting == this))
                {
                    enemy.StopFighting(false);
                    // TODO: change/update pos
                }
            return true;
        }

        public bool CombatDamage(ICharacter source, string ability, int damage, SchoolTypes damageType, bool visible) // damage with known source
        {
            if (!IsValid)
            {
                Log.Default.WriteLine(LogLevels.Error, "CombatDamage: {0} is not valid anymore", Name);
                return false;
            }

            // TODO: check combat_damage in fight.C:1940
            // TODO: damage reduction

            // Starts fight if needed (if A attacks B, A fights B and B fights A)
            if (this != source)
            {
                if (Fighting == null)
                    StartFighting(source);
                if (source.Fighting == null)
                    source.StartFighting(this);
                // TODO: Cannot attack slave without breaking slavery
            }
            // TODO: if invisible, remove invisibility
            // TODO: damage modifier
            // TODO: check immunity/resist/vuln

            // Check shield
            bool fullyAbsorbed = false;
            if (_auras.Any(x => x.Modifier == AuraModifiers.Shield))
            {
                bool needsRecompute = false;
                // Process every shield aura until 0 damage left or 0 shield aura left
                IReadOnlyCollection<IAura> shields = new ReadOnlyCollection<IAura>(_auras.Where(x => x.Modifier == AuraModifiers.Shield).ToList());
                foreach (IAura shield in shields)
                {
                    // Process absorb
                    damage = shield.Absorb(damage);
                    if (damage == 0) // full absorb
                    {
                        fullyAbsorbed = true;
                        Log.Default.WriteLine(LogLevels.Debug, "Damage [{0}] totally absorbed by shield {1}", damage, shield.Name);
                        break; // no need to check other shield
                    }
                    else // partial absorb
                    {
                        Log.Default.WriteLine(LogLevels.Debug, "Damage [{0}] partially absorbed [{1}] by shield {2}", damage, shield.Amount, shield.Name);
                        needsRecompute = true;
                        RemoveAura(shield, false); // recompute when everything is done
                    }
                }
                if (needsRecompute)
                    RecomputeAttributes();
            }

            if (visible) // equivalent to dam_message in fight.C:4381
            {
                if (fullyAbsorbed)
                    DisplayAbsorbPhrase(ability, source);
                else
                    DisplayCombatDamagePhrase(ability, damage, source);
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

        public bool UnknownSourceDamage(string ability, int damage, SchoolTypes damageType, bool visible) // damage with unknown source or no source
        {
            if (!IsValid)
            {
                Log.Default.WriteLine(LogLevels.Error, "UnknownSourceDamage: {0} is not valid anymore", Name);
                return false;
            }

            // TODO: if invisible, remove invisibility
            // TODO: damage modifier
            // TODO: check immunity/resist/vuln

            // Check shield
            bool fullyAbsorbed = false;
            if (_auras.Any(x => x.Modifier == AuraModifiers.Shield))
            {
                bool needsRecompute = false;
                // Process every shield aura until 0 damage left or 0 shield aura left
                IReadOnlyCollection<IAura> shields = new ReadOnlyCollection<IAura>(_auras.Where(x => x.Modifier == AuraModifiers.Shield).ToList());
                foreach (IAura shield in shields)
                {
                    // Process absorb
                    damage = shield.Absorb(damage);
                    if (damage == 0) // full absorb
                    {
                        fullyAbsorbed = true;
                        Log.Default.WriteLine(LogLevels.Debug, "Damage [{0}] totally absorbed by shield {1}", damage, shield.Name);
                        break; // no need to check other shield
                    }
                    else // partial absorb
                    {
                        Log.Default.WriteLine(LogLevels.Debug, "Damage [{0}] partially absorbed [{1}] by shield {2}", damage, shield.Amount, shield.Name);
                        needsRecompute = true;
                        RemoveAura(shield, false); // recompute when everything is done
                    }
                }
                if (needsRecompute)
                    RecomputeAttributes();
            }

            if (visible) // equivalent to dam_message in fight.C:4381
            {
                if (fullyAbsorbed)
                    DisplayUnknownSourceAbsorbPhrase(ability);
                else
                    DisplayUnknownSourceDamagePhrase(ability, damage);
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
            IItemCorpse corpse = World.World.Instance.AddItemCorpse(Guid.NewGuid(), ServerOptions.CorpseBlueprint, Room, victim);
            if (victim.ImpersonatedBy != null) // If impersonated, no real death
            {
                // TODO: teleport player to hall room/graveyard  see fight.C:3952
                victim.ResetAttributes();
            }
            else // If not impersonated, remove from game
            {
                World.World.Instance.RemoveCharacter(victim);
            }

            // TODO: autoloot, autosac  damage.C:96
            return true;
        }

        public void ResetAttributes()
        {
            if (!IsValid)
            {
                Log.Default.WriteLine(LogLevels.Error, "ResetAttributes: {0} is not valid anymore", Name);
                return;
            }

            RecomputeAttributes();

            HitPoints = MaxHitPoints;
            // TODO: mana, ...
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
            _computedAttributes[(int)ComputedAttributeTypes.MaxHitPoints] = _currentPrimaryAttributes[(int)PrimaryAttributeTypes.Stamina] * 50 + 1000;
            _computedAttributes[(int)ComputedAttributeTypes.AttackPower] = _currentPrimaryAttributes[(int)PrimaryAttributeTypes.Strength] * 2 + 50;
            _computedAttributes[(int)ComputedAttributeTypes.SpellPower] = _currentPrimaryAttributes[(int)PrimaryAttributeTypes.Intellect] + 50;
            _computedAttributes[(int)ComputedAttributeTypes.AttackSpeed] = 20;
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
                }
            }
            // Keep attributes in valid range
            HitPoints = Math.Min(HitPoints, MaxHitPoints);
        }

        #endregion

        protected void ModifyPrimaryAttribute(PrimaryAttributeTypes attribute, AmountOperators op, int amount)
        {
            if (op == AmountOperators.Percentage)
                amount = _basePrimaryAttributes[(int) attribute]*amount/100;
            _currentPrimaryAttributes[(int) attribute] += amount;
        }

        protected void ModifyComputedAttribute(ComputedAttributeTypes attribute, AmountOperators op, int amount)
        {
            if (op == AmountOperators.Percentage)
                amount = _computedAttributes[(int)attribute] * amount / 100;
            _computedAttributes[(int)attribute] += amount;
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
            if (ImpersonatedBy != null)
                ImpersonatedBy.SetGlobalCooldown(pulseCount);
        }

        protected bool ApplyDamageAndDisplayStatus(int damage)
        {
            // Apply damage
            bool dead = false;
            HitPoints -= damage;
            if (HitPoints < 1)
            {
                if (ImpersonatedBy != null)
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

        protected bool OneHit(ICharacter victim, string ability, IItemWeapon weapon, SchoolTypes damageType) // TODO: skill    check fight.C:1394
        {
            if (this == victim || Room != victim.Room)
                return false;
            // TODO: skill percentage
            // TODO: check wield  fight.C:1595
            // TODO: check parry, dodge, shield block, ...

            int damage = 0;

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

            return victim.CombatDamage(this, ability, damage, damageType, true);
        }

        protected void DisplayAbsorbPhrase(string ability, ICharacter source)
        {
            if (!String.IsNullOrWhiteSpace(ability))
            {
                if (this == source)
                {
                    Act(ActOptions.ToCharacter, "Your {0} is absorbed.", ability);
                    Act(ActOptions.ToRoom, "{0} {1} is absorbed.", source, ability);
                }
                else
                {
                    Act(ActOptions.ToCharacter, "{0}'s {1} is absorbed.", source, ability);
                    if (Room == source.Room)
                        source.Act(ActOptions.ToCharacter, "Your {0} is absorbed.", ability);
                    ActToNotVictim(source, "{0}'s {1} is absorbed by {2}.", source, ability, this);
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

        protected void DisplayUnknownSourceAbsorbPhrase(string ability)
        {
            if (!String.IsNullOrWhiteSpace(ability))
            {
                Act(ActOptions.ToCharacter, "{0} is absorbed.", ability);
                Act(ActOptions.ToRoom, "{0} is absorbed by {1}.", ability, this);
            }
            else
            {
                Act(ActOptions.ToCharacter, "You absorb damage.");
                Act(ActOptions.ToRoom, "{0} absorbs damage.", this);
            }
        }

        protected void DisplayCombatDamagePhrase(string ability, int damage, ICharacter source)
        {
            string damagePhraseSelf = StringHelpers.DamagePhraseSelf(damage);
            string damagePhraseOther = StringHelpers.DamagePhraseOther(damage);

            if (!String.IsNullOrWhiteSpace(ability))
            {
                if (this == source)
                {
                    Act(ActOptions.ToCharacter, "Your {0} {1} yourself.[{2}]", ability, damagePhraseOther, damage);
                    Act(ActOptions.ToRoom, "{0} {1} {2} {0:m}self.[{3}]", source, ability, damagePhraseOther, damage);
                }
                else
                {
                    Act(ActOptions.ToCharacter, "{0}'s {1} {2} you.[{3}]", source, ability, damagePhraseOther, damage);
                    if (Room == source.Room)
                        source.Act(ActOptions.ToCharacter, "Your {0} {1} {2}.[{3}]", ability, damagePhraseOther, this, damage);
                    ActToNotVictim(source, "{0}'s {1} {2} {3}.[{4}]", source, ability, damagePhraseOther, this, damage);
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

        protected void DisplayUnknownSourceDamagePhrase(string ability, int damage)
        {
            string damagePhraseSelf = StringHelpers.DamagePhraseSelf(damage);
            string damagePhraseOther = StringHelpers.DamagePhraseOther(damage);

            if (!String.IsNullOrWhiteSpace(ability))
            {
                Act(ActOptions.ToCharacter, "{0} {1} you.[{2}]", ability, damagePhraseSelf, damage);
                Act(ActOptions.ToRoom, "{0} {1} {2}.[{3}]", ability, damagePhraseOther, this, damage);
            }
            else
            {
                Log.Default.WriteLine(LogLevels.Warning, "ICharacter.NonCombatDamage: no ability");
                Act(ActOptions.ToCharacter, "Something {0} you.[{1}]", damagePhraseOther, damage);
                Act(ActOptions.ToRoom, "Something {0} {1}.[{2}]", damagePhraseOther, this, damage);
            }
        }

        protected void DisplayHealPhrase(string ability, int amount, ICharacter source)
        {
            if (!String.IsNullOrWhiteSpace(ability))
            {
                if (this == source)
                {
                    Act(ActOptions.ToCharacter, "Your {0} %w%heals%x% yourself.[{1}]", ability, amount);
                    Act(ActOptions.ToRoom, "{0} {1} %w%heals%x% {0:m}self.[{2}]", this, ability, amount);
                }
                else
                {
                    Act(ActOptions.ToCharacter, "{0}'s {1} %w%heals%x% you.[{2}]", source, ability, amount);
                    if (Room == source.Room)
                        source.Act(ActOptions.ToCharacter, "Your {0} %w%heals%x% {1}.[{2}]", ability, this, amount);
                    ActToNotVictim(source, "{0}'s {1} %w%heals%x% {2}.[{3}]", source, ability, this, amount);
                }
            }
            else
            {
                if (this == source)
                {
                    Act(ActOptions.ToCharacter, "You %w%heal%x% yourself.[{0}]", amount);
                    Act(ActOptions.ToRoom, "{0} %w%heals%x% {0:m}self.[{1}]", this, amount);
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
            for (int i = 0; i < format.Length; i++)
            {
                char c = format[i];

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
        private static void FormatActOneArgument(ICharacter target, StringBuilder result, string format, object argument)
        {
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
            }
            else
            {
                IItem item = argument as IItem;
                if (item != null)
                {
                    // no specific format
                    result.Append(target.CanSee(item)
                        ? item.DisplayName
                        : "something");
                }
                else
                {
                    IExit exit = argument as IExit;
                    if (exit != null)
                        result.Append(exit.Name);
                    else
                    {
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
                }
            }
        }

        #endregion

        [Command("kill")]
        protected virtual bool DoKill(string rawParameters, params CommandParameter[] parameters)
        {
            Send("DoKill: NOT YET IMPLEMENTED" + Environment.NewLine);

            Send("==> TESTING MULTIHIT" + Environment.NewLine);
            if (parameters.Length > 0)
            {
                ICharacter target = FindHelpers.FindByName(Room.People, parameters[0]);

                if (target != null)
                    MultiHit(target);
            }
            return true;
        }

        [Command("test")]
        protected virtual bool DoTest(string rawParameters, params CommandParameter[] parameters)
        {
            Send("Character: DoTest" + Environment.NewLine);

            Send("==> TESTING DAMAGE/PERIODIC AURAS/AURAS/ABILITIES" + Environment.NewLine);
            if (parameters.Length == 0)
                CombatDamage(this, "STUPIDITY", 500, SchoolTypes.Fire, true);
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
                    AbilityManager abilityManager = new AbilityManager();
                    foreach (Ability ability in abilityManager.Abilities)
                    {
                        Send("[{0}]{1} {2} [{3}|{4}|{5}] [{6}|{7}|{8}] [{9}|{10}|{11}] {12} {13}" + Environment.NewLine,
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
                    World.World.Instance.AddPeriodicAura(victim, "DoT", this, SchoolTypes.Arcane, 75, AmountOperators.Fixed, true, 3, 8);
                    World.World.Instance.AddPeriodicAura(victim, "DoT", this, SchoolTypes.Arcane, 75, AmountOperators.Fixed, true, 3, 8);
                    World.World.Instance.AddPeriodicAura(victim, "DoT", this, SchoolTypes.Arcane, 75, AmountOperators.Fixed, true, 3, 8);
                    World.World.Instance.AddPeriodicAura(victim, "HoT", this, 10, AmountOperators.Percentage, true, 3, 8);
                    World.World.Instance.AddPeriodicAura(victim, "HoT", this, 10, AmountOperators.Percentage, true, 3, 8);
                    World.World.Instance.AddPeriodicAura(victim, "HoT", this, 10, AmountOperators.Percentage, true, 3, 8);
                }
                else if (parameters[0].Value == "1")
                    victim.UnknownSourceDamage("STUPIDITY2", 100, SchoolTypes.Frost, true);
                else if (parameters[0].Value == "2")
                    victim.UnknownSourceDamage(null, 100, SchoolTypes.Frost, true);
                else if (parameters[0].Value == "3")
                    World.World.Instance.AddPeriodicAura(victim, "DoT", this, SchoolTypes.Arcane, 75, AmountOperators.Fixed, true, 3, 8);
                else if (parameters[0].Value == "4")
                    World.World.Instance.AddPeriodicAura(victim, "HoT", this, 10, AmountOperators.Percentage, true, 3, 8);
                else if (parameters[0].Value == "5")
                {
                    World.World.Instance.AddAura(victim, "BuffSta", AuraModifiers.Stamina, 15, AmountOperators.Percentage, 70, true);
                    World.World.Instance.AddAura(victim, "DebuffCarac", AuraModifiers.Characteristics, -10, AmountOperators.Fixed, 30, true);
                    World.World.Instance.AddAura(victim, "BuffAP", AuraModifiers.AttackPower, 150, AmountOperators.Fixed, 90, true);
                }
                else if (parameters[0].Value == "6")
                {
                    AbilityManager abilityManager = new AbilityManager();
                    abilityManager.Process(this, victim, abilityManager.Abilities.First(x => x.Name == "Shadow Word: Pain"));
                }
                else if (parameters[0].Value == "7")
                {
                    AbilityManager abilityManager = new AbilityManager();
                    abilityManager.Process(this, victim, abilityManager.Abilities.First(x => x.Name == "Rupture"));
                }
                else if (parameters[0].Value == "8")
                {
                    AbilityManager abilityManager = new AbilityManager();
                    abilityManager.Process(this, victim, abilityManager.Abilities.First(x => x.Name == "Trash"));
                }
                else
                {
                    AbilityManager abilityManager = new AbilityManager();
                    abilityManager.Process(this, parameters);
                }
            }
            return true;
        }
    }
}
