using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Mud.Container;
using Mud.DataStructures.Trie;
using Mud.Domain;
using Mud.Logger;
using Mud.Server.Blueprints.Item;
using Mud.Server.Common;
using Mud.Server.Helpers;
using Mud.Server.Input;
using Mud.Server.Item;
using Mud.Server.Quest;

namespace Mud.Server.Character.PlayableCharacter
{
    public partial class PlayableCharacter : CharacterBase, IPlayableCharacter
    {
        private static readonly Lazy<IReadOnlyTrie<CommandMethodInfo>> PlayableCharacterCommands = new Lazy<IReadOnlyTrie<CommandMethodInfo>>(() => GetCommands<PlayableCharacter>());

        protected IAdminManager AdminManager => DependencyContainer.Current.GetInstance<IAdminManager>();

        private readonly List<IPlayableCharacter> _groupMembers;
        private readonly List<IQuest> _quests;

        public PlayableCharacter(Guid guid, CharacterData data, IPlayer player, IRoom room)
            : base(guid, data.Name, string.Empty)
        {
            _groupMembers = new List<IPlayableCharacter>();
            _quests = new List<IQuest>();

            ImpersonatedBy = player;

            // Extract informations from CharacterData
            CreationTime = data.CreationTime;
            Class = ClassManager[data.Class];
            if (Class == null)
            {
                string msg = $"Invalid class '{data.Class}' for character {data.Name}!!";
                Log.Default.WriteLine(LogLevels.Error, msg);
                Class = ClassManager.Classes.First();
                Wiznet.Wiznet(msg, WiznetFlags.Bugs, AdminLevels.Implementor);
            }
            Race = RaceManager[data.Race];
            if (Race == null)
            {
                string msg = $"Invalid race '{data.Race}' for character {data.Name}!!";
                Log.Default.WriteLine(LogLevels.Error, msg);
                Race = RaceManager.Races.First();
                Wiznet.Wiznet(msg, WiznetFlags.Bugs, AdminLevels.Implementor);
            }
            Sex = data.Sex;
            Level = data.Level;
            Experience = data.Experience;

            // Must be built before equiping
            BuildEquipmentSlots();

            // Equiped items
            if (data.Equipments != null)
            {
                // Create item in inventory and try to equip it
                foreach (EquipedItemData equipedItemData in data.Equipments)
                {
                    // Create in inventory
                    var item = World.AddItem(Guid.NewGuid(), equipedItemData.Item as ItemData, this);

                    // Try to equip it
                    EquipedItem equipedItem = SearchEquipmentSlot(equipedItemData.Slot, false);
                    if (equipedItem != null)
                    {
                        if (item is IEquipable equipable)
                        {
                            equipedItem.Item = equipable;
                            equipable.ChangeContainer(null); // remove from inventory
                            equipable.ChangeEquipedBy(this); // set as equiped by this
                        }
                        else
                        {
                            string msg = $"Item blueprint Id {equipedItemData.Item.ItemId} cannot be equipped anymore in slot {equipedItemData.Slot} for character {data.Name}.";
                            Log.Default.WriteLine(LogLevels.Error, msg, equipedItemData.Item.ItemId, equipedItemData.Slot);
                            Wiznet.Wiznet(msg, WiznetFlags.Bugs);
                        }
                    }
                    else
                    {
                        string msg = $"Item blueprint Id {equipedItemData.Item.ItemId} was supposed to be equipped in first empty slot {equipedItemData.Slot} for character {data.Name} but this slot doesn't exist anymore.";
                        Log.Default.WriteLine(LogLevels.Error, msg);
                        Wiznet.Wiznet(msg, WiznetFlags.Bugs);
                    }
                }
            }
            // Inventory
            if (data.Inventory != null)
            {
                foreach (ItemData itemData in data.Inventory)
                    World.AddItem(Guid.NewGuid(), itemData, this);
            }
            // Quests
            if (data.CurrentQuests != null)
            {
                foreach (CurrentQuestData questData in data.CurrentQuests)
                    _quests.Add(new Quest.Quest(questData, this));
            }

            Room = room;
            room.Enter(this);

            RecomputeBaseAttributes();
            RecomputeKnownAbilities();
            ResetAttributes(true);
            RecomputeCommands();
            RecomputeCurrentResourceKinds();
        }

        #region IPlayableCharacter

        #region ICharacter

        #region IEntity

        #region IActor

        public override bool ExecuteBeforeCommand(CommandMethodInfo methodInfo, string rawParameters, params CommandParameter[] parameters)
        {
            if (methodInfo.Attribute is PlayableCharacterCommandAttribute playableCharacterCommandAttribute)
            {
                if (ImpersonatedBy == null)
                {
                    Send($"You must be impersonated to use '{playableCharacterCommandAttribute.Name}'.");
                    return false;
                }
            }
            return base.ExecuteBeforeCommand(methodInfo, rawParameters, parameters);
        }

        public override void Send(string message, bool addTrailingNewLine)
        {
            // TODO: use Act formatter ?
            base.Send(message, addTrailingNewLine);
            if (ImpersonatedBy != null)
            {
                if (Settings.PrefixForwardedMessages)
                    message = "<IMP|" + DisplayName + ">" + message;
                ImpersonatedBy.Send(message, addTrailingNewLine);
            }
        }

        public override void Page(StringBuilder text)
        {
            base.Page(text);
            ImpersonatedBy?.Page(text);
        }

        #endregion

        public override string DisplayName => Name.UpperFirstLetter();

        public override string DebugName => $"{DisplayName}[{ImpersonatedBy?.DisplayName ?? "???"}]";

        public override string RelativeDisplayName(ICharacter beholder, bool capitalizeFirstLetter = false)
        {
            StringBuilder displayName = new StringBuilder();
            if (beholder.CanSee(this))
                displayName.Append(DisplayName);
            else if (capitalizeFirstLetter)
                displayName.Append("Someone");
            else
                displayName.Append("someone");
            if (beholder is IPlayableCharacter playableBeholder && playableBeholder.ImpersonatedBy is IAdmin)
                displayName.Append($" [{ImpersonatedBy?.DisplayName ?? " ??? "}]");
            return displayName.ToString();
        }


        public override void OnRemoved() // called before removing a character from the game
        {
            base.OnRemoved();

            StopFighting(true);
            Slave?.ChangeController(null);
            // TODO: what if character is incarnated
            ImpersonatedBy?.StopImpersonating();
            ImpersonatedBy = null; // TODO: warn ImpersonatedBy ?
            ControlledBy = null; // TODO: warn ControlledBy ?
            Leader = null; // TODO: warn Leader
            ResetCooldowns();
            DeleteInventory();
            DeleteEquipments();
            Room = null;
        }

        #endregion

        // Combat
        public override void KillingPayoff(ICharacter victim) // Gain xp/gold/reputation/...
        {
            // Gain xp
            if (this != victim && victim is INonPlayableCharacter) // gain xp only for non-playable victim
            {
                // TODO: not the best way to do it
                // Build group members
                List<IPlayableCharacter> members = new List<IPlayableCharacter>
                {
                    this
                };
                IPlayableCharacter leader = null;
                // Member of a group
                if (Leader != null && Leader.GroupMembers.Any(x => x == this))
                    leader = Leader;
                // Leader of a group
                else if (Leader == null && GroupMembers.Any())
                    leader = this;
                if (leader != null)
                    members.AddRange(leader.GroupMembers);
                int highestLevel = members.Max(x => x.Level);
                int sumLevels = members.Sum(x => x.Level);
                int memberCount = members.Count;
                // Gain xp
                foreach (IPlayableCharacter member in members)
                {
                    long experienceGain;
                    if (memberCount == 1)
                        experienceGain = CombatHelpers.GetSoloMobExperienceFull(member.Level, victim.Level, false /*TODO: elite*/, 0 /*TODO: rested exp*/);
                    else if (memberCount == 2)
                        experienceGain = CombatHelpers.GetDuoMobExperienceFull(members[0].Level, members[1].Level, victim.Level, false /*TODO: elite*/, 0 /*TODO: rested exp*/);
                    else
                        experienceGain = CombatHelpers.GetPartyMobExperienceFull(member.Level, highestLevel, sumLevels, memberCount, victim.Level, false /*TODO: elite*/, 0 /*TODO: rested exp*/);
                    if (experienceGain > 0)
                    {
                        member.Send("%y%You gain {0} experience points.%x%", experienceGain);
                        member.GainExperience(experienceGain);
                    }
                }
                // TODO: gold, reputation
            }
        }

        #endregion

        public DateTime CreationTime { get; protected set; }

        public long ExperienceToLevel =>
            Level >= Settings.MaxLevel
                ? 0
                : CombatHelpers.CumulativeExperienceByLevel[Level] + CombatHelpers.ExperienceToNextLevel[Level] - Experience;

        public long Experience { get; protected set; }

        public IPlayableCharacter Leader { get; protected set; }

        public IEnumerable<IPlayableCharacter> GroupMembers => _groupMembers.Where(x => x.IsValid);

        public override bool CanSee(IItem target)
        {
            if (target is IItemQuest questItem)
            {
                // See only if on this quest
                if (Quests.Any(x => x.Objectives.OfType<ItemQuestObjective>().Any(o => o.Blueprint.Id == questItem.Blueprint.Id)))
                    return true;
                return false;
            }
            return base.CanSee(target);
        }

        // Impersonation/Controller
        public IPlayer ImpersonatedBy { get; protected set; }

        // Quest
        public IEnumerable<IQuest> Quests => _quests;

        public void AddQuest(IQuest quest)
        {
            // TODO: max quest ?
            _quests.Add(quest);
        }

        public void RemoveQuest(IQuest quest)
        {
            _quests.Remove(quest);
        }

        // Room
        public IRoom RecallRoom => World.GetDefaultRecallRoom(); // TODO: could be different from default one

        // Group
        public bool IsSameGroup(IPlayableCharacter character)
        {
            if (!IsValid || !character.IsValid)
                return false;
            //if (GroupMembers.Any(x => x == character) && character.GroupMembers.Any(x => x == this))
            //    return true;
            //return false;
            IPlayableCharacter first = Leader ?? this;
            IPlayableCharacter second = character.Leader ?? character;
            return first == second;
        }

        public bool ChangeLeader(IPlayableCharacter newLeader)
        {
            if (!IsValid)
            {
                Log.Default.WriteLine(LogLevels.Error, "ICharacter.ChangeLeader: {0} is not valid anymore", DebugName);
                return false;
            }
            if (newLeader != null && !newLeader.IsValid)
            {
                Log.Default.WriteLine(LogLevels.Error, "ICharacter.ChangeLeader: {0} is not valid anymore", newLeader.DebugName);
                return false;
            }
            Log.Default.WriteLine(LogLevels.Debug, "ICharacter.ChangeLeader: {0} old= {1}; new {2}", DebugName, Leader == null ? "<<none>>" : Leader.DebugName, newLeader == null ? "<<none>>" : newLeader.DebugName);
            Leader = newLeader;
            return true;
        }

        public bool AddGroupMember(IPlayableCharacter newMember, bool silent)
        {
            if (!IsValid)
            {
                Log.Default.WriteLine(LogLevels.Error, "ICharacter.AddGroupMember: {0} is not valid anymore", DebugName);
                return false;
            }
            if (Leader != null)
            {
                Log.Default.WriteLine(LogLevels.Warning, "ICharacter.AddGroupMember: {0} cannot add member because leader is not null", DebugName);
                return false;
            }
            if (!newMember.IsValid)
            {
                Log.Default.WriteLine(LogLevels.Error, "ICharacter.AddGroupMember: new member {0} is not valid anymore");
                return false;
            }
            if (_groupMembers.Any(x => x == newMember))
            {
                Log.Default.WriteLine(LogLevels.Error, "ICharacter.AddGroupMember: {0} already in group of {1}", newMember.DebugName, DebugName);
                return false;
            }
            Log.Default.WriteLine(LogLevels.Debug, "ICharacter.AddGroupMember: {0} joined by {1}", DebugName, newMember.DebugName);
            if (!silent)
                Send("{0} joins group.", newMember.DisplayName);
            newMember.ChangeLeader(this); // this is not mandatory (should be done by caller)
            if (!silent)
            {
                foreach (IPlayableCharacter member in _groupMembers)
                    member.Send("{0} joins group.", newMember.DisplayName);
            }
            if (!silent)
                newMember.Act(ActOptions.ToCharacter, "You join {0}'s group.", this);
            _groupMembers.Add(newMember);
            return true;
        }

        public bool RemoveGroupMember(IPlayableCharacter oldMember, bool silent) // TODO: what if leader leaves group!!!
        {
            Log.Default.WriteLine(LogLevels.Debug, "ICharacter.RemoveGroupMember: {0} leaves {1}", oldMember.DebugName, DebugName);
            bool removed = _groupMembers.Remove(oldMember);
            if (!removed)
            {
                Log.Default.WriteLine(LogLevels.Debug, "ICharacter.RemoveGroupMember: {0} not in group of {1}", oldMember.DebugName, DebugName);
                return false;
            }
            oldMember.ChangeLeader(null); // this is not mandatory (should be done by caller)
            if (!silent)
                Send("{0} leaves group.", oldMember.DebugName);
            if (!silent)
            {
                foreach (IPlayableCharacter member in _groupMembers)
                    member.Send("{0} leaves group.", member.DebugName);
            }
            if (!silent)
                oldMember.Act(ActOptions.ToCharacter, "You leave {0}'s group.", this);
            return true;
        }

        public bool AddFollower(IPlayableCharacter follower)
        {
            follower.ChangeLeader(this);
            if (CanSee(follower))
                Act(ActOptions.ToCharacter, "{0} now follows you.", follower);
            follower.Act(ActOptions.ToCharacter, "You now follow {0}.", this);
            return true;
        }

        public bool StopFollower(IPlayableCharacter follower)
        {
            if (follower.Leader == null)
            {
                Log.Default.WriteLine(LogLevels.Warning, "ICharacter:StopFollower: {0} is not following anyone", follower.DebugName);
                return false;
            }
            if (follower.Leader != this)
            {
                Log.Default.WriteLine(LogLevels.Error, "ICharacter:StopFollower: {0} is not following {1} but {2}", follower.DebugName, DebugName, follower.Leader == null ? "<<none>>" : follower.Leader.DebugName);
                return false;
            }
            follower.ChangeLeader(null);
            if (CanSee(follower))
                Act(ActOptions.ToCharacter, "{0} stops following you.", follower);
            follower.Act(ActOptions.ToCharacter, "You stop following {0}.", this);
            return true;
        }

        // Impersonation
        public bool ChangeImpersonation(IPlayer player) // if non-null, start impersonation, else, stop impersonation
        {
            if (!IsValid)
            {
                Log.Default.WriteLine(LogLevels.Error, "ICharacter.ChangeImpersonation: {0} is not valid anymore", DebugName);
                ImpersonatedBy = null;
                return false;
            }
            if (player != null)
            {
                if (ImpersonatedBy != null)
                {
                    Log.Default.WriteLine(LogLevels.Warning, "ICharacter.ChangeImpersonation: {0} is already impersonated by {1}", DebugName, ImpersonatedBy.DisplayName);
                    return false;
                }
            }

            Log.Default.WriteLine(LogLevels.Debug, "ICharacter.ChangeImpersonation: {0} old: {1}; new {2}", DebugName, ImpersonatedBy == null ? "<<none>>" : ImpersonatedBy.DisplayName, player == null ? "<<none>>" : player.DisplayName);
            ImpersonatedBy = player;
            RecomputeKnownAbilities();
            RecomputeAttributes();
            RecomputeCommands();
            return true;
        }

        // Combat
        public void GainExperience(long experience)
        {
            if (Level >= Settings.MaxLevel)
            {
                // NOP
            }
            else
            {
                bool recompute = false;
                Experience += experience;
                // Raise level
                if (experience > 0)
                {
                    while (ExperienceToLevel <= 0)
                    {
                        recompute = true;
                        Level++;
                        Wiznet.Wiznet($"{DebugName} has attained level {Level}", WiznetFlags.Levels);
                        Send("You raise a level!!");
                        Act(ActOptions.ToGroup, "{0} has attained level {1}", this, Level);
                        // In case multiple level are gain, check max level
                        if (Level >= Settings.MaxLevel)
                            break;
                    }
                }
                if (recompute)
                {
                    RecomputeBaseAttributes();
                    RecomputeKnownAbilities();
                    RecomputeAttributes();
                    RecomputeCommands();
                    // Bonus -> reset cooldown and set resource to max
                    ResetCooldowns();
                    ResetAttributes(true);
                    ImpersonatedBy?.Save(); // Force a save when a level is gained
                }
            }
        }

        public CharacterData MapCharacterData()
        {
            CharacterData data = new CharacterData
            {
                Name = Name,
                CreationTime = CreationTime,
                Sex = Sex,
                Class = Class?.Name ?? string.Empty,
                Race = Race?.Name ?? string.Empty,
                Level = Level,
                RoomId = Room?.Blueprint?.Id ?? 0,
                Experience = Experience,
                Equipments = Equipments.Where(x => x.Item != null).Select(x => x.MapEquipedData()).ToArray(),
                Inventory = Inventory.Select(x => x.MapItemData()).ToArray(),
                CurrentQuests = Quests.Select(x => x.GenerateQuestData()).ToArray(),
                // TODO: aura, cooldown, ...
            };
            return data;
        }


        #endregion

        #region CharacterBase

        protected override int NoWeaponDamage => 75; // TODO

        protected override int HitPointMinValue => 1; // Cannot be really killed

        protected override IReadOnlyTrie<CommandMethodInfo> StaticCommands => PlayableCharacterCommands.Value;

        protected override int ModifyCriticalDamage(int damage) => (damage * 150) / 200; // TODO http://wow.gamepedia.com/Critical_strike

        protected override bool RawKilled(IEntity killer, bool killingPayoff) // TODO: refactor, same code in NonPlayableCharacter
        {
            if (!IsValid)
            {
                Log.Default.WriteLine(LogLevels.Error, "RawKilled: {0} is not valid anymore", DebugName);
                return false;
            }

            ICharacter characterKiller = killer as ICharacter;

            string wiznetMsg;
            if (killer != null)
                wiznetMsg = $"{DebugName} got toasted by {killer.DebugName ?? "???"} at {Room?.DebugName ?? "???"}";
            else
                wiznetMsg = $"{DebugName} got toasted by an unknown source at {Room?.DebugName ?? "???"}";
            Wiznet.Wiznet(wiznetMsg, WiznetFlags.Deaths);

            StopFighting(true);
            // Remove periodic auras
            IReadOnlyCollection<IPeriodicAura> periodicAuras = new ReadOnlyCollection<IPeriodicAura>(PeriodicAuras.ToList()); // clone
            foreach (IPeriodicAura pa in periodicAuras)
                RemovePeriodicAura(pa);
            // Remove auras
            IReadOnlyCollection<IAura> auras = new ReadOnlyCollection<IAura>(Auras.ToList()); // clone
            foreach (IAura aura in auras)
                RemoveAura(aura, false);
            // no need to recompute

            // Death cry
            ActToNotVictim(this, "You hear {0}'s death cry.", this);

            // Gain/lose xp/reputation   damage.C:32
            if (killingPayoff && characterKiller != null)
                characterKiller?.KillingPayoff(this);
            DeathPayoff();

            // Create corpse
            ItemCorpseBlueprint itemCorpseBlueprint = World.GetItemBlueprint<ItemCorpseBlueprint>(Settings.CorpseBlueprintId);
            if (itemCorpseBlueprint != null)
            {
                IItemCorpse corpse;
                if (characterKiller != null)
                    corpse = World.AddItemCorpse(Guid.NewGuid(), itemCorpseBlueprint, Room, this, characterKiller);
                else
                    corpse = World.AddItemCorpse(Guid.NewGuid(), itemCorpseBlueprint, Room, this);
            }
            else
            {
                string msg = $"ItemCorpseBlueprint (id:{Settings.CorpseBlueprintId}) doesn't exist !!!";
                Log.Default.WriteLine(LogLevels.Error, msg);
                Wiznet.Wiznet(msg, WiznetFlags.Bugs);
            }

            if (ImpersonatedBy != null) // If impersonated, no real death
            {
                // TODO: teleport player to hall room/graveyard  see fight.C:3952
                ResetAttributes(false); // don't reset hp
            }
            else // If not impersonated, remove from game
            {
                World.RemoveCharacter(this);
            }

            // TODO: autoloot, autosac  damage.C:96
            return true;
        }

        protected override bool AutomaticallyDisplayRoom => true;

        protected override bool BeforeMove(ExitDirections direction, IRoom fromRoom, IRoom toRoom)
        {
            // Compute move and check if enough move left
            int moveCost = RandomManager.Range(1, 6); // TODO: dependends on room sector
            if (CharacterFlags.HasFlag(CharacterFlags.Flying))
                moveCost /= 2; // flying is less exhausting
            if (CharacterFlags.HasFlag(CharacterFlags.Slow))
                moveCost *= 2; // being slowed is more exhausting
            if (MovePoints < moveCost)
            {
                Send("You are too exhausted.");
                return false;
            }
            MovePoints -= moveCost;

            // Delay player by one pulse
            ImpersonatedBy?.SetGlobalCooldown(1);
            return true;
        }

        protected override void MoveFollow(IRoom fromRoom, IRoom toRoom, ExitDirections direction)
        {
            base.MoveFollow(fromRoom, toRoom, direction);

            if (fromRoom != toRoom)
            {
                IReadOnlyCollection<IPlayableCharacter> followers = new ReadOnlyCollection<IPlayableCharacter>(fromRoom.People.OfType<IPlayableCharacter>().Where(x => x.Leader == this).ToList()); // clone because Move will modify fromRoom.People
                foreach (IPlayableCharacter follower in followers)
                {
                    follower.Send("You follow {0}.", DebugName);
                    follower.Move(direction, true);
                }
            }
        }

        protected override void EnterFollow(IRoom wasRoom, IRoom destination, IItemPortal portal)
        {
            base.EnterFollow(wasRoom, destination, portal);

            if (wasRoom != destination)
            {
                IReadOnlyCollection<IPlayableCharacter> followers = new ReadOnlyCollection<IPlayableCharacter>(wasRoom.People.OfType<IPlayableCharacter>().Where(x => x.Leader == this).ToList()); // clone because Move will modify fromRoom.People
                foreach (IPlayableCharacter follower in followers)
                {
                    follower.Send("You follow {0}.", DebugName);
                    follower.Enter(portal, true);
                }
            }
        }

        protected override IEnumerable<ICharacter> GetActTargets(ActOptions option)
        {
            if (option == ActOptions.ToGroup)
            {
                if (Leader != null)
                    return Leader.GroupMembers; // !! GroupMembers doesn't include Leader
                return Enumerable.Empty<ICharacter>(); // NOP
            }
            return base.GetActTargets(option);
        }

        #endregion

        private void DeathPayoff() // Lose xp/reputation..
        {
            // TODO
        }
    }
}
