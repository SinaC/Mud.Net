using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Mud.Container;
using Mud.DataStructures.Trie;
using Mud.Domain;
using Mud.Domain.Extensions;
using Mud.Logger;
using Mud.Server.Abilities;
using Mud.Server.Common;
using Mud.Server.Input;
using Mud.Server.Item;
using Mud.Server.Quest;

namespace Mud.Server.Character.PlayableCharacter
{
    public partial class PlayableCharacter : CharacterBase, IPlayableCharacter
    {
        public static readonly int NoCondition = -1;
        public static readonly int MinCondition = 0;
        public static readonly int MaxCondition = 48;

        private static readonly Lazy<IReadOnlyTrie<CommandMethodInfo>> PlayableCharacterCommands = new Lazy<IReadOnlyTrie<CommandMethodInfo>>(GetCommands<PlayableCharacter>);

        protected IAdminManager AdminManager => DependencyContainer.Current.GetInstance<IAdminManager>();

        private readonly List<IPlayableCharacter> _groupMembers;
        private readonly List<IQuest> _quests;
        private readonly int[] _conditions;

        public PlayableCharacter(Guid guid, CharacterData data, IPlayer player, IRoom room)
            : base(guid, data.Name, string.Empty)
        {
            _groupMembers = new List<IPlayableCharacter>();
            _quests = new List<IQuest>();
            _conditions = new int[EnumHelpers.GetCount<Conditions>()];

            ImpersonatedBy = player;

            Room = World.NullRoom; // add in null room to avoid problem if an initializer needs a room

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
            Level = data.Level;
            Experience = data.Experience;
            SilverCoins = data.SilverCoins;
            GoldCoins = data.GoldCoins;
            HitPoints = data.HitPoints;
            MovePoints = data.MovePoints;
            if (data.CurrentResources != null)
            {
                foreach (var currentResourceData in data.CurrentResources)
                    this[currentResourceData.Key] = currentResourceData.Value;
            }
            else
            {
                Log.Default.WriteLine(LogLevels.Error, "PlayableCharacter.ctor: currentResources not found in pfile for {0}", data.Name);
                // set to 1 if not found
                foreach (ResourceKinds resource in EnumHelpers.GetValues<ResourceKinds>())
                    this[resource] = 1;
            }
            if (data.MaxResources != null)
            {
                foreach (var maxResourceData in data.MaxResources)
                    SetMaxResource(maxResourceData.Key, maxResourceData.Value, false);
            }
            Trains = data.Trains;
            Practices = data.Practices;
            // Conditions
            if (data.Conditions != null)
            {
                foreach (var conditionData in data.Conditions)
                    this[conditionData.Key] = conditionData.Value;
            }
            //
            BaseCharacterFlags = data.CharacterFlags;
            BaseImmunities = data.Immunities;
            BaseResistances = data.Resistances;
            BaseVulnerabilities = data.Vulnerabilities;
            BaseSex = data.Sex;
            BaseSize = data.Size;
            if (data.Attributes != null)
            {
                foreach (var attributeData in data.Attributes)
                    SetBaseAttributes(attributeData.Key, attributeData.Value, false);
            }
            else
            {
                Log.Default.WriteLine(LogLevels.Error, "PlayableCharacter.ctor: attributes not found in pfile for {0}", data.Name);
                // set to 1 if not found
                foreach (CharacterAttributes attribute in EnumHelpers.GetValues<CharacterAttributes>())
                    this[attribute] = 1;
            }

            // TODO: set not-found attributes to base value (from class/race)

            // Must be built before equiping
            BuildEquipmentSlots();

            // Equipped items
            if (data.Equipments != null)
            {
                // Create item in inventory and try to equip it
                foreach (EquippedItemData equippedItemData in data.Equipments)
                {
                    // Create in inventory
                    var item = World.AddItem(Guid.NewGuid(), equippedItemData.Item, this);

                    // Try to equip it
                    EquippedItem equippedItem = SearchEquipmentSlot(equippedItemData.Slot, false);
                    if (equippedItem != null)
                    {
                        if (item.WearLocation != WearLocations.None)
                        {
                            equippedItem.Item = item;
                            item.ChangeContainer(null); // remove from inventory
                            item.ChangeEquippedBy(this, false); // set as equipped by this
                        }
                        else
                        {
                            string msg = $"Item blueprint Id {equippedItemData.Item.ItemId} cannot be equipped anymore in slot {equippedItemData.Slot} for character {data.Name}.";
                            Log.Default.WriteLine(LogLevels.Error, msg, equippedItemData.Item.ItemId, equippedItemData.Slot);
                            Wiznet.Wiznet(msg, WiznetFlags.Bugs);
                        }
                    }
                    else
                    {
                        string msg = $"Item blueprint Id {equippedItemData.Item.ItemId} was supposed to be equipped in first empty slot {equippedItemData.Slot} for character {data.Name} but this slot doesn't exist anymore.";
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
            // Auras
            if (data.Auras != null)
            {
                foreach (AuraData auraData in data.Auras)
                    AddAura(new Aura.Aura(auraData), false); // TODO: !!! auras is not added thru World.AddAura
            }
            // Known abilities
            if (data.KnownAbilities != null)
            {
                foreach (KnownAbilityData knownAbilityData in data.KnownAbilities)
                {
                    IAbility ability = AbilityManager[knownAbilityData.AbilityId];
                    if (ability == null)
                        Log.Default.WriteLine(LogLevels.Error, "KnownAbility ability id {0} doesn't exist anymore", knownAbilityData.AbilityId);
                    else
                    {
                        KnownAbility knownAbility = new KnownAbility
                        {
                            Ability = ability,
                            ResourceKind = knownAbilityData.ResourceKind,
                            CostAmount = knownAbilityData.CostAmount,
                            CostAmountOperator = knownAbilityData.CostAmountOperator,
                            Level = knownAbilityData.Level,
                            Learned = knownAbilityData.Learned,
                            Rating = knownAbilityData.Rating
                        };
                        AddKnownAbility(knownAbility);
                    }
                }
            }

            //
            Room = room;
            room.Enter(this);

            RecomputeKnownAbilities();
            ResetCurrentAttributes();
            RecomputeCurrentResourceKinds();
        }

        #region IPlayableCharacter

        #region ICharacter

        #region IEntity

        #region IActor

        public override IReadOnlyTrie<CommandMethodInfo> Commands => PlayableCharacterCommands.Value;

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
                displayName.Append($" [PLR {ImpersonatedBy?.DisplayName ?? " ??? "}]");
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
            Room = World.NullRoom; // this will avoid a lot of problem, will be set to null in Cleanup phase
        }

        #endregion

        // Combat
        public override void MultiHit(ICharacter victim, IMultiHitModifier multiHitModifier) // 'this' starts a combat with 'victim'
        {
            // no attacks for stunnies
            if (Position <= Positions.Stunned)
                return;

            IItemWeapon mainHand = GetEquipment<IItemWeapon>(EquipmentSlots.MainHand);
            IItemWeapon offHand = GetEquipment<IItemWeapon>(EquipmentSlots.OffHand);
            // 1/ main hand attack
            OneHit(victim, mainHand, multiHitModifier);
            if (Fighting != victim)
                return;
            // 2/ off hand attack
            var dualWield = GetLearnInfo("Dual wield");
            int dualWieldChance = dualWield.learned;
            if (offHand != null && RandomManager.Chance(dualWieldChance))
            {
                OneHit(victim, offHand, multiHitModifier);
                CheckAbilityImprove(dualWield.knownAbility, true, 4);
            }
            if (Fighting != victim)
                return;
            if (multiHitModifier?.MaxAttackCount <= 1)
                return;
            // 3/ main hand haste attack
            if (CharacterFlags.HasFlag(CharacterFlags.Haste))
                OneHit(victim, mainHand, multiHitModifier);
            if (Fighting != victim)
                return;
            if (multiHitModifier?.MaxAttackCount <= 2)
                return;
            // 4/ main hand second attack
            var secondAttackLearnInfo = GetLearnInfo("Second attack");
            int secondAttackChance = secondAttackLearnInfo.learned/2;
            if (CharacterFlags.HasFlag(CharacterFlags.Slow))
                secondAttackChance /= 2;
            if (RandomManager.Chance(secondAttackChance))
            {
                OneHit(victim, mainHand, multiHitModifier);
                CheckAbilityImprove(secondAttackLearnInfo.knownAbility, true, 5);
            }
            if (Fighting != victim)
                return;
            if (multiHitModifier?.MaxAttackCount <= 3)
                return;
            // 5/ main hand third attack
            var thirdAttackLearnInfo = GetLearnInfo("Third attack");
            int thirdAttackChance = thirdAttackLearnInfo.learned/4;
            if (CharacterFlags.HasFlag(CharacterFlags.Slow))
                thirdAttackChance = 0;
            if (RandomManager.Chance(thirdAttackChance))
            {
                OneHit(victim, mainHand, multiHitModifier);
                CheckAbilityImprove(thirdAttackLearnInfo.knownAbility, true, 6);
            }
            if (Fighting != victim)
                return;
            if (multiHitModifier?.MaxAttackCount <= 4)
                return;
            // TODO: 2nd main hand, 2nd off hand, 4th, 5th, ... attack
        }

        public override void KillingPayoff(ICharacter victim) // Gain xp/gold/reputation/...
        {
            // Gain xp and alignment
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
                int sumLevels = members.Sum(x => x.Level); // TODO: NPC in group -> level / 2
                // Gain xp and change alignment
                foreach (IPlayableCharacter member in members)
                {
                    var gain = ComputeExperienceAndAlignment(victim, sumLevels);
                    if (gain.experience > 0)
                    {
                        member.Send("%y%You gain {0} experience points.%x%", gain.experience);
                        member.GainExperience(gain.experience);
                    }
                    member.UpdateAlignment(gain.alignment);
                }
                // TODO: gold, reputation
            }
        }

        public override void DeathPayoff(ICharacter killer) // Lose xp/reputation..
        {
            // TODO
        }

        // Abilities
        public override (int learned, KnownAbility knownAbility) GetWeaponLearnInfo(IItemWeapon weapon)
        {
            IAbility ability = null;
            if (weapon == null)
                ability = AbilityManager["Hand to hand"];
            else
            {
                switch (weapon.Type)
                {
                    case WeaponTypes.Exotic:
                        // no ability
                        break;
                    case WeaponTypes.Sword:
                        ability = AbilityManager["Sword"];
                        break;
                    case WeaponTypes.Dagger:
                        ability = AbilityManager["Dagger"];
                        break;
                    case WeaponTypes.Spear:
                        ability = AbilityManager["Spear"];
                        break;
                    case WeaponTypes.Mace:
                        ability = AbilityManager["Mace"];
                        break;
                    case WeaponTypes.Axe:
                        ability = AbilityManager["Axe"];
                        break;
                    case WeaponTypes.Flail:
                        ability = AbilityManager["Flail"];
                        break;
                    case WeaponTypes.Whip:
                        ability = AbilityManager["Whip"];
                        break;
                    case WeaponTypes.Polearm:
                        ability = AbilityManager["Polearm"];
                        break;
                    case WeaponTypes.Staff:
                        ability = AbilityManager["Staff(weapon)"];
                        break;
                    default:
                        Log.Default.WriteLine(LogLevels.Error, "PlayableCharacter.GetWeaponLearned: Invalid WeaponType {0}", weapon.Type);
                        break;
                }
            }

            // no ability -> 3*level
            if (ability == null)
            {
                int learned = (3 * Level).Range(0, 100);
                return (learned, null);
            }
            // ability, check %
            var learnInfo = GetLearnInfo(ability);
            return learnInfo;
        }

        public override (int learned, KnownAbility knownAbility) GetLearnInfo(IAbility ability)
        {
            KnownAbility knownAbility = this[ability];
            int learned = 0;
            if (knownAbility != null && knownAbility.Level <= Level)
                learned = knownAbility.Learned;

            // TODO: if daze /=2 for spell and *2/3 if otherwise

            if (this[Conditions.Drunk] > 10)
                learned = (learned * 9 ) / 10;

            return (learned.Range(0, 100), knownAbility);
        }

        #endregion

        public DateTime CreationTime { get; protected set; }

        public long ExperienceToLevel =>
            Level >= Settings.MaxLevel
                ? 0
                : (ExperienceByLevel * Level) - Experience;

        public long Experience { get; protected set; }

        public int Trains { get; protected set; }

        public int Practices { get; protected set; }

        public int this[Conditions condition]
        {
            get
            {
                int index = (int)condition;
                if (index >= _conditions.Length)
                {
                    Log.Default.WriteLine(LogLevels.Error, "Trying to get current condition for condition {0} (index {1}) but current condition length is smaller", condition, index);
                    return NoCondition;
                }
                return _conditions[index];
            }
            protected set
            {
                int index = (int)condition;
                if (index >= _conditions.Length)
                {
                    Log.Default.WriteLine(LogLevels.Error, "Trying to get current condition for condition {0} (index {1}) but current condition length is smaller", condition, index);
                    return;
                }
                _conditions[index] = value;
            }
        }

        public void GainCondition(Conditions condition, int value)
        {
            if (value == 0)
                return;
            if (ImpersonatedBy?.IsAfk == true)
                return;
            if (Room == null)
                return;
            // TODO: if undead or ghost
            if (this[condition] == NoCondition)
                return;
            int previousValue = this[condition];
            this[condition] = (previousValue + value).Range(MinCondition, MaxCondition);
            if (this[condition] == MinCondition)
            {
                switch (condition)
                {
                    case Conditions.Hunger:
                        Send("You are hungry.");
                        break;
                    case Conditions.Thirst:
                        Send("You are thirsty.");
                        break;
                    case Conditions.Drunk:
                        if (previousValue != 0)
                            Send("You are sober.");
                        break;
                }
            }
        }

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
        public IRoom RecallRoom => World.DefaultRecallRoom; // TODO: could be different from default one

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
                Log.Default.WriteLine(LogLevels.Warning, "ICharacter.ChangeLeader: {0} is not valid anymore", DebugName);
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
                Log.Default.WriteLine(LogLevels.Warning, "ICharacter.AddGroupMember: {0} is not valid anymore", DebugName);
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
                Log.Default.WriteLine(LogLevels.Error, "ICharacter:StopFollower: {0} is not following {1} but {2}", follower.DebugName, DebugName, follower.Leader?.DebugName ?? "<<none>>");
                return false;
            }
            follower.ChangeLeader(null);
            if (CanSee(follower))
                Act(ActOptions.ToCharacter, "{0} stops following you.", follower);
            follower.Act(ActOptions.ToCharacter, "You stop following {0}.", this);
            return true;
        }

        // Impersonation
        public bool StopImpersonation()
        {
            if (!IsValid)
            {
                Log.Default.WriteLine(LogLevels.Warning, "ICharacter.StopImpersonation: {0} is not valid anymore", DebugName);
                ImpersonatedBy = null;
                return false;
            }

            Log.Default.WriteLine(LogLevels.Debug, "ICharacter.StopImpersonation: {0} old: {1};", DebugName, ImpersonatedBy?.DisplayName ?? "<<none>>");
            ImpersonatedBy = null;
            RecomputeKnownAbilities();
            Recompute();
            return true;
        }

        // Combat
        public void GainExperience(long experience)
        {
            if (Level < Settings.MaxLevel)
            {
                bool recompute = false;
                Experience = Math.Max(ExperienceByLevel * (Level-1), Experience + experience); // don't go below current level
                // Raise level
                if (experience > 0)
                {
                    // In case multiple level are gain, check max level
                    while (ExperienceToLevel <= 0 && Level < Settings.MaxLevel)
                    {
                        recompute = true;
                        Level++;
                        Wiznet.Wiznet($"{DebugName} has attained level {Level}", WiznetFlags.Levels);
                        Send("You raise a level!!");
                        Act(ActOptions.ToGroup, "{0} has attained level {1}", this, Level);
                        AdvanceLevel();
                    }
                }
                if (recompute)
                {
                    RecomputeKnownAbilities();
                    Recompute();
                    // Bonus -> reset cooldown and set resource to max
                    ResetCooldowns();
                    ImpersonatedBy?.Save(); // Force a save when a level is gained
                }
            }
        }

        // Ability
        public bool CheckAbilityImprove(KnownAbility knownAbility, bool abilityUsedSuccessfully, int multiplier)
        {
            // Know ability ?
            if (knownAbility == null
                || knownAbility.Ability == null
                || knownAbility.Learned == 0
                || knownAbility.Learned == 100)
                return false; // ability not known
            // check to see if the character has a chance to learn
            if (multiplier <= 0)
            {
                Log.Default.WriteLine(LogLevels.Error, "PlayableCharacter.CheckAbilityImprove: multiplier had invalid value {0}", multiplier);
                multiplier = 1;
            }
            int difficultyMultiplier = knownAbility.Rating;
            if (difficultyMultiplier <= 0)
            {
                Log.Default.WriteLine(LogLevels.Error, "PlayableCharacter.CheckAbilityImprove: difficulty multiplier had invalid value {0} for KnownAbility {1} Player {2}", multiplier, knownAbility.Ability, DebugName);
                difficultyMultiplier = 1;
            }
            // TODO: percentage depends on intelligence replace CurrentAttributes(CharacterAttributes.Intelligence) with values from 3 to 85
            int chance = 10 * TableValues.LearnBonus(this) / (multiplier * difficultyMultiplier * 4) + Level;
            if (RandomManager.Range(1, 1000) > chance)
                return false;
            // now that the character has a CHANCE to learn, see if they really have
            if (abilityUsedSuccessfully)
            {
                chance = (100 - knownAbility.Learned).Range(5, 95);
                if (RandomManager.Chance(chance))
                {
                    Send("You have become better at {0}!", knownAbility.Ability.Name);
                    knownAbility.Learned++;
                    GainExperience(2 * difficultyMultiplier);
                    return true;
                }
            }
            else
            {
                chance = (knownAbility.Learned / 2).Range(5, 30);
                if (RandomManager.Chance(chance))
                {
                    Send("You learn from your mistakes, and your {0} skill improves.!", knownAbility.Ability.Name);
                    int learned = RandomManager.Range(1, 3);
                    knownAbility.Learned = Math.Min(knownAbility.Learned + learned, 100);
                    GainExperience(2 * difficultyMultiplier);
                    return true;
                }
            }

            return false;
        }

        // Mapping
        public CharacterData MapCharacterData()
        {
            CharacterData data = new CharacterData
            {
                CreationTime = CreationTime,
                Name = Name,
                RoomId = Room?.Blueprint?.Id ?? 0,
                Race = Race?.Name ?? string.Empty,
                Class = Class?.Name ?? string.Empty,
                Level = Level,
                Sex = BaseSex,
                Size = BaseSize,
                SilverCoins = SilverCoins,
                GoldCoins = GoldCoins,
                HitPoints = HitPoints,
                MovePoints = MovePoints,
                CurrentResources = EnumHelpers.GetValues<ResourceKinds>().ToDictionary(x => x, x => this[x]),
                MaxResources = EnumHelpers.GetValues<ResourceKinds>().ToDictionary(x => x, MaxResource),
                Experience = Experience,
                Trains = Trains,
                Practices = Practices,
                Conditions = EnumHelpers.GetValues<Conditions>().ToDictionary(x => x, x => this[x]),
                Equipments = Equipments.Where(x => x.Item != null).Select(x => x.MapEquippedData()).ToArray(),
                Inventory = Inventory.Select(x => x.MapItemData()).ToArray(),
                CurrentQuests = Quests.Select(x => x.MapQuestData()).ToArray(),
                Auras = MapAuraData(),
                CharacterFlags = BaseCharacterFlags,
                Immunities = BaseImmunities,
                Resistances = BaseResistances,
                Vulnerabilities = BaseVulnerabilities,
                Attributes = EnumHelpers.GetValues<CharacterAttributes>().ToDictionary(x => x, BaseAttribute),
                KnownAbilities = KnownAbilities.Select(x => x.MapKnownAbilityData()).ToArray(),
                // TODO: cooldown, ...
            };
            return data;
        }

        #endregion

        #region ActorBase

        protected override bool ExecuteBeforeCommand(CommandMethodInfo methodInfo, string rawParameters, params CommandParameter[] parameters)
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

        #endregion

        #region CharacterBase

        protected override bool AutomaticallyDisplayRoom => true;

        protected override (int hitGain, int moveGain, int manaGain) RegenBaseValues()
        {
            int hitGain = Math.Max(3, this[CharacterAttributes.Constitution] - 3 + Level / 2);
            int moveGain = Math.Max(15, Level);
            int manaGain = (this[CharacterAttributes.Wisdom] + this[CharacterAttributes.Intelligence] + Level) / 2;
            // regen
            if (CharacterFlags.HasFlag(CharacterFlags.Regeneration))
                hitGain *= 2;
            // class bonus
            hitGain += (Class?.MaxHitPointGainPerLevel ?? 0) - 10;
            // fast healing
            (int learned, KnownAbility knownAbility) fastHealing = GetLearnInfo("Fast healing");
            if (RandomManager.Chance(fastHealing.learned))
            {
                hitGain += (fastHealing.learned * hitGain) / 100;
                if (HitPoints < MaxHitPoints)
                    CheckAbilityImprove(fastHealing.knownAbility, true, 8);
            }
            // meditation
            (int learned, KnownAbility knownAbility) meditation = GetLearnInfo("Meditation");
            if (RandomManager.Chance(meditation.learned))
            {
                manaGain += (meditation.learned * manaGain) / 100;
                if (this[ResourceKinds.Mana] < MaxResource(ResourceKinds.Mana))
                    CheckAbilityImprove(meditation.knownAbility, true, 8);
            }
            // position
            switch (Position)
            {
                case Positions.Sleeping:
                    moveGain += this[CharacterAttributes.Dexterity];
                    break;
                case Positions.Resting:
                    hitGain /= 2;
                    moveGain += this[CharacterAttributes.Dexterity] / 2;
                    manaGain /= 2;
                    break;
                case Positions.Fighting:
                    hitGain /= 6;
                    manaGain /= 6;
                    break;
                default:
                    hitGain /= 4;
                    manaGain /= 4;
                    break;
            }
            if (this[Conditions.Hunger] == 0)
            {
                hitGain /= 2;
                manaGain /= 2;
                moveGain /= 2;
            }
            if (this[Conditions.Thirst] == 0)
            {
                hitGain /= 2;
                manaGain /= 2;
                moveGain /= 2;
            }
            return (hitGain, moveGain, manaGain);
        }

        protected override ExitDirections ChangeDirectionBeforeMove(ExitDirections direction, IRoom fromRoom)
        {
            // Drunk enough to change direction ?
            int drunkLevel = this[Conditions.Drunk];
            if (drunkLevel > 10)
            {
                if (RandomManager.Chance(drunkLevel))
                {
                    Act(ActOptions.ToCharacter, "You feel a little drunk.. not to mention kind of lost..", this);
                    Act(ActOptions.ToRoom, "{0:N} looks a little drunk.. not to mention kind of lost..", this);
                    direction = RandomManager.Random<ExitDirections>(); // change direction
                }
                else
                {
                    Act(ActOptions.ToCharacter, "You feel a little.. drunk..", this);
                    Act(ActOptions.ToRoom, "{0:N} looks a little.. drunk..", this);
                }
            }
            return direction;
        }

        protected override bool BeforeMove(ExitDirections direction, IRoom fromRoom, IRoom toRoom)
        {
            // Compute move and check if enough move left
            int moveCost = TableValues.MovementLoss(fromRoom.SectorType) + TableValues.MovementLoss(toRoom.SectorType);
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

            // Drunk ?
            if (this[Conditions.Drunk] > 10)
                Act(ActOptions.ToRoom, "{0:N} stumbles off drunkenly on {0:s} way {1}.", this, direction.DisplayName());
            return true;
        }

        protected override void AfterMove(ExitDirections direction, IRoom fromRoom, IRoom toRoom)
        {
            // Drunk?
            if (this[Conditions.Drunk] > 10)
                Act(ActOptions.ToRoom, "{0:N} stumbles in drunkenly, looking all nice and French.", this);

            // Autolook
            AutoLook();
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

        protected override void HandleDeath()
        {
            ChangePosition(Positions.Resting);
            HitPoints = 1;
            MovePoints = 1;
            foreach (var resourceKind in EnumHelpers.GetValues<ResourceKinds>())
                this[resourceKind] = 1;
            if (ImpersonatedBy != null) // If impersonated, no real death
            {
                IRoom room = World.DefaultDeathRoom ?? World.DefaultRecallRoom;
                ChangeRoom(room);
                Recompute(); // don't reset hp
            }
            else // If not impersonated, remove from game // TODO: this can be a really bad idea
            {
                World.RemoveCharacter(this);
            }
        }

        protected override void HandleWimpy(int damage)
        {
            // TODO
            //if (HitPoints > 0 && HitPoints <= Wimpy) // TODO: test on wait < PULSE_VIOLENCE / 2
            //  DoFlee(null, null);
        }

        protected override (int thac0_00, int thac0_32) GetThac0()
        {
            if (Class != null)
                return Class.Thac0;
            return (20, 0);
        }

        protected override int NoWeaponBaseDamage
        {
            get
            {
                var hand2HandLearnInfo = GetLearnInfo("Hand to hand");
                int learned = hand2HandLearnInfo.learned;
                return RandomManager.Range(1 + 4 * learned / 100, 2 * Level / 3 * learned / 100);
            }
        }

        protected override string NoWeaponDamageNoun => "hit";

        #endregion

        private int ExperienceByLevel => 1000 * (Race?.ClassExperiencePercentageMultiplier(Class) ?? 100) / 100;

        private (int experience, int alignment) ComputeExperienceAndAlignment(ICharacter victim, int totalLevel)
        {
            int experience = 0;
            int alignment = 0;

            int levelDiff = victim.Level - Level;
            // compute base experience
            int baseExp = 0;
            switch (levelDiff)
            {
                default: baseExp = 0; break;
                case -9: baseExp = 1; break;
                case -8: baseExp = 2; break;
                case -7: baseExp = 5; break;
                case -6: baseExp = 9; break;
                case -5: baseExp = 11; break;
                case -4: baseExp = 22; break;
                case -3: baseExp = 33; break;
                case -2: baseExp = 50; break;
                case -1: baseExp = 66; break;
                case 0: baseExp = 83; break;
                case 1: baseExp = 99; break;
                case 2: baseExp = 121; break;
                case 3: baseExp = 143; break;
                case 4: baseExp = 165; break;
            }

            if (levelDiff > 4)
                baseExp = 160 + 20 * (levelDiff - 4);
            // do alignment computation
            bool noAlign = victim is INonPlayableCharacter npcVictim && npcVictim.ActFlags.HasFlag(ActFlags.NoAlign);
            int alignDiff = victim.Alignment - Alignment;
            if (!noAlign)
            {
                if (alignDiff > 500) // more good than member
                    alignment = -Math.Max(1, (alignDiff - 500) * baseExp / 500 * Level / totalLevel); // become move evil
                else if (alignDiff < -500) // more evil than member
                    alignment = Math.Max(1, (-alignDiff - 500) * baseExp / 500 * Level / totalLevel); // become more good
                else
                    alignment = -(Alignment * baseExp / 500 * Level / totalLevel); // improve this someday
            }
            // change exp based on alignment
            if (noAlign)
                experience = baseExp;
            else if (Alignment > 500) // for goodie
            {
                if (victim.Alignment < -750)
                    experience = (baseExp * 4) / 3;
                if (victim.Alignment < -500)
                    experience = (baseExp * 5) / 4;
                else if (victim.Alignment > 750)
                    experience = baseExp / 4;
                else if (victim.Alignment > 500)
                    experience = baseExp / 2;
                else if (victim.Alignment > 250)
                    experience = (baseExp * 3) / 4;
                else
                    experience = baseExp;
            }
            else if (Alignment < -500) // for baddie
            {
                if (victim.Alignment > 750)
                    experience = (baseExp * 5) / 4;
                if (victim.Alignment > 500)
                    experience = (baseExp * 11) / 10;
                else if (victim.Alignment < -750)
                    experience = baseExp / 2;
                else if (victim.Alignment < -500)
                    experience = (baseExp * 3) / 4;
                else if (victim.Alignment < -250)
                    experience = (baseExp * 9) / 10;
                else
                    experience = baseExp;
            }
            else if (Alignment > 200) // little good
            {
                if (victim.Alignment < -500)
                    experience = (baseExp * 6) / 5;
                else if (victim.Alignment > 750)
                    experience = baseExp / 2;
                else if (victim.Alignment > 0)
                    experience = (baseExp * 3) / 4;
                else
                    experience = baseExp;
            }
            else if (Alignment < -200) // little bad
            {
                if (victim.Alignment > 500)
                    experience = (baseExp * 6) / 5;
                else if (victim.Alignment < -750)
                    experience = baseExp / 2;
                else if (victim.Alignment < 0)
                    experience = (baseExp * 3) / 4;
                else
                    experience = baseExp;
            }
            else // neutral
            {
                if (victim.Alignment > 500 || victim.Alignment < -500)
                    experience = (baseExp * 4) / 3;
                else if (victim.Alignment > 200 || victim.Alignment < -200)
                    experience = baseExp / 2;
                else
                    experience = baseExp;
            }
            // more xp at low level
            if (Level < 6)
                experience = 10 * experience / (Level + 4);
            // less xp at high level
            if (Level > 35)
                experience = 15 * experience / (Level - 25);
            // TODO: depends on playing time since last level
            // randomize
            experience = RandomManager.Range((experience * 3) / 4, (experience * 5) / 4);
            // adjust for grouping
            experience = experience * Level / Math.Max(1, totalLevel - 1);

            return (experience, alignment);
        }

        private void AdvanceLevel()
        {
            var bonus = TableValues.Bonus(this);

            //
            int addHitpoints = bonus.hitpoint + RandomManager.Range(Class?.MinHitPointGainPerLevel ?? 0, Class?.MaxHitPointGainPerLevel ?? 1);
            int addMana = RandomManager.Range(2, 2 * this[CharacterAttributes.Intelligence] + this[CharacterAttributes.Wisdom]);
            if (Class?.ResourceKinds.Contains(ResourceKinds.Mana) == false)
                addMana /= 2;
            // TODO: other resources
            int addMove = RandomManager.Range(1, this[CharacterAttributes.Constitution] + this[CharacterAttributes.Dexterity] / 6);

            addHitpoints = (addHitpoints * 9) / 10;
            addMana = (addMana * 9) / 10;
            addMove = (addMove * 9) / 10;

            addHitpoints = Math.Max(2, addHitpoints);
            addMana = Math.Max(2, addMana);
            addMove = Math.Max(6, addMove);

            int addPractice = bonus.practice;

            SetBaseAttributes(CharacterAttributes.MaxHitPoints, MaxHitPoints + addHitpoints, false);
            SetMaxResource(ResourceKinds.Mana, this[ResourceKinds.Mana] + addMana, false);
            SetBaseAttributes(CharacterAttributes.MaxMovePoints, MaxMovePoints + addHitpoints, false);
            Practices += addPractice;
            Trains++;

            Send("You gain {0} hit point{1}, {2} mana, {3} move, and {4} practice{5}.", addHitpoints, addHitpoints == 1 ? "" : "s", addMana, addMove, addPractice, addPractice == 1 ? "" : "s");
            // Inform about new abilities
            KnownAbility[] newAbilities = KnownAbilities.Where(x => x.Level == Level && x.Learned == 0).ToArray();
            if (newAbilities.Any())
            {
                StringBuilder sb = new StringBuilder("You can now gain following abilities: %y%");
                foreach (KnownAbility knownAbility in newAbilities)
                {
                    sb.Append(knownAbility.Ability.Name);
                    sb.Append(", ");
                }
                sb.Remove(sb.Length - 2, 2); // remove trailing comma
                sb.AppendLine("%x%");
                Send(sb);
            }
        }
    }
}
