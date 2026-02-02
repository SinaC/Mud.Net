using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mud.Blueprints.Character;
using Mud.Common;
using Mud.Common.Attributes;
using Mud.DataStructures;
using Mud.Domain;
using Mud.Domain.SerializationData;
using Mud.Domain.SerializationData.Avatar;
using Mud.Flags;
using Mud.Flags.Interfaces;
using Mud.Random;
using Mud.Server.Ability;
using Mud.Server.Ability.AbilityGroup;
using Mud.Server.Ability.Interfaces;
using Mud.Server.Ability.Passive.Interfaces;
using Mud.Server.AbilityGroup.Interfaces;
using Mud.Server.Class.Interfaces;
using Mud.Server.CommandParser.Interfaces;
using Mud.Server.Common;
using Mud.Server.Common.Extensions;
using Mud.Server.Domain;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.Admin;
using Mud.Server.Interfaces.Affect;
using Mud.Server.Interfaces.Affect.Character;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Combat;
using Mud.Server.Interfaces.Effect;
using Mud.Server.Interfaces.Entity;
using Mud.Server.Interfaces.Flags;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Item;
using Mud.Server.Interfaces.Loot;
using Mud.Server.Interfaces.Player;
using Mud.Server.Interfaces.Quest;
using Mud.Server.Interfaces.Room;
using Mud.Server.Interfaces.Table;
using Mud.Server.Options;
using Mud.Server.Race.Interfaces;
using System.Diagnostics;
using System.Text;

namespace Mud.Server.Character.PlayableCharacter;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
[Export(typeof(IPlayableCharacter))]
public class PlayableCharacter : CharacterBase, IPlayableCharacter
{
    private const int NoCondition = -1;
    private const int MinCondition = 0;
    private const int MaxCondition = 48;

    private IOptions<WorldOptions> WorldOptions { get; }
    private IClassManager ClassManager { get; }
    private IRaceManager RaceManager { get; }
    private IQuestManager QuestManager { get; }
    private IAbilityGroupManager AbilityGroupManager { get; }
    private IOmniscienceManager OmniscienceManager { get; }
    private int MaxLevel { get; }

    private readonly ArrayByEnum<long, AvatarStatisticTypes> _statistics;
    private readonly ArrayByEnum<int, Currencies> _currencies;
    private readonly List<IQuest> _activeQuests;
    private readonly List<ICompletedQuest> _completedQuests;
    private readonly ArrayByEnum<int, Conditions> _conditions;
    private readonly Dictionary<string, string> _aliases;
    private readonly List<INonPlayableCharacter> _pets;
    private readonly Dictionary<string, IAbilityGroupLearned> _learnedAbilityGroups;

    public PlayableCharacter(ILogger<PlayableCharacter> logger, IGameActionManager gameActionManager, ICommandParser commandParser, IOptions<MessageForwardOptions> messageForwardOptions, IOptions<WorldOptions> worldOptions, IAbilityManager abilityManager, IRandomManager randomManager, ITableValues tableValues, IRoomManager roomManager, IItemManager itemManager, ICharacterManager characterManager, IAuraManager auraManager, IWeaponEffectManager weaponEffectManager, IFlagsManager flagsManager, IWiznet wiznet, ILootManager lootManager, IAggroManager aggroManager, IRaceManager raceManager, IClassManager classManager, IQuestManager questManager, IResistanceCalculator resistanceCalculator, IRageGenerator rageGenerator, IAffectManager affectManager, IAbilityGroupManager abilityGroupManager, IOmniscienceManager omniscienceManager)
        : base(logger, gameActionManager, commandParser, messageForwardOptions, abilityManager, randomManager, tableValues, roomManager, itemManager, characterManager, auraManager, resistanceCalculator, rageGenerator, weaponEffectManager, affectManager, flagsManager, wiznet, lootManager, aggroManager)
    {
        WorldOptions = worldOptions;
        ClassManager = classManager;
        RaceManager = raceManager;
        QuestManager = questManager;
        AbilityGroupManager = abilityGroupManager;
        OmniscienceManager = omniscienceManager;
        MaxLevel = WorldOptions.Value.MaxLevel;

        _statistics = [];
        _currencies = [];
        _activeQuests = [];
        _completedQuests = [];
        _conditions = new();
        _aliases = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
        _pets = [];
        _learnedAbilityGroups = [];

        ImmortalMode = new ImmortalModes();
        AutoFlags = new AutoFlags();
    }

    public void Initialize(Guid guid, AvatarData data, IPlayer player, IRoom room)
    {
        Initialize(guid, data.Name, string.Empty);

        Incarnatable = false;

        ImpersonatedBy = player;

        Room = RoomManager.NullRoom; // add in null room to avoid problem if an initializer needs a room

        // Extract informations from PlayableCharacterData
        ImmortalMode = new ImmortalModes(data.ImmortalMode);
        FlagsManager.CheckFlags(ImmortalMode);
        CreationTime = data.CreationTime;
        Class = ClassManager[data.Class]!;
        if (Class == null)
        {
            Class = ClassManager.Classes.First();
            Wiznet.Log($"Invalid class '{data.Class}' for character {data.Name}!!", new WiznetFlags("Bugs"), AdminLevels.Implementor);
        }
        Race = RaceManager[data.Race]!;
        if (Race == null || Race is not IPlayableRace)
        {
            Race = RaceManager.PlayableRaces.First();
            Wiznet.Log($"Invalid race '{data.Race}' for character {data.Name}!!", new WiznetFlags("Bugs"), AdminLevels.Implementor);
        }
        BaseBodyForms = Race.BodyForms;
        BaseBodyParts = Race.BodyParts;
        Level = data.Level;
        Experience = data.Experience;
        SilverCoins = data.SilverCoins;
        GoldCoins = data.GoldCoins;
        Alignment = data.Alignment;
        // set max resource first because set current resource will be capped to max resource
        if (data.MaxResources != null)
        {
            foreach (var maxResourceData in data.MaxResources)
            {
                SetBaseMaxResource(maxResourceData.Key, maxResourceData.Value, false);
                SetCurrentMaxResource(maxResourceData.Key, maxResourceData.Value);
            }
        }
        // setting current resources
        if (data.CurrentResources != null)
        {
            foreach (var currentResourceData in data.CurrentResources)
                SetResource(currentResourceData.Key, currentResourceData.Value);
        }
        else
        {
            Wiznet.Log($"PlayableCharacter.ctor: currentResources not found in pfile for {data.Name}", new WiznetFlags("Bugs"), AdminLevels.Implementor);
            // set to 1 if not found
            foreach (ResourceKinds resource in Enum.GetValues<ResourceKinds>())
                SetResource(resource, 1);
        }
        Trains = data.Trains;
        Practices = data.Practices;
        AutoFlags = new AutoFlags(data.AutoFlags);
        FlagsManager.CheckFlags(AutoFlags);
        // Conditions
        if (data.Conditions != null)
        {
            foreach (var conditionData in data.Conditions)
                this[conditionData.Key] = conditionData.Value;
        }
        //
        BaseCharacterFlags = new CharacterFlags(data.CharacterFlags);
        FlagsManager.CheckFlags(BaseCharacterFlags);
        BaseImmunities = new IRVFlags(data.Immunities);
        FlagsManager.CheckFlags(BaseImmunities);
        BaseResistances = new IRVFlags(data.Resistances);
        FlagsManager.CheckFlags(BaseResistances);
        BaseVulnerabilities = new IRVFlags(data.Vulnerabilities);
        FlagsManager.CheckFlags(BaseVulnerabilities);
        BaseShieldFlags = new ShieldFlags(data.ShieldFlags);
        FlagsManager.CheckFlags(BaseShieldFlags);
        BaseSex = data.Sex;
        BaseSize = data.Size;
        if (data.Attributes != null)
        {
            foreach (var attributeData in data.Attributes)
                SetBaseAttributes(attributeData.Key, attributeData.Value, false);
        }
        else
        {
            Wiznet.Log($"PlayableCharacter.ctor: attributes not found in pfile for {data.Name}", new WiznetFlags("Bugs"), AdminLevels.Implementor);
            // set to 1 if not found
            foreach (var attribute in Enum.GetValues<CharacterAttributes>())
                this[attribute] = 1;
        }

        // TODO: set not-found attributes to base value (from class/race)

        // Must be built before equiping
        BuildEquipmentSlots();

        // Statistics
        if (data.Statistics != null)
        {
            foreach (var statistic in data.Statistics)
                this[statistic.Key] = statistic.Value;
        }
        // Currencies
        if (data.Currencies != null)
        {
            foreach (var currency in data.Currencies)
                this[currency.Key] = currency.Value;
        }
        // Equipped items
        if (data.Equipments != null)
        {
            // Create item in inventory and try to equip it
            foreach (var equippedItemData in data.Equipments)
            {
                if (equippedItemData.Item == null)
                {
                    Wiznet.Log($"Item to equip in slot {equippedItemData.Slot} for character {data.Name} is null.", new WiznetFlags("Bugs"), AdminLevels.Implementor);
                }
                else
                {
                    // Create in inventory
                    var item = ItemManager.AddItem(Guid.NewGuid(), equippedItemData.Item, this);
                    if (item != null)
                    {
                        // Try to equip it
                        var (equippedItem, searchEquipmentSlotResult) = SearchEquipmentSlot(equippedItemData.Slot, false);
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
                                Wiznet.Log($"Item blueprint Id {equippedItemData.Item.ItemId} cannot be equipped anymore in slot {equippedItemData.Slot} for character {data.Name}.", new WiznetFlags("Bugs"), AdminLevels.Implementor);
                            }
                        }
                        else
                        {
                            Wiznet.Log($"Item blueprint Id {equippedItemData.Item.ItemId} was supposed to be equipped in first empty slot {equippedItemData.Slot} for character {data.Name} but this slot doesn't exist anymore (result: {searchEquipmentSlotResult}).", new WiznetFlags("Bugs"), AdminLevels.Implementor);
                        }
                    }
                    else
                    {
                        Wiznet.Log($"Item blueprint Id {equippedItemData.Item.ItemId} cannot be created in slot {equippedItemData.Slot} for character {data.Name}.", new WiznetFlags("Bugs"), AdminLevels.Implementor);
                    }
                }
            }
        }
        // Inventory
        if (data.Inventory != null)
        {
            foreach (var itemData in data.Inventory.Reverse()) // PutInContainer used Insert(0,x) to put new item but when loading pfile we want to preserve order so we reverse
                ItemManager.AddItem(Guid.NewGuid(), itemData, this);
        }
        // Active quests
        if (data.ActiveQuests != null)
        {
            foreach (var questData in data.ActiveQuests)
            {
                QuestManager.AddQuest(questData, this);
            }
        }
        // Completed quests
        if (data.CompletedQuests != null)
        {
            foreach (var completeQuest in data.CompletedQuests)
            {
                QuestManager.AddCompletedQuest(completeQuest, this);
            }
        }
        // Auras
        if (data.Auras != null)
        {
            foreach (var auraData in data.Auras)
                AuraManager.AddAura(this, auraData, false);
        }
        // Learn abilities
        if (data.LearnedAbilities != null)
        {
            foreach (var learnedAbilityData in data.LearnedAbilities)
            {
                var abilityDefinition = AbilityManager[learnedAbilityData.Name];
                if (abilityDefinition == null)
                    Wiznet.Log($"LearnedAbility: Ability {learnedAbilityData.Name} doesn't exist anymore", new WiznetFlags("Bugs"), AdminLevels.Implementor);
                else
                {
                    // search ability usage in race then in class
                    var abilityUsage = ((IPlayableRace)Race).Abilities.SingleOrDefault(x => StringCompareHelpers.StringEquals(x.Name, abilityDefinition.Name)) ?? Class.AvailableAbilities.SingleOrDefault(x => StringCompareHelpers.StringEquals(x.Name, abilityDefinition.Name));
                    if (abilityUsage == null)
                        Wiznet.Log($"LearnedAbility: Ability {learnedAbilityData.Name} is not anymore available for {Race.Name} or {Class.Name}", new WiznetFlags("Bugs"), AdminLevels.Implementor);
                    else
                    {
                        var abilityLearned = new AbilityLearned(learnedAbilityData, abilityUsage);
                        AddLearnedAbility(abilityLearned);
                    }
                }
            }
        }
        // Learn ability groups
        if (data.LearnedAbilityGroups != null)
        {
            foreach (var learnedAbilityGroupData in data.LearnedAbilityGroups)
            {
                var abilityGroupDefinition = AbilityGroupManager[learnedAbilityGroupData.Name];
                if (abilityGroupDefinition == null)
                    Wiznet.Log($"LearnedAbilityGroup: Ability group {learnedAbilityGroupData.Name} doesn't exist anymore", new WiznetFlags("Bugs"), AdminLevels.Implementor);
                else
                {
                    var abilityGroupLearned = new AbilityGroupLearned(learnedAbilityGroupData, abilityGroupDefinition);
                    AddLearnedAbilityGroup(abilityGroupLearned);
                }
            }
        }
        // add class basics group
        foreach (var basicAbilityGroupUsage in Class.BasicAbilityGroups)
        {
            if (!_learnedAbilityGroups.ContainsKey(basicAbilityGroupUsage.Name))
            {
                var abilityGroupLearned = new AbilityGroupLearned(basicAbilityGroupUsage);
                AddLearnedAbilityGroup(abilityGroupLearned);
            }
        }
        // Aliases
        if (data.Aliases != null)
        {
            foreach (var alias in data.Aliases)
                _aliases.Add(alias.Key, alias.Value);
        }
        // Cooldowns
        if (data.Cooldowns != null)
        {
            foreach (var cooldown in data.Cooldowns)
            {
                var abilityDefinition = AbilityManager[cooldown.Key];
                if (abilityDefinition == null)
                    Wiznet.Log($"Cooldown: ability {cooldown.Key} doesn't exist anymore", new WiznetFlags("Bugs"), AdminLevels.Implementor);
                else
                    SetCooldown(cooldown.Key, Pulse.ToTimeSpan(cooldown.Value));
            }
        }
        // Pets
        if (data.Pets != null)
        {
            foreach (var petData in data.Pets)
            {
                var blueprint = CharacterManager.GetCharacterBlueprint<CharacterNormalBlueprint>(petData.BlueprintId);
                if (blueprint == null)
                {
                    Wiznet.Log($"Pet blueprint id {petData.BlueprintId} doesn't exist anymore", new WiznetFlags("Bugs"), AdminLevels.Implementor);
                }
                else
                {
                    var pet = CharacterManager.AddNonPlayableCharacter(Guid.NewGuid(), blueprint, petData, room);
                    if (pet == null)
                    {
                        Wiznet.Log($"Pet blueprint id {petData.BlueprintId} cannot be created for {DebugName}", new WiznetFlags("Bugs"), AdminLevels.Implementor);
                    }
                    else
                        AddPet(pet);
                }
            }
        }

        //
        Room = room;
        room.Enter(this);

        RecomputeKnownAbilities();
        ResetAttributesAndResourcesAndFlags();
        RecomputeCurrentResourceKinds();
        AddAurasFromBaseFlags();
    }

    #region IPlayableCharacter

    #region ICharacter

    #region IEntity

    #region IActor

    public override void Send(string message, bool addTrailingNewLine)
    {
        base.Send(message, addTrailingNewLine);
        if (ImpersonatedBy != null)
        {
            if (PrefixForwardedMessages)
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

    public override string DebugName => $"{DisplayName}[{ImpersonatedBy?.DisplayName ?? "???"}][Id:{Id}]";

    public override string RelativeDisplayName(ICharacter beholder, bool capitalizeFirstLetter = false)
    {
        StringBuilder displayName = new();
        if (beholder.CanSee(this))
            displayName.Append(DisplayName);
        else if (capitalizeFirstLetter)
            displayName.Append("Someone");
        else
            displayName.Append("someone");
        if (beholder.ImmortalMode.IsSet("Holylight"))
            displayName.Append($" [PLR {ImpersonatedBy?.DisplayName ?? " ??? "}]");
        return displayName.ToString();
    }

    public override bool CanSee(ICharacter? victim)
    {
        if (victim == null)
            return false;
        if (ImmortalMode.IsSet("Holylight"))
            return true;
        return base.CanSee(victim);
    }

    public override bool CanSee(IExit? exit)
    {
        if (exit == null)
            return false;
        if (ImmortalMode.IsSet("Holylight"))
            return true;
        return base.CanSee(exit);
    }

    public override bool CanSee(IRoom? room)
    {
        if (room == null)
            return false;

        var adminLevel = (ImpersonatedBy as IAdmin)?.Level;
        if (room.RoomFlags.IsSet("ImpOnly") && (!ImmortalMode.IsSet("Holylight") || adminLevel is null || adminLevel < AdminLevels.Implementor))
            return false;

        if (room.RoomFlags.IsSet("GodsOnly") && (!ImmortalMode.IsSet("Holylight") || adminLevel is null || adminLevel < AdminLevels.God))
            return false;

        if (room.RoomFlags.IsSet("HeroesOnly") && !ImmortalMode.IsSet("Holylight"))
            return false;

        if (room.RoomFlags.IsSet("NewbiesOnly") && Level > 5 && !ImmortalMode.IsSet("Holylight"))
            return false;


        return base.CanSee(room);
    }

    public override void OnRemoved(IRoom nullRoom) // called before removing a character from the game
    {
        base.OnRemoved(nullRoom);

        // Leave group
        if (Group != null)
        {
            if (Group.Members.Count() <= 2) // group will contain only one member, disband
                Group.Disband();
            else
                Group.RemoveMember(this);
            Group = null;
        }

        // Release pets
        foreach (INonPlayableCharacter pet in _pets)
        {
            if (pet.Room != null)
                pet.Act(ActOptions.ToRoom, "{0:N} slowly fades away.", pet);
            RemoveFollower(pet);
            pet.ChangeMaster(null);
            CharacterManager.RemoveCharacter(pet);
        }
        _pets.Clear();

        // TODO: what if character is incarnated
        ImpersonatedBy?.StopImpersonating();
        ImpersonatedBy = null; // TODO: warn ImpersonatedBy ?
    }

    #endregion

    public override IImmortalModes ImmortalMode { get; protected set; }

    public override int MaxCarryWeight => ImmortalMode.IsSet("Infinite")
        ? 10000000
        : base.MaxCarryWeight;

    public override int MaxCarryNumber => ImmortalMode.IsSet("Infinite")
        ? 1000
        : base.MaxCarryNumber;

    public override IEnumerable<IPlayableCharacter> GetPlayableCharactersImpactedByKill()
    {
        var characters = new List<IPlayableCharacter>();
        if (Group != null)
        {
            foreach (var character in Group.Members)
                characters.Add(character);
        }
        else
            characters.Add(this);
        return characters;
    }

    // Combat
    public override void MultiHit(ICharacter? victim, IMultiHitModifier? multiHitModifier) // 'this' starts a combat with 'victim'
    {
        if (victim == null)
            return;

        // no attacks for stunnies
        if (IsStunned)
        {
            DecreaseStun();
            if (!IsStunned)
                Act(ActOptions.ToAll, "%W%{0:N} regain{0:v} {0:s} equilibrium.%x%", this);
            return;
        }

        //StandUpInCombatIfPossible();

        var mainHand = GetEquipment<IItemWeapon>(EquipmentSlots.MainHand);
        // main hand attack
        int attackCount = 0;
        OneHit(victim, mainHand, multiHitModifier);
        attackCount++;
        if (Fighting != victim)
            return;

        // additional hits from affects
        var characterAdditionalHitAffects = victim.Auras.Where(x => x.IsValid).SelectMany(x => x.Affects.OfType<ICharacterAdditionalHitAffect>()).ToArray();
        foreach (var characterAdditionalHitAffect in characterAdditionalHitAffects)
        {
            for (int additionalHitFromAffect = 0; additionalHitFromAffect < characterAdditionalHitAffect.AdditionalHitCount; additionalHitFromAffect++)
            {
                if (characterAdditionalHitAffect.IsAdditionalHitAvailable(this, additionalHitFromAffect))
                    OneHit(victim, mainHand, multiHitModifier);
                attackCount++;
                if (Fighting != victim) // stop if not anymore fighting
                    return;
                if (multiHitModifier?.MaxAttackCount <= attackCount)
                    return;
            }
        }

        // additional hits (second, third attack, ...)
        var additionalHitAbilities = new List<IAdditionalHitPassive>();
        foreach (var additionalHitAbilityDefinition in AbilityManager.SearchAbilitiesByExecutionType<IAdditionalHitPassive>())
        {
            var additionalHitAbility = AbilityManager.CreateInstance<IAdditionalHitPassive>(additionalHitAbilityDefinition);
            if (additionalHitAbility != null)
                additionalHitAbilities.Add(additionalHitAbility);
        }
        foreach (var additionalHitAbility in additionalHitAbilities.OrderBy(x => x.AdditionalHitIndex).ThenBy(x => !x.StopMultiHitIfFailed)) // for each hit index, use additional hits which dont stop multi hit first
        {
            if (additionalHitAbility.IsTriggered(this, victim, true, out _, out _))
            {
                OneHit(victim, mainHand, multiHitModifier);
                attackCount++;
                if (Fighting != victim) // stop if not anymore fighting
                    return;
                if (multiHitModifier?.MaxAttackCount <= attackCount)
                    return;
            }
            else if (additionalHitAbility.StopMultiHitIfFailed)
                return; // stop once an additional fails
        }

        // additional wield  (dual wield, third wield, ...)
        var additionalWieldAbilities = new List<IAdditionalWieldPassive>();
        foreach (var additionalWieldAbilityDefinition in AbilityManager.SearchAbilitiesByExecutionType<IAdditionalWieldPassive>())
        {
            var additionalWieldAbility = AbilityManager.CreateInstance<IAdditionalWieldPassive>(additionalWieldAbilityDefinition);
            if (additionalWieldAbility != null)
                additionalWieldAbilities.Add(additionalWieldAbility);
        }
        foreach (var additionalWieldAbility in additionalWieldAbilities.OrderBy(x => x.AdditionalHitIndex).ThenBy(x => !x.StopMultiHitIfFailed)) // for each hit index, use additional hits which dont stop multi hit first
        {
            if (additionalWieldAbility.IsTriggered(this, victim, true, out _, out _, out var weapon))
            {
                OneHit(victim, weapon, multiHitModifier);
                attackCount++;
                if (Fighting != victim) // stop if not anymore fighting
                    return;
                if (multiHitModifier?.MaxAttackCount <= attackCount)
                    return;
            }
            else if (additionalWieldAbility.StopMultiHitIfFailed)
                return; // stop once an additional fails
        }
    }

    public override void HandleAutoGold(IItemCorpse corpse)
    {
        if (!corpse.IsPlayableCharacterCorpse && AutoFlags.IsSet("Gold") && corpse.Content.Any())
        {
            var corpseContent = corpse.Content.OfType<IItemMoney>().Where(CanLoot).ToArray();
            foreach (var money in corpseContent)
                GetItem(money, corpse);
        }
    }

    public override void HandleAutoLoot(IItemCorpse corpse)
    {
        if (!corpse.IsPlayableCharacterCorpse && AutoFlags.IsSet("Loot") && corpse.Content.Any())
        {
            var corpseContent = corpse.Content.Where(CanLoot).ToArray();
            foreach (var item in corpseContent)
                GetItem(item, corpse);
        }
    }

    public override void HandleAutoSacrifice(IItemCorpse corpse)
    {
        if (!corpse.IsPlayableCharacterCorpse && AutoFlags.IsSet("Sacrifice") && !corpse.Content.Any()) // TODO: corpse empty only if autoloot is set?
            SacrificeItem(corpse);
    }

    public void KillingPayoff(ICharacter victim, int groupLevelSum) // Gain xp/gold/reputation/quests/...
    {
        var killStatistics = victim is IPlayableCharacter
            ? AvatarStatisticTypes.PcKill
            : AvatarStatisticTypes.NpcKill;
        IncrementStatistics(killStatistics);

        // Gain xp and alignment
        if (this != victim && victim is INonPlayableCharacter) // gain xp only for NPC victim
        {
            // Gain xp and change alignment
            var (experience, alignment) = ComputeExperienceAndAlignment(victim, groupLevelSum);
            if (experience > 0)
            {
                Send("%y%You gain {0} experience points.%x%", experience);
                GainExperience(experience);
            }
            UpdateAlignment(alignment);
        }
        // TODO: reputation
        // quests
        if (victim is INonPlayableCharacter npcVictim)
        {
            // Check killer quest table (only if killer is PC and victim is NPC)
            foreach (var quest in ActiveQuests)
            {
                // Update kill objectives
                quest.Update(npcVictim);
            }
        }
    }

    // Abilities
    public override IEnumerable<IAbilityLearned> LearnedAbilities
        => ImmortalMode.IsSet("Omniscient")
            ? OmniscienceManager.LearnedAbilities 
            : base.LearnedAbilities;

    public override (int percentage, IAbilityLearned? abilityLearned) GetWeaponLearnedAndPercentage(IItemWeapon? weapon)
    {
        string abilityName = null!;
        if (weapon == null)
            abilityName = "Hand to Hand";
        else
        {
            // search weapon ability from weapon type
            var weaponAbility = AbilityManager[weapon.Type];
            if (weaponAbility != null)
                abilityName = weaponAbility.Name;
        }

        // no ability (exotic) -> 3*level
        if (abilityName == null)
        {
            var learned = Math.Clamp(3 * Level, 0, 100);
            return (learned, null);
        }
        // ability, check %
        var learnInfo = GetAbilityLearnedAndPercentage(abilityName);
        return learnInfo;
    }

    public override (int percentage, IAbilityLearned? abilityLearned) GetAbilityLearnedAndPercentage(string abilityName)
    {
        var abilityLearned = GetAbilityLearned(abilityName);

        int learned = 0;
        if (ImmortalMode.IsSet("Omniscient"))
            learned = abilityLearned?.Learned ?? 100;
        else if (abilityLearned != null && abilityLearned.Level <= Level)
        {
            learned = abilityLearned.Learned;

            if (Daze > 0)
            {
                if (abilityLearned?.AbilityUsage?.AbilityDefinition.Type == AbilityTypes.Spell)
                    learned /= 2;
                else
                    learned = (learned * 2) / 3;
            }

            if (this[Conditions.Drunk] > 10)
                learned = (learned * 9) / 10;
        }

        return (Math.Clamp(learned, 0, 100), abilityLearned);
    }

    #endregion

    public IReadOnlyDictionary<string, string> Aliases => _aliases;

    public void SetAlias(string alias, string command)
    {
        _aliases[alias] = command;
    }

    public void RemoveAlias(string alias)
    {
        _aliases.Remove(alias);
    }

    public DateTime CreationTime { get; protected set; }

    // Statistics
    public long this[AvatarStatisticTypes avatarStatisticType]
    {
        get => _statistics[avatarStatisticType];
        protected set => _statistics[avatarStatisticType] = value;
    }

    public void IncrementStatistics(AvatarStatisticTypes avatarStatisticType, long increment = 1)
    {
        if (increment <= 0)
        {
            Logger.LogError("IncrementStatistics: invalid increment {increment} for statistics {avatarStatisticType}", increment, avatarStatisticType);
            return;
        }
        _statistics[avatarStatisticType] += increment;
    }


    // Currencies
    public int this[Currencies currency]
    {
        get => _currencies[currency];
        protected set => _currencies[currency] = value;
    }

    public void UpdateCurrency(Currencies currency, int delta)
    {
        if (currency == Currencies.QuestPoints)
        {
            if (delta > 0)
                IncrementStatistics(AvatarStatisticTypes.QuestPointsEarned, delta);
            else if (delta < 0)
                IncrementStatistics(AvatarStatisticTypes.QuestPointsSpent, -delta);
        }

        _currencies[currency] = Math.Max(0, _currencies[currency] + delta);
    }

    public long ExperienceToLevel =>
        Level >= MaxLevel
            ? 0
            : (ExperienceByLevel * Level) - Experience;

    public long Experience { get; protected set; }

    public int Trains { get; protected set; }

    public int Practices { get; protected set; }

    public void UpdateTrainsAndPractices(int trainsAmount, int practicesAmount)
    {
        Trains = Math.Max(0, Trains + trainsAmount);
        Practices = Math.Max(0, Practices + practicesAmount);
    }

    public int Wimpy { get; protected set; }

    public void SetWimpy(int wimpy)
    {
        Wimpy = Math.Clamp(wimpy, 0, MaxResource(ResourceKinds.HitPoints)/2); // not higher than half max hit points
    }

    public IAutoFlags AutoFlags { get; protected set; }

    public void AddAutoFlags(IAutoFlags autoFlags)
    {
        AutoFlags.Set(autoFlags);
        FlagsManager.CheckFlags(AutoFlags);
    }

    public void RemoveAutoFlags(IAutoFlags autoFlags)
    {
        AutoFlags.Unset(autoFlags);
        FlagsManager.CheckFlags(AutoFlags);
    }

    public int this[Conditions condition]
    {
        get => _conditions[condition];
        protected set => _conditions[condition] = value;
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
        this[condition] = Math.Clamp(previousValue + value, MinCondition, MaxCondition);
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

    public override (long silverSpent, long goldSpent) DeductCost(long cost)
    {
        var (silverSpent, goldSpent) = base.DeductCost(cost);
        IncrementStatistics(AvatarStatisticTypes.MoneySpent, cost);
        return (silverSpent, goldSpent);
    }

    public override void UpdateMoney(long silverCoins, long goldCoins)
    {
        base.UpdateMoney(silverCoins, goldCoins);
        if (silverCoins > 0)
            IncrementStatistics(AvatarStatisticTypes.SilverEarned, silverCoins);
        if (goldCoins > 0)
            IncrementStatistics(AvatarStatisticTypes.GoldEarned, goldCoins);
    }

    public override (long stolenSilver, long stolenGold) StealMoney(long silverCoins, long goldCoins)
    {
        var (stolenSilver, stolenGold) = base.StealMoney(silverCoins, goldCoins);
        IncrementStatistics(AvatarStatisticTypes.SilverStolen, stolenSilver);
        IncrementStatistics(AvatarStatisticTypes.GoldStolen, stolenGold);
        return (stolenSilver, stolenGold);
    }

    public override bool CanSee(IItem? target)
    {
        if (target is IItemQuest questItem)
        {
            // See only if on this quest
            if (ImmortalMode.IsSet("Holylight") || questItem.IsQuestObjective(this, false)) // we don't care if the objective is completed or not
                return true;
            return false;
        }
        return base.CanSee(target);
    }

    // Loot
    public override bool CanLoot(IItem? target)
    {
        if (target is IItemQuest questItem)
        {
            // See only if on this quest
            if (CanSee(target) && questItem.IsQuestObjective(this, true)) // can loot only if objective is not complete
                return true;
            return false;
        }
        return base.CanLoot(target);
    }

    // Impersonation
    public IPlayer? ImpersonatedBy { get; protected set; }

    // Group
    public IGroup? Group { get; protected set; }

    public void ChangeGroup(IGroup? group)
    {
        if (Group != null && group == null)
            Group = null;
        if (Group == null && group != null)
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

    public int PulseLeftBeforeNextAutomaticQuest { get; protected set; }

    public void SetTimeLeftBeforeNextAutomaticQuest(TimeSpan timeSpan)
    {
        PulseLeftBeforeNextAutomaticQuest = Pulse.FromTimeSpan(timeSpan);
    }

    public int DecreasePulseLeftBeforeNextAutomaticQuest(int pulseCount)
    {
        PulseLeftBeforeNextAutomaticQuest = Math.Max(0, PulseLeftBeforeNextAutomaticQuest - pulseCount);
        return PulseLeftBeforeNextAutomaticQuest;
    }

    public IEnumerable<IQuest> ActiveQuests => _activeQuests;

    public void AddQuest(IQuest quest)
    {
        // TODO: max quest ?
        _activeQuests.Add(quest);
    }

    public void RemoveQuest(IQuest quest)
    {
        _activeQuests.Remove(quest);
    }

    public IEnumerable<ICompletedQuest> CompletedQuests => _completedQuests;

    public void AddCompletedQuest(ICompletedQuest quest)
    {
        _completedQuests.Add(quest);
    }

    // Room
    public IRoom RecallRoom
        => RoomManager.DefaultRecallRoom; // TODO: could be different from default one

    // Impersonation
    public bool StopImpersonation()
    {
        if (!IsValid)
        {
            Logger.LogWarning("ICharacter.StopImpersonation: {name} is not valid anymore", DebugName);
            ImpersonatedBy = null;
            return false;
        }

        Logger.LogDebug("ICharacter.StopImpersonation: {name} old: {impersonatedName};", DebugName, ImpersonatedBy?.DisplayName ?? "<<none>>");
        ImpersonatedBy = null;
        RemoveGeneratedQuests(); // TODO: don't do this when client disconnects
        RecomputeKnownAbilities();
        Recompute();
        return true;
    }

    // Combat
    public void GainExperience(long experience)
    {
        if (Level < MaxLevel)
        {
            bool levelGained = false;
            Experience = Math.Max(ExperienceByLevel * (Level-1), Experience + experience); // don't go below current level
            // Raise level
            if (experience > 0)
            {
                // In case multiple level are gain, check max level
                while (ExperienceToLevel <= 0 && Level < MaxLevel)
                {
                    levelGained = true;
                    Level++;
                    Wiznet.Log($"{DebugName} has attained level {Level}", new WiznetFlags("Levels"));
                    Send("%G%You raise a level!!%x%");
                    Act(ActOptions.ToGroup, "{0} has attained level {1}", this, Level);
                    AdvanceLevel();
                }
            }
            if (levelGained)
            {
                RecomputeKnownAbilities();
                Recompute();
                // Bonus -> reset cooldown and set resource to max
                ResetCooldowns();
                foreach (ResourceKinds resourceKind in Enum.GetValues<ResourceKinds>())
                    SetResource(resourceKind, MaxResource(resourceKind));
                ImpersonatedBy?.SetSaveNeeded();
            }
        }
    }

    // Ability
    public IEnumerable<IAbilityGroupLearned> LearnedAbilityGroups => _learnedAbilityGroups.Values;

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
            Wiznet.Log($"PlayableCharacter.CheckAbilityImprove: multiplier had invalid value {multiplier}", new WiznetFlags("Bugs"), AdminLevels.Implementor);
            multiplier = 1;
        }
        var difficultyMultiplier = abilityLearned.Rating;
        if (difficultyMultiplier <= 0)
        {
            Wiznet.Log($"PlayableCharacter.CheckAbilityImprove: difficulty multiplier had invalid value {multiplier} for KnownAbility {abilityLearned.Name} Player {DebugName}", new WiznetFlags("Bugs"), AdminLevels.Implementor);
            difficultyMultiplier = 1;
        }
        // percentage depends on intelligence replace CurrentAttributes(CharacterAttributes.Intelligence) with values from 3 to 85
        var chance = 10 * TableValues.LearnBonus(this) / (multiplier * difficultyMultiplier * 4) + Level;
        if (RandomManager.Range(1, 1000) > chance)
            return false;
        // now that the character has a CHANCE to learn, see if they really have
        if (abilityUsedSuccessfully)
        {
            chance = Math.Clamp(100 - abilityLearned.Learned, 5, 95);
            if (RandomManager.Chance(chance))
            {
                Send("%W%You have become better at {0}!%x%", abilityLearned.Name);
                abilityLearned.IncrementLearned(1);
                GainExperience(2 * difficultyMultiplier);
                return true;
            }
        }
        else
        {
            chance = Math.Clamp(abilityLearned.Learned / 2, 5, 30);
            if (RandomManager.Chance(chance))
            {
                Send("%W%You learn from your mistakes, and your {0} skill improves!%x%", abilityLearned.Name);
                var increment = RandomManager.Range(1, 3);
                abilityLearned.IncrementLearned(increment);
                GainExperience(2 * difficultyMultiplier);
                return true;
            }
        }

        return false;
    }

    public void GainAbility(IAbilityUsage abilityUsage)
        => AddLearnedAbility(abilityUsage, false);

    public void GainLearnedAbilityGroup(IAbilityGroupUsage abilityGroupUsage)
    {
        var learnedAbilityGroup = new AbilityGroupLearned(abilityGroupUsage);
        if (!_learnedAbilityGroups.ContainsKey(learnedAbilityGroup.Name))
            _learnedAbilityGroups.Add(learnedAbilityGroup.Name, learnedAbilityGroup);
        RecomputeKnownAbilities();
    }

    // Immortality
    public void ChangeImmortalMode(IImmortalModes mode)
    {
        Wiznet.Log($"{DebugName} is immortal mode changed from {ImmortalMode} to {mode}.", new WiznetFlags("Immortal"), AdminLevels.God);

        ImmortalMode = mode;
        FlagsManager.CheckFlags(ImmortalMode);

        RecomputeCurrentResourceKinds();
    }

    // Misc
    public bool SacrificeItem(IItem item)
    {
        if (item.NoTake || item.ItemFlags.IsSet("NoSacrifice"))
        {
            Act(ActOptions.ToCharacter, "{0} is not an acceptable sacrifice.", item);
            return false;
        }
        if (item is IItemCorpse itemCorpse && itemCorpse.IsPlayableCharacterCorpse && itemCorpse.Content.Any())
        {
            Send("Mota wouldn't like that.");
            return false;
        }
        if (item is IItemFurniture itemFurniture)
        {
            var user = itemFurniture.People?.FirstOrDefault();
            if (user != null)
            {
                Act(ActOptions.ToCharacter, "{0:N} appears to be using {1}.", user, itemFurniture);
                return false;
            }
        }

        Act(ActOptions.ToAll, "{0:N} sacrifices {1:v} to Mota.", this, item);
        Wiznet.Log($"{DebugName} sacrifices {item.DebugName} as a burnt offering.", new WiznetFlags("Saccing"));
        ItemManager.RemoveItem(item);
        //
        long silver = Math.Max(1, item.Level * 3);
        if (item is not IItemCorpse)
            silver = Math.Min(silver, item.Cost);
        if (silver <= 0)
        {
            Send("Mota doesn't give you anything for your sacrifice."); // TODO: god
            Wiznet.Log($"DoSacrifice: {item.DebugName} gives zero or negative money {silver}!", new WiznetFlags("Bugs"), AdminLevels.Implementor);
        }
        else if (silver == 1)
            Send("Mota gives you one silver coin for your sacrifice.");
        else
            Send("Mota gives you {0} silver coins for your sacrifice.", silver);
        if (silver > 0)
            UpdateMoney(silver, 0);
        // autosplit
        if (silver > 0 && AutoFlags.IsSet("Split"))
            SplitMoney(silver, 0);

        return true;
    }

    public bool SplitMoney(long amountSilver, long amountGold)
    {
        var members = (Group?.Members ?? this.Yield()).ToArray();
        if (members.Length < 2)
            return false;
        long extraSilver = Math.DivRem(amountSilver, members.Length, out long shareSilver);
        long extraGold = Math.DivRem(amountGold, members.Length, out long shareGold);
        if (shareSilver == 0 && shareGold == 0)
        {
            Send("Don't even bother, cheapstake.");
            return false;
        }
        // Remove money from ours, extra money excluded
        if (shareSilver > 0)
            Send("You split {0} silver coins. Your share is {1} silver.", amountSilver, shareSilver + extraSilver);
        if (shareGold > 0)
            Send("You split {0} gold coins. Your share is {1} gold.", amountGold, shareGold + extraGold);
        UpdateMoney(-amountSilver + extraSilver, -amountGold + extraGold);
        UpdateMoney(extraSilver, extraGold);
        // Give share money to group member (including ourself)
        foreach (var member in members)
        {
            if (member != this)
            {
                if (shareGold == 0)
                    Act(ActOptions.ToCharacter, "{0:N} splits {1} silver coins. Your share is {2} silver.", this, amountSilver, shareSilver);
                else if (shareSilver == 0)
                    Act(ActOptions.ToCharacter, "{0:N} splits {1} gold coins. Your share is {2} gold.", this, amountGold, shareGold);
                else
                    Act(ActOptions.ToCharacter, "{0:N} splits {1} silver and {2} gold coins, giving you {3} silver and {4} gold.", this, amountSilver, amountGold, shareSilver, shareGold);
            }
            UpdateMoney(shareSilver, shareGold);
        }

        return true;
    }

    // Mapping
    public AvatarData MapAvatarData()
    {
        AvatarData data = new()
        {
            Version = Versioning.AvatarCurrentVersion,
            AccountName = ImpersonatedBy?.Name ?? "???",
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
            CurrentResources = Enum.GetValues<ResourceKinds>().ToDictionary(x => x, x => this[x]),
            MaxResources = Enum.GetValues<ResourceKinds>().ToDictionary(x => x, MaxResource),
            Currencies = _currencies.ToDictionary(),
            Alignment = Alignment,
            Wimpy = Wimpy,
            Experience = Experience,
            Trains = Trains,
            Practices = Practices,
            AutoFlags = AutoFlags.Serialize(),
            Conditions = _conditions.ToDictionary(),
            Equipments = Equipments.Where(x => x.Item != null).Select(x => x.MapEquippedData()).ToArray(),
            Inventory = Inventory.Select(x => x.MapItemData()).ToArray(),
            ActiveQuests = ActiveQuests.OfType<IPredefinedQuest>().Select(x => x.MapQuestData()).ToArray(), // don't save generated quest
            Auras = MapAuraData(),
            CharacterFlags = BaseCharacterFlags.Serialize(),
            Immunities = BaseImmunities.Serialize(),
            Resistances = BaseResistances.Serialize(),
            Vulnerabilities = BaseVulnerabilities.Serialize(),
            ShieldFlags = BaseShieldFlags.Serialize(),
            Attributes = Enum.GetValues<CharacterAttributes>().ToDictionary(x => x, BaseAttribute),
            LearnedAbilities = base.LearnedAbilities.Select(x => x.MapLearnedAbilityData()).ToArray(), // use base.LearnedAbilities to avoid retrieve every abilities if Ominiscient immortal flags is active
            LearnedAbilityGroups = LearnedAbilityGroups.Select(x => x.MapLearnedAbilityGroupData()).ToArray(),
            Aliases = Aliases.ToDictionary(x => x.Key, x => x.Value),
            Cooldowns = AbilitiesInCooldown.ToDictionary(x => x.Key, x => x.Value),
            Pets = Pets.Where(x => x.ActFlags.IsSet("pet")).Select(x => x.MapPetData()).ToArray(), // save only pets with act flag PET ro differentiate bought pet and charmed pet
            Statistics = _statistics.ToDictionary(),
            ImmortalMode = ImmortalMode.Serialize(),
            CompletedQuests = _completedQuests.Select(x => x.MapCompletedQuestData()).ToArray(),
            PulseLeftBeforeNextAutomaticQuest = PulseLeftBeforeNextAutomaticQuest,
        };
        return data;
    }

    #endregion

    #region CharacterBase

    public override bool GetItem(IItem item, IContainer container)
    {
        long silver = 0, gold = 0;
        if (item is IItemMoney money)
        {
            silver = money.SilverCoins;
            gold = money.GoldCoins;
        }

        bool got = base.GetItem(item, container);
        // autosplit
        if (got && AutoFlags.IsSet("Split")
                && (silver > 0 || gold > 0))
            SplitMoney(silver, gold);
        return true;
    }

    protected override bool AutomaticallyDisplayRoom => true;

    protected override Positions DefaultPosition => Positions.Standing;

    protected override bool CannotDie => ImmortalMode.IsSet("NoDeath");

    protected override bool CheckEquippedItemsDuringRecompute()
    {
        var recomputeNeeded = false;
        foreach (var equipedItem in Equipments.Where(x => x.Slot == EquipmentSlots.MainHand && x.Item is IItemWeapon))
        {
            if (equipedItem.Item is IItemWeapon weapon) // always true
            {
                if (!weapon.CanWield(this))
                {
                    Act(ActOptions.ToAll, "{0:N} can't use {1} anymore.", this, weapon);
                    weapon.ChangeEquippedBy(null, false);
                    weapon.ChangeContainer(this);
                    recomputeNeeded = true;
                }
            }
        }
        return recomputeNeeded;
    }

    protected override int MaxAllowedBasicAttribute(BasicAttributes basicAttribute)
    {
        if (Race is IPlayableRace playableRace)
        {
            var max = playableRace.GetMaxAttribute(basicAttribute) + 4;
            if (Class != null && basicAttribute == Class.PrimeAttribute)
            {
                max += 2;
                if (playableRace?.EnhancedPrimeAttribute == true)
                    max++;
            }
            return Math.Min(max, 25);
        }
        return 25;
    }

    protected override (decimal hit, decimal move, decimal mana, decimal psy) CalculateResourcesDeltaByMinute()
    {
        decimal hitGain = Math.Max(3, this[CharacterAttributes.Constitution] - 3 + Level / 2);
        decimal moveGain = Math.Max(15, Level);
        decimal manaGain = (this[CharacterAttributes.Wisdom] + this[CharacterAttributes.Intelligence] + Level) / 2;
        decimal psyGain = (this[CharacterAttributes.Wisdom] + this[CharacterAttributes.Intelligence] + Level) / 2; // TODO: correct formula
        // class bonus
        hitGain += (Class?.MaxHitPointGainPerLevel ?? 0) - 10;
        // abilities
        foreach (var regenerationAbility in AbilityManager.SearchAbilitiesByExecutionType<IRegenerationPassive>())
        {
            var ability = AbilityManager.CreateInstance<IRegenerationPassive>(regenerationAbility);
            if (ability != null)
            {
                hitGain += ability.HitGainModifier(this, hitGain);
                manaGain += ability.ResourceGainModifier(this, ResourceKinds.Mana, manaGain);
                psyGain += ability.ResourceGainModifier(this, ResourceKinds.Psy, psyGain);
            }
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
                psyGain /= 2;
                break;
            default:
                if (Fighting != null)
                {
                    hitGain /= 6;
                    manaGain /= 6;
                    psyGain /= 6;
                }
                else
                {
                    hitGain /= 4;
                    manaGain /= 4;
                    psyGain /= 4;
                }
                break;
        }
        if (this[Conditions.Hunger] == 0)
        {
            hitGain /= 2;
            moveGain /= 2;
            manaGain /= 2;
            psyGain /= 2;
        }
        if (this[Conditions.Thirst] == 0)
        {
            hitGain /= 2;
            moveGain /= 2;
            manaGain /= 2;
            psyGain /= 2;
        }
        return (hitGain, moveGain, manaGain, psyGain);
    }

    protected override (decimal energy, decimal rage) CalculateResourcesDeltaBySecond()
        => (10, -1);

    protected override IWiznetFlags DeathWiznetFlags => new WiznetFlags("Deaths");

    protected override bool CreateCorpseOnDeath => true;

    protected override int CharacterTypeSpecificDamageModifier(int damage)
    {
        if (this[Conditions.Drunk] > 10)
            damage = 9 * damage / 10;
        return damage;
    }

    protected override bool CanMove => true;

    protected override bool IsAllowedToEnterTo(IRoom destination)
        => true;

    protected override bool HasBoat
        => Inventory.OfType<IItemBoat>().Any();

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
                var newDirection = RandomManager.Random<ExitDirections>() ?? direction; // change direction if possible
                direction = newDirection;
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
        if (CharacterFlags.IsSet("Flying") || CharacterFlags.IsSet("Haste"))
            moveCost /= 2; // flying is less exhausting
        if (CharacterFlags.IsSet("Slow"))
            moveCost *= 2; // being slowed is more exhausting
        if (this[ResourceKinds.MovePoints] < moveCost)
        {
            Send("You are too exhausted.");
            return false;
        }
        UpdateResource(ResourceKinds.MovePoints, -moveCost);

        // Delay player by one pulse
        SetGlobalCooldown(1);

        // Drunk ?
        if (this[Conditions.Drunk] > 10)
            Act(ActOptions.ToRoom, "{0:N} stumbles off drunkenly on {0:s} way {1}.", this, direction.DisplayNameLowerCase());
        return true;
    }

    protected override void AfterMove(ExitDirections direction, IRoom fromRoom, IRoom toRoom)
    {
        // Drunk?
        if (this[Conditions.Drunk] > 10)
            Act(ActOptions.ToRoom, "{0:N} stumbles in drunkenly, looking all nice and French.", this);

        // Autolook
        StringBuilder sb = new ();
        Room.Append(sb, this);
        Send(sb);
    }

    protected override void MoveFollow(IRoom fromRoom, IRoom toRoom, ExitDirections direction)
    {
        if (fromRoom != toRoom)
        {
            var followers = fromRoom.People.Where(x => x.Leader == this).ToArray(); // clone because Move will modify fromRoom.People
            foreach (var follower in followers)
            {
                if (follower is INonPlayableCharacter npcFollower)
                {
                    if (npcFollower.CharacterFlags.IsSet("Charm") && npcFollower.Position < Positions.Standing)
                    {
                        GameActionManager.Execute<Commands.Character.Movement.Stand, ICharacter>(npcFollower, null);
                    }
                    if (npcFollower.ActFlags.IsSet("Aggressive") && toRoom.RoomFlags.IsSet("Law"))
                    {
                        npcFollower.Master?.Act(ActOptions.ToCharacter, "You can't bring {0} into the city.", npcFollower);
                        npcFollower.Send("You aren't allowed in the city.");
                        continue;
                    }
                }

                if (follower.Position == Positions.Standing && follower.CanSee(toRoom))
                {
                    follower.Send("You follow {0}.", DebugName);
                    follower.Move(direction, true, true);
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
                pet.Enter(portal, true, true);
            }
        }
    }

    protected override IEnumerable<ICharacter> GetActTargets(ActOptions option, Positions minPosition)
    {
        if (option == ActOptions.ToGroup)
            return Group?.Members ?? this.Yield(); // no check on position
        return base.GetActTargets(option, minPosition);
    }

    protected override void HandleDeath()
    {
        // remove auras
        RemoveAuras(_ => true, false, false);
        // change position
        ChangePosition(Positions.Resting);
        // reset resources
        foreach (var resourceKind in Enum.GetValues<ResourceKinds>())
            SetResource(resourceKind, 1);
        // reset cooldowns
        ResetCooldowns();
        // release pets
        foreach (INonPlayableCharacter pet in _pets)
        {
            if (pet.Room != null)
                pet.Act(ActOptions.ToRoom, "{0:N} slowly fades away.", pet);
            RemoveFollower(pet);
            pet.ChangeMaster(null);
            CharacterManager.RemoveCharacter(pet);
        }
        _pets.Clear();
        //
        if (ImpersonatedBy != null) // If impersonated, no real death
        {
            var room = RoomManager.DefaultDeathRoom ?? RoomManager.DefaultRecallRoom;
            ChangeRoom(room, true);
            ChangePosition(Positions.Resting);
            AddAurasFromBaseFlags();
            Recompute(); // don't reset hp
        }
        else // If not impersonated, remove from game // TODO: this can be a really bad idea
        {
            CharacterManager.RemoveCharacter(this);
        }
    }

    protected override void HandleWimpy()
    {
        if (this[ResourceKinds.HitPoints] > 0 && this[ResourceKinds.HitPoints] <= Wimpy && GlobalCooldown < Pulse.PulseViolence / 2)
            Flee();
    }

    protected override (int thac0_00, int thac0_32) GetThac0()
    {
        if (Class != null)
            return Class.Thac0;
        return (20, 0);
    }

    protected override SchoolTypes NoWeaponDamageType => SchoolTypes.Bash;

    protected override int NoWeaponBaseDamage
    {
        get
        {
            var (percentage, _) = GetAbilityLearnedAndPercentage("Hand to hand");
            int learned = percentage;
            return RandomManager.Range(1 + 4 * learned / 100, 2 * Level / 3 * learned / 100);
        }
    }

    protected override string NoWeaponDamageNoun => "hit";

    protected override void DeathPayoff(ICharacter? killer) // Lose xp/reputation..
    {
        var killedStatistics = AvatarStatisticTypes.NoSourceKilled;
        if (killer != null)
        {
            killedStatistics = killer is IPlayableCharacter
                ? AvatarStatisticTypes.PcKilled
                : AvatarStatisticTypes.NpcKilled;
        }
        IncrementStatistics(killedStatistics);
        // 5/6 way back to previous level.
        var loss = -5 * ExperienceToLevel / 6;
        GainExperience(loss);
    }

    protected void RemoveGeneratedQuests()
    {
        var generatedQuests = ActiveQuests.OfType<IGeneratedQuest>().ToArray();
        foreach (var generatedQuest in generatedQuests)
        {
            generatedQuest.Delete();
            RemoveQuest(generatedQuest);
        }
    }

    protected void AddLearnedAbilityGroup(IAbilityGroupLearned abilityGroupLearned)
    {
        if (!_learnedAbilityGroups.ContainsKey(abilityGroupLearned.Name))
            _learnedAbilityGroups.Add(abilityGroupLearned.Name, abilityGroupLearned);
    }

    protected override void RecomputeKnownAbilities()
    {
        if (Race is IPlayableRace playableRace)
            MergeAbilities(playableRace.Abilities, true);
        // loop among learned ability groups
        foreach (var learnedAbilityGroup in _learnedAbilityGroups)
        {
            var abilityGroupUsage = Class.AvailableAbilityGroups.SingleOrDefault(x => StringCompareHelpers.StringEquals(x.Name, learnedAbilityGroup.Key));
            if (abilityGroupUsage != null)
            {
                foreach (var abilityDefinition in abilityGroupUsage.AbilityGroupDefinition.AbilityDefinitions)
                {
                    var abilityUsage = Class.AvailableAbilities.SingleOrDefault(x => StringCompareHelpers.StringEquals(x.Name, abilityDefinition.Name));
                    if (abilityUsage != null)
                    {
                        MergeAbility(abilityUsage, false);
                    }
                }
            }
        }
    }

    protected override IAbilityLearned? GetAbilityLearned(string abilityName)
        => ImmortalMode.IsSet("Omniscient")
            ? OmniscienceManager[abilityName]
            : base.GetAbilityLearned(abilityName);

    #endregion

    private int ExperienceByLevel => 1000 * ((Race as IPlayableRace)?.ClassExperiencePercentageMultiplier(Class) ?? 100) / 100;

    private (int experience, int alignment) ComputeExperienceAndAlignment(ICharacter victim, int totalLevel)
    {
        var experience = 0;
        var alignment = 0;

        var levelDiff = victim.Level - Level;
        // compute base experience
        var baseExp = levelDiff switch
        {
            -9 => 1,
            -8 => 2,
            -7 => 5,
            -6 => 9,
            -5 => 11,
            -4 => 22,
            -3 => 33,
            -2 => 50,
            -1 => 66,
            0 => 83,
            1 => 99,
            2 => 121,
            3 => 143,
            4 => 165,
            _ => 0,
        };
        if (levelDiff > 4)
            baseExp = 160 + 20 * (levelDiff - 4);
        // do alignment computation
        bool noAlign = victim is INonPlayableCharacter npcVictim && npcVictim.ActFlags.IsSet("NoAlign");
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
        var (_, _, _, _, _, practice, _, hitpoint, _) = TableValues.Bonus(this);

        //
        int addHitpoints = hitpoint + RandomManager.Range(Class?.MinHitPointGainPerLevel ?? 0, Class?.MaxHitPointGainPerLevel ?? 1);
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

        int addPractice = practice;

        SetBaseMaxResource(ResourceKinds.HitPoints, BaseMaxResource(ResourceKinds.HitPoints) + addHitpoints, false);
        SetBaseMaxResource(ResourceKinds.MovePoints, BaseMaxResource(ResourceKinds.MovePoints) + addHitpoints, false);
        SetBaseMaxResource(ResourceKinds.Mana, BaseMaxResource(ResourceKinds.Mana) + addMana, false);
        // TODO: psy
        Practices += addPractice;
        Trains++;

        Send("You gain {0} hit point{1}, {2} mana, {3} move, and {4} practice{5}.", addHitpoints, addHitpoints == 1 ? "" : "s", addMana, addMove, addPractice, addPractice == 1 ? "" : "s");
        // Inform about new abilities
        var newAbilities = LearnedAbilities.Where(x => x.Level == Level && x.Learned == 0).ToArray();
        if (newAbilities.Length != 0)
        {
            StringBuilder sb = new ();
            sb.AppendLine("You can now gain following abilities: %c%");
            foreach (var abilityLearned in newAbilities)
            {
                sb.AppendLine(abilityLearned.Name.ToPascalCase());
                abilityLearned.IncrementLearned(1); // set to 1%
            }
            sb.Append("%x%");
            Send(sb);
        }
    }

    //
    private string DebuggerDisplay => $"PC {Name} IMP:{ImpersonatedBy?.Name} IMM:{ImmortalMode} R:{Room?.Id} F:{Fighting?.Name}";
}
