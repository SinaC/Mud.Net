using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Mud.Common;
using Mud.Container;
using Mud.DataStructures.Trie;
using Mud.Domain;
using Mud.Domain.Extensions;
using Mud.Logger;
using Mud.Server.Ability;
using Mud.Server.Blueprints.Character;
using Mud.Server.Input;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Admin;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Class;
using Mud.Server.Interfaces.Item;
using Mud.Server.Interfaces.Player;
using Mud.Server.Interfaces.Quest;
using Mud.Server.Interfaces.Race;
using Mud.Server.Interfaces.Room;
using Mud.Server.Quest;

namespace Mud.Server.Character.PlayableCharacter
{
    public partial class PlayableCharacter : CharacterBase, IPlayableCharacter
    {
        public static readonly int NoCondition = -1;
        public static readonly int MinCondition = 0;
        public static readonly int MaxCondition = 48;

        private static readonly Lazy<IReadOnlyTrie<CommandMethodInfo>> PlayableCharacterCommands = new Lazy<IReadOnlyTrie<CommandMethodInfo>>(GetCommands<PlayableCharacter>);

        protected IClassManager ClassManager => DependencyContainer.Current.GetInstance<IClassManager>();
        protected IRaceManager RaceManager => DependencyContainer.Current.GetInstance<IRaceManager>();
        protected IAdminManager AdminManager => DependencyContainer.Current.GetInstance<IAdminManager>();

        private readonly List<IQuest> _quests;
        private readonly int[] _conditions;
        private readonly Dictionary<string, string> _aliases;
        private readonly List<INonPlayableCharacter> _pets;

        public PlayableCharacter(Guid guid, PlayableCharacterData data, IPlayer player, IRoom room)
            : base(guid, data.Name, string.Empty)
        {
            _quests = new List<IQuest>();
            _conditions = new int[EnumHelpers.GetCount<Conditions>()];
            _aliases = new Dictionary<string, string>();
            _pets = new List<INonPlayableCharacter>();

            ImpersonatedBy = player;

            Room = World.NullRoom; // add in null room to avoid problem if an initializer needs a room

            // Extract informations from PlayableCharacterData
            CreationTime = data.CreationTime;
            Class = ClassManager[data.Class];
            if (Class == null)
            {
                Class = ClassManager.Classes.First();
                Wiznet.Wiznet($"Invalid class '{data.Class}' for character {data.Name}!!", WiznetFlags.Bugs, AdminLevels.Implementor);
            }
            Race = RaceManager[data.Race];
            if (Race == null)
            {
                Race = RaceManager.PlayableRaces.First();
                Wiznet.Wiznet($"Invalid race '{data.Race}' for character {data.Name}!!", WiznetFlags.Bugs, AdminLevels.Implementor);
            }
            Level = data.Level;
            Experience = data.Experience;
            SilverCoins = data.SilverCoins;
            GoldCoins = data.GoldCoins;
            HitPoints = data.HitPoints;
            MovePoints = data.MovePoints;
            Alignment = data.Alignment;
            if (data.CurrentResources != null)
            {
                foreach (var currentResourceData in data.CurrentResources)
                    this[currentResourceData.Key] = currentResourceData.Value;
            }
            else
            {
                Wiznet.Wiznet($"PlayableCharacter.ctor: currentResources not found in pfile for {data.Name}", WiznetFlags.Bugs, AdminLevels.Implementor);
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
            AutoFlags = data.AutoFlags;
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
                Wiznet.Wiznet($"PlayableCharacter.ctor: attributes not found in pfile for {data.Name}", WiznetFlags.Bugs, AdminLevels.Implementor);
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
                    var item = ItemManager.AddItem(Guid.NewGuid(), equippedItemData.Item, this);

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
                            Wiznet.Wiznet($"Item blueprint Id {equippedItemData.Item.ItemId} cannot be equipped anymore in slot {equippedItemData.Slot} for character {data.Name}.", WiznetFlags.Bugs, AdminLevels.Implementor);
                        }
                    }
                    else
                    {
                        Wiznet.Wiznet($"Item blueprint Id {equippedItemData.Item.ItemId} was supposed to be equipped in first empty slot {equippedItemData.Slot} for character {data.Name} but this slot doesn't exist anymore.", WiznetFlags.Bugs, AdminLevels.Implementor);
                    }
                }
            }
            // Inventory
            if (data.Inventory != null)
            {
                foreach (ItemData itemData in data.Inventory)
                    ItemManager.AddItem(Guid.NewGuid(), itemData, this);
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
            // Learn abilities
            if (data.LearnedAbilities != null)
            {
                foreach (LearnedAbilityData learnedAbilityData in data.LearnedAbilities)
                {
                    IAbilityInfo abilityInfo = AbilityManager[learnedAbilityData.Name];
                    if (abilityInfo == null)
                        Wiznet.Wiznet($"LearnedAbility:  Ability {learnedAbilityData.Name} doesn't exist anymore", WiznetFlags.Bugs, AdminLevels.Implementor);
                    else
                    {
                        AbilityLearned abilityLearned = new AbilityLearned(learnedAbilityData, abilityInfo);
                        AddLearnedAbility(abilityLearned);
                    }
                }
            }
            // Aliases
            if (data.Aliases != null)
            {
                foreach (KeyValuePair<string, string> alias in data.Aliases)
                    _aliases.Add(alias.Key, alias.Value);
            }
            // Cooldowns
            if (data.Cooldowns != null)
            {
                foreach (KeyValuePair<string, int> cooldown in data.Cooldowns)
                {
                    IAbilityInfo abilityInfo = AbilityManager[cooldown.Key];
                    if (abilityInfo == null)
                        Wiznet.Wiznet($"Cooldown: ability {cooldown.Key} doesn't exist anymore", WiznetFlags.Bugs, AdminLevels.Implementor);
                    else
                        SetCooldown(cooldown.Key, cooldown.Value);
                }
            }
            // Pets
            if (data.Pets != null)
            {
                foreach (PetData petData in data.Pets)
                {
                    CharacterNormalBlueprint blueprint = World.GetCharacterBlueprint<CharacterNormalBlueprint>(petData.BlueprintId);
                    if (blueprint == null)
                    {
                        Wiznet.Wiznet($"Pet blueprint id {petData.BlueprintId} doesn't exist anymore", WiznetFlags.Bugs, AdminLevels.Implementor);
                    }
                    else
                    {
                        INonPlayableCharacter pet = World.AddNonPlayableCharacter(Guid.NewGuid(), blueprint, petData, room);
                        AddPet(pet);
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
            if (beholder is IPlayableCharacter playableBeholder && playableBeholder.IsImmortal)
                displayName.Append($" [PLR {ImpersonatedBy?.DisplayName ?? " ??? "}]");
            return displayName.ToString();
        }


        public override void OnRemoved() // called before removing a character from the game
        {
            base.OnRemoved();

            StopFighting(true);

            // Leave group
            if (Group != null)
            {
                if (Group.Members.Count() <= 2) // group will contain only one member, disband
                    Group.Disband();
                else
                    Group.RemoveMember(this);
            }

            // Release pets
            foreach (INonPlayableCharacter pet in _pets)
            {
                if (pet.Room != null)
                    pet.Act(ActOptions.ToRoom, "{0:N} slowly fades away.", pet);
                RemoveFollower(pet);
                pet.ChangeMaster(null);
                World.RemoveCharacter(pet);
            }
            _pets.Clear();

            // TODO: what if character is incarnated
            ImpersonatedBy?.StopImpersonating();
            ImpersonatedBy = null; // TODO: warn ImpersonatedBy ?
            ResetCooldowns();
            DeleteInventory();
            DeleteEquipments();
            Room = World.NullRoom; // this will avoid a lot of problem, will be set to null in Cleanup phase
        }

        #endregion

        public override int MaxCarryWeight => IsImmortal
            ? 10000000
            : base.MaxCarryWeight;

        public override int MaxCarryNumber => IsImmortal
            ? 1000
            : base.MaxCarryNumber;

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
            var dualWield = AbilityManager.CreateInstance<IPassive>("Dual Wield");
            if (offHand != null && dualWield.IsTriggered(this, victim, true, out _, out _))
                OneHit(victim, offHand, multiHitModifier);
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
            var secondAttack = AbilityManager.CreateInstance<IPassive>("Second Attack");
            if (secondAttack.IsTriggered(this, victim, true, out _, out _))
                OneHit(victim, mainHand, multiHitModifier);
            if (Fighting != victim)
                return;
            if (multiHitModifier?.MaxAttackCount <= 3)
                return;
            // 5/ main hand third attack
            var thirdAttack = AbilityManager.CreateInstance<IPassive>("Third Attack");
            if (thirdAttack.IsTriggered(this, victim, true, out _, out _))
                OneHit(victim, mainHand, multiHitModifier);
            if (Fighting != victim)
                return;
            if (multiHitModifier?.MaxAttackCount <= 4)
                return;
            // TODO: 2nd main hand, 2nd off hand, 4th, 5th, ... attack
            // TODO: only if wielding 3 or 4 weapons
            //var thirdWieldLearnInfo = GetLearnInfo("Third wield");
            //var thirdWieldChance = thirdWieldLearnInfo.learned / 6;
            //if (CharacterFlags.HasFlag(CharacterFlags.Slow))
            //    thirdWieldChance = 0;
            //if (RandomManager.Chance(thirdWieldChance))
            //{
            //    OneHit(victim, mainHand, multiHitModifier);
            //    CheckAbilityImprove(thirdWieldLearnInfo.knownAbility, true, 6);
            //}
            //if (Fighting != victim)
            //    return;
            //if (multiHitModifier?.MaxAttackCount <= 5)
            //    return;
            //var FourthWieldLearnInfo = GetLearnInfo("Fourth wield");
            //var FourthWieldChance = FourthWieldLearnInfo.learned / 8;
            //if (CharacterFlags.HasFlag(CharacterFlags.Slow))
            //    FourthWieldChance = 0;
            //if (RandomManager.Chance(FourthWieldChance))
            //{
            //    OneHit(victim, mainHand, multiHitModifier);
            //    CheckAbilityImprove(FourthWieldLearnInfo.knownAbility, true, 6);
            //}
            //if (Fighting != victim)
            //    return;
            //if (multiHitModifier?.MaxAttackCount <= 6)
            //    return;
        }

        public override void KillingPayoff(ICharacter victim, IItemCorpse corpse) // Gain xp/gold/reputation/...
        {
            // Gain xp and alignment
            if (this != victim && victim is INonPlayableCharacter) // gain xp only for non-playable victim
            {
                IPlayableCharacter[] members = (Group?.Members ?? this.Yield()).ToArray();
                int sumLevels = members.Sum(x => x.Level);
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
                // TODO: reputation
                if (corpse != null && !corpse.IsPlayableCharacterCorpse)
                {
                    // autoloot
                    if (AutoFlags.HasFlag(AutoFlags.Loot) && corpse.Content.Any())
                    {
                        IReadOnlyCollection<IItem> corpseContent = new ReadOnlyCollection<IItem>(corpse.Content.Where(CanSee).ToList());
                        foreach (IItem item in corpseContent)
                            GetItem(item, corpse);
                    }

                    // autogold
                    if (AutoFlags.HasFlag(AutoFlags.Gold) && corpse.Content.Any())
                    {
                        IReadOnlyCollection<IItemMoney> corpseContent = new ReadOnlyCollection<IItemMoney>(corpse.Content.OfType<IItemMoney>().Where(CanSee).ToList());
                        foreach (IItemMoney money in corpseContent)
                            GetItem(money, corpse);
                    }

                    // autosac
                    if (AutoFlags.HasFlag(AutoFlags.Sacrifice) && !corpse.Content.Any()) // TODO: corpse empty only if autoloot is set?
                    {
                        SacrificeItem(corpse);
                    }
                }
            }
        }

        public override void DeathPayoff(ICharacter killer) // Lose xp/reputation..
        {
            // 5/6 way back to previous level.
            var loss = -5 * ExperienceToLevel / 6;
            GainExperience(loss);
        }

        // Abilities
        public override (int percentage, IAbilityLearned abilityLearned) GetWeaponLearnedInfo(IItemWeapon weapon)
        {
            string abilityName = null;
            if (weapon == null)
                abilityName = "Hand to hand";
            else
            {
                switch (weapon.Type)
                {
                    case WeaponTypes.Exotic:
                        // no ability
                        break;
                    case WeaponTypes.Sword:
                        abilityName = "Sword";
                        break;
                    case WeaponTypes.Dagger:
                        abilityName = "Dagger";
                        break;
                    case WeaponTypes.Spear:
                        abilityName = "Spear";
                        break;
                    case WeaponTypes.Mace:
                        abilityName = "Mace";
                        break;
                    case WeaponTypes.Axe:
                        abilityName = "Axe";
                        break;
                    case WeaponTypes.Flail:
                        abilityName = "Flail";
                        break;
                    case WeaponTypes.Whip:
                        abilityName = "Whip";
                        break;
                    case WeaponTypes.Polearm:
                        abilityName = "Polearm";
                        break;
                    case WeaponTypes.Staff:
                        abilityName = "Staff(weapon)";
                        break;
                    default:
                        Wiznet.Wiznet($"PlayableCharacter.GetWeaponLearned: Invalid WeaponType {weapon.Type}", WiznetFlags.Bugs, AdminLevels.Implementor);
                        break;
                }
            }

            // no ability -> 3*level
            if (abilityName == null)
            {
                int learned = (3 * Level).Range(0, 100);
                return (learned, null);
            }
            // ability, check %
            var learnInfo = GetAbilityLearnedInfo(abilityName);
            return learnInfo;
        }

        public override (int percentage, IAbilityLearned abilityLearned) GetAbilityLearnedInfo(string abilityName)
        {
            IAbilityLearned learnedAbility = GetAbilityLearned(abilityName);
            int learned = 0;
            if (learnedAbility != null && learnedAbility.Level <= Level)
                learned = learnedAbility.Learned;

            // TODO: if daze /=2 for spell and *2/3 if otherwise

            if (this[Conditions.Drunk] > 10)
                learned = (learned * 9 ) / 10;

            return (learned.Range(0, 100), learnedAbility);
        }

        #endregion

        public IReadOnlyDictionary<string, string> Aliases => _aliases;

        public DateTime CreationTime { get; protected set; }

        public long ExperienceToLevel =>
            Level >= Settings.MaxLevel
                ? 0
                : (ExperienceByLevel * Level) - Experience;

        public bool IsImmortal { get; protected set; }

        public long Experience { get; protected set; }

        public int Trains { get; protected set; }

        public int Practices { get; protected set; }

        public AutoFlags AutoFlags { get; protected set; }

        public int this[Conditions condition]
        {
            get
            {
                int index = (int)condition;
                if (index >= _conditions.Length)
                {
                    Wiznet.Wiznet($"Trying to get current condition for condition {condition} (index {index}) but current condition length is smaller", WiznetFlags.Bugs, AdminLevels.Implementor);
                    return NoCondition;
                }
                return _conditions[index];
            }
            protected set
            {
                int index = (int)condition;
                if (index >= _conditions.Length)
                {
                    Wiznet.Wiznet($"Trying to get current condition for condition {condition} (index {index}) but current condition length is smaller", WiznetFlags.Bugs, AdminLevels.Implementor);
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

        // Impersonation
        public IPlayer ImpersonatedBy { get; protected set; }

        // Group
        public IGroup Group { get; protected set; }

        public void ChangeGroup(IGroup group)
        {
            if (Group != null && group != null)
                return; // cannot change from one group to another
            Group = group;
        }

        public bool IsSameGroup(IPlayableCharacter character)
        {
            if (this == character)
                return true;
            if (Group == null || character.Group == null)
                return false;
            return Group == character.Group;
        }

        public override bool IsSameGroupOrPet(ICharacter character)
        {
            if (character is IPlayableCharacter pc)
                return IsSameGroup(pc);
            if (character is INonPlayableCharacter npc)
                return npc.Master == this;
            return false;
        }

        // Pets
        public IEnumerable<INonPlayableCharacter> Pets => _pets;

        public void AddPet(INonPlayableCharacter pet)
        {
            if (_pets.Contains(pet))
                return;
            if (pet.Master != null) // cannot change master
                return;
            AddFollower(pet);
            _pets.Add(pet);
            pet.ChangeMaster(this);
        }

        public void RemovePet(INonPlayableCharacter pet)
        {
            if (!_pets.Contains(pet))
                return;
            RemoveFollower(pet);
            _pets.Remove(pet);
            pet.ChangeMaster(null);
        }

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
        public IRoom RecallRoom => RoomManager.DefaultRecallRoom; // TODO: could be different from default one

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
        public override SchoolTypes NoWeaponDamageType => SchoolTypes.Bash;

        public override int NoWeaponBaseDamage
        {
            get
            {
                var hand2HandLearnInfo = GetAbilityLearnedInfo("Hand to hand");
                int learned = hand2HandLearnInfo.percentage;
                return RandomManager.Range(1 + 4 * learned / 100, 2 * Level / 3 * learned / 100);
            }
        }

        public override string NoWeaponDamageNoun => "hit";

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
                    HitPoints = MaxHitPoints;
                    MovePoints = MaxMovePoints;
                    foreach (ResourceKinds resourceKind in EnumHelpers.GetValues<ResourceKinds>())
                        this[resourceKind] = MaxResource(resourceKind);
                    ImpersonatedBy?.Save(); // Force a save when a level is gained
                }
            }
        }

        // Ability
        public bool CheckAbilityImprove(string abilityName, bool abilityUsedSuccessfully, int multiplier)
        {
            var abilityLearned = GetAbilityLearned(abilityName);
            // Know ability ?
            if (abilityLearned == null
                || abilityLearned.Learned == 0
                || abilityLearned.Learned >= 100)
                return false; // ability not known and already at max
            // check to see if the character has a chance to learn
            if (multiplier <= 0)
            {
                Wiznet.Wiznet($"PlayableCharacter.CheckAbilityImprove: multiplier had invalid value {multiplier}", WiznetFlags.Bugs, AdminLevels.Implementor);
                multiplier = 1;
            }
            int difficultyMultiplier = abilityLearned.Rating;
            if (difficultyMultiplier <= 0)
            {
                Wiznet.Wiznet($"PlayableCharacter.CheckAbilityImprove: difficulty multiplier had invalid value {multiplier} for KnownAbility {abilityLearned.Name} Player {DebugName}", WiznetFlags.Bugs, AdminLevels.Implementor);
                difficultyMultiplier = 1;
            }
            // TODO: percentage depends on intelligence replace CurrentAttributes(CharacterAttributes.Intelligence) with values from 3 to 85
            int chance = 10 * TableValues.LearnBonus(this) / (multiplier * difficultyMultiplier * 4) + Level;
            if (RandomManager.Range(1, 1000) > chance)
                return false;
            // now that the character has a CHANCE to learn, see if they really have
            if (abilityUsedSuccessfully)
            {
                chance = (100 - abilityLearned.Learned).Range(5, 95);
                if (RandomManager.Chance(chance))
                {
                    Send("You have become better at {0}!", abilityLearned.Name);
                    abilityLearned.IncrementLearned(1);
                    GainExperience(2 * difficultyMultiplier);
                    return true;
                }
            }
            else
            {
                chance = (abilityLearned.Learned / 2).Range(5, 30);
                if (RandomManager.Chance(chance))
                {
                    Send("You learn from your mistakes, and your {0} skill improves.!", abilityLearned.Name);
                    int increment = RandomManager.Range(1, 3);
                    abilityLearned.IncrementLearned(increment);
                    GainExperience(2 * difficultyMultiplier);
                    return true;
                }
            }

            return false;
        }

        // Immortality
        public void ChangeImmortalState(bool isImmortal)
        {
            if (IsImmortal && !isImmortal)
                Wiznet.Wiznet($"{DebugName} is not immortal anymore.", WiznetFlags.Immortal, AdminLevels.God);
            else if (!IsImmortal && isImmortal)
                Wiznet.Wiznet($"{DebugName} is now immortal.", WiznetFlags.Immortal, AdminLevels.God);
            IsImmortal = isImmortal;
        }

        // Mapping
        public PlayableCharacterData MapPlayableCharacterData()
        {
            PlayableCharacterData data = new PlayableCharacterData
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
                Alignment = Alignment,
                Experience = Experience,
                Trains = Trains,
                Practices = Practices,
                AutoFlags = AutoFlags,
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
                LearnedAbilities = LearnedAbilities.Select(x => x.MapLearnedAbilityData()).ToArray(),
                Aliases = Aliases.ToDictionary(x => x.Key, x => x.Value),
                Cooldowns = AbilitiesInCooldown.ToDictionary(x => x.Key, x => x.Value),
                Pets = Pets.Select(x => x.MapPetData()).ToArray(),
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
            var fastHealing = AbilityManager.CreateInstance<IPassive>("Fast Healing");
            if (fastHealing != null && fastHealing.IsTriggered(this, this, false, out _, out int fastHealingLearnPercentage) == true)
            {
                hitGain += (fastHealingLearnPercentage * hitGain) / 100;
                if (HitPoints < MaxHitPoints)
                    CheckAbilityImprove("Fast Healing", true, 8);
            }
            // meditation
            var meditation = AbilityManager.CreateInstance<IPassive>("Meditation");
            if (meditation != null && fastHealing.IsTriggered(this, this, false, out _, out int meditationLearnPercentage) == true)
            {
                manaGain += (meditationLearnPercentage * manaGain) / 100;
                if (this[ResourceKinds.Mana] < MaxResource(ResourceKinds.Mana))
                    CheckAbilityImprove("Meditation", true, 8);
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
            if (fromRoom != toRoom)
            {
                IReadOnlyCollection<ICharacter> followers = new ReadOnlyCollection<ICharacter>(fromRoom.People.Where(x => x.Leader == this).ToList()); // clone because Move will modify fromRoom.People
                foreach (ICharacter follower in followers)
                {
                    if (follower is INonPlayableCharacter npcFollower)
                    {
                        if (npcFollower.CharacterFlags.HasFlag(CharacterFlags.Charm) && npcFollower.Position < Positions.Standing)
                            ; // TODO: npcFollower.DoStand
                        if (npcFollower.ActFlags.HasFlag(ActFlags.Aggressive) && toRoom.RoomFlags.HasFlag(RoomFlags.Law))
                        {
                            npcFollower.Master?.Act(ActOptions.ToCharacter, "You can't bring {0} into the city.", npcFollower);
                            npcFollower.Send("You aren't allowed in the city.");
                            continue;
                        }
                    }

                    if (follower.Position == Positions.Standing && follower.CanSee(toRoom))
                    {
                        follower.Send("You follow {0}.", DebugName);
                        follower.Move(direction, true);
                    }
                }
            }
        }

        protected override void EnterFollow(IRoom wasRoom, IRoom destination, IItemPortal portal)
        {
            if (wasRoom == destination)
                return;
            if (Pets.Any())
            {
                foreach (INonPlayableCharacter pet in Pets)
                {
                    // TODO: if pet is not standing, call DoStand
                    if (pet.Position != Positions.Standing)
                        return;
                    pet.Send("You follow {0}.", DebugName);
                    pet.Enter(portal, true);
                }
            }
        }

        protected override IEnumerable<ICharacter> GetActTargets(ActOptions option)
        {
            if (option == ActOptions.ToGroup)
                return Group?.Members ?? this.Yield();
            return base.GetActTargets(option);
        }

        protected override void HandleDeath()
        {
            ChangePosition(Positions.Resting);
            HitPoints = 1;
            MovePoints = 1;
            foreach (var resourceKind in EnumHelpers.GetValues<ResourceKinds>())
                this[resourceKind] = 1;
            ResetCooldowns();
            // release pets
            foreach (INonPlayableCharacter pet in _pets)
            {
                if (pet.Room != null)
                    pet.Act(ActOptions.ToRoom, "{0:N} slowly fades away.", pet);
                RemoveFollower(pet);
                pet.ChangeMaster(null);
                World.RemoveCharacter(pet);
            }
            _pets.Clear();
            if (ImpersonatedBy != null) // If impersonated, no real death
            {
                IRoom room = RoomManager.DefaultDeathRoom ?? RoomManager.DefaultRecallRoom;
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

        protected override void RecomputeKnownAbilities()
        {
            base.RecomputeKnownAbilities();
            if (Race is IPlayableRace playableRace)
                MergeAbilities(playableRace.Abilities, true);
        }

        #endregion

        private int ExperienceByLevel => 1000 * ((Race as IPlayableRace)?.ClassExperiencePercentageMultiplier(Class) ?? 100) / 100;

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
                    alignment = -Math.Max(1, (alignDiff - 500) * baseExp / 500 * Level / totalLevel); // become more evil
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
                else if (victim.Alignment < -500)
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
                else if (victim.Alignment > 500)
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
            IAbilityLearned[] newAbilities = LearnedAbilities.Where(x => x.Level == Level && x.Learned == 0).ToArray();
            if (newAbilities.Any())
            {
                StringBuilder sb = new StringBuilder("You can now gain following abilities: %C%");
                foreach (IAbilityLearned abilityLearned in newAbilities)
                    sb.AppendLine(abilityLearned.Name);
                sb.Append("%x%");
                Send(sb);
            }
        }
    }
}
