using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mud.Common;
using Mud.Common.Attributes;
using Mud.DataStructures.Trie;
using Mud.Domain;
using Mud.Domain.SerializationData;
using Mud.Server.Ability;
using Mud.Server.Ability.AbilityGroup;
using Mud.Blueprints.Character;
using Mud.Server.Common;
using Mud.Server.Common.Extensions;
using Mud.Server.Domain;
using Mud.Server.Flags;
using Mud.Server.Flags.Interfaces;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.AbilityGroup;
using Mud.Server.Interfaces.Affect;
using Mud.Server.Interfaces.Affect.Character;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Class;
using Mud.Server.Interfaces.Combat;
using Mud.Server.Interfaces.Effect;
using Mud.Server.Interfaces.Entity;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Item;
using Mud.Server.Interfaces.Player;
using Mud.Server.Interfaces.Quest;
using Mud.Server.Interfaces.Race;
using Mud.Server.Interfaces.Room;
using Mud.Server.Interfaces.Table;
using Mud.Server.Options;
using Mud.Server.Quest;
using Mud.Server.Random;
using System.Collections.ObjectModel;
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
    private int MaxLevel { get; }

    private readonly List<IQuest> _quests;
    private readonly int[] _conditions;
    private readonly Dictionary<string, string> _aliases;
    private readonly List<INonPlayableCharacter> _pets;
    private readonly Dictionary<string, IAbilityGroupLearned> _learnedAbilityGroups;

    public PlayableCharacter(ILogger<PlayableCharacter> logger, IGameActionManager gameActionManager, ICommandParser commandParser, IAbilityManager abilityManager, IOptions<MessageForwardOptions> messageForwardOptions, IOptions<WorldOptions> worldOptions, IRandomManager randomManager, ITableValues tableValues, IRoomManager roomManager, IItemManager itemManager, ICharacterManager characterManager, IAuraManager auraManager, IWeaponEffectManager weaponEffectManager, IFlagsManager flagsManager, IWiznet wiznet, IRaceManager raceManager, IClassManager classManager, IQuestManager questManager, IResistanceCalculator resistanceCalculator, IRageGenerator rageGenerator, IAffectManager affectManager, IAbilityGroupManager abilityGroupManager)
        : base(logger, gameActionManager, commandParser, abilityManager, messageForwardOptions, randomManager, tableValues, roomManager, itemManager, characterManager, auraManager, resistanceCalculator, rageGenerator, weaponEffectManager, affectManager, flagsManager, wiznet)
    {
        WorldOptions = worldOptions;
        ClassManager = classManager;
        RaceManager = raceManager;
        QuestManager = questManager;
        AbilityGroupManager = abilityGroupManager;
        MaxLevel = WorldOptions.Value.MaxLevel;

        _quests = [];
        _conditions = new int[EnumHelpers.GetCount<Conditions>()];
        _aliases = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
        _pets = [];
        _learnedAbilityGroups = [];
    }

    public void Initialize(Guid guid, PlayableCharacterData data, IPlayer player, IRoom room)
    {
        Initialize(guid, data.Name, string.Empty);

        Incarnatable = false;

        ImpersonatedBy = player;

        Room = RoomManager.NullRoom; // add in null room to avoid problem if an initializer needs a room

        // Extract informations from PlayableCharacterData
        CreationTime = data.CreationTime;
        Class = ClassManager[data.Class]!;
        if (Class == null)
        {
            Class = ClassManager.Classes.First();
            Wiznet.Log($"Invalid class '{data.Class}' for character {data.Name}!!", WiznetFlags.Bugs, AdminLevels.Implementor);
        }
        Race = RaceManager[data.Race]!;
        if (Race == null)
        {
            Race = RaceManager.PlayableRaces.First();
            Wiznet.Log($"Invalid race '{data.Race}' for character {data.Name}!!", WiznetFlags.Bugs, AdminLevels.Implementor);
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
                SetMaxResource(maxResourceData.Key, maxResourceData.Value, false);
        }
        // setting current resources
        if (data.CurrentResources != null)
        {
            foreach (var currentResourceData in data.CurrentResources)
                SetResource(currentResourceData.Key, currentResourceData.Value);
        }
        else
        {
            Wiznet.Log($"PlayableCharacter.ctor: currentResources not found in pfile for {data.Name}", WiznetFlags.Bugs, AdminLevels.Implementor);
            // set to 1 if not found
            foreach (ResourceKinds resource in Enum.GetValues<ResourceKinds>())
                SetResource(resource, 1);
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
        BaseCharacterFlags = new CharacterFlags(data.CharacterFlags);
        BaseImmunities = new IRVFlags(data.Immunities);
        BaseResistances = new IRVFlags(data.Resistances);
        BaseVulnerabilities = new IRVFlags(data.Vulnerabilities);
        BaseShieldFlags = new ShieldFlags(data.ShieldFlags);
        BaseSex = data.Sex;
        BaseSize = data.Size;
        if (data.Attributes != null)
        {
            foreach (var attributeData in data.Attributes)
                SetBaseAttributes(attributeData.Key, attributeData.Value, false);
        }
        else
        {
            Wiznet.Log($"PlayableCharacter.ctor: attributes not found in pfile for {data.Name}", WiznetFlags.Bugs, AdminLevels.Implementor);
            // set to 1 if not found
            foreach (CharacterAttributes attribute in Enum.GetValues<CharacterAttributes>())
                this[attribute] = 1;
        }
        InitializeCurrentHitPoints(data.HitPoints); // cannot use SetHitPoints because CurrentMax has not been yet calculated (will be calculated in ResetAttributes)
        InitializeCurrentMovePoints(data.MovePoints); // cannot use SetMovePoints because CurrentMax has not been yet calculated (will be calculated in ResetAttributes)

        // TODO: set not-found attributes to base value (from class/race)

        // Must be built before equiping
        BuildEquipmentSlots();

        // Equipped items
        if (data.Equipments != null)
        {
            // Create item in inventory and try to equip it
            foreach (EquippedItemData equippedItemData in data.Equipments)
            {
                if (equippedItemData.Item == null)
                {
                    Wiznet.Log($"Item to equip in slot {equippedItemData.Slot} for character {data.Name} is null.", WiznetFlags.Bugs, AdminLevels.Implementor);
                }
                else
                {
                    // Create in inventory
                    var item = ItemManager.AddItem(Guid.NewGuid(), equippedItemData.Item, this);
                    if (item != null)
                    {
                        // Try to equip it
                        var equippedItem = SearchEquipmentSlot(equippedItemData.Slot, false);
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
                                Wiznet.Log($"Item blueprint Id {equippedItemData.Item.ItemId} cannot be equipped anymore in slot {equippedItemData.Slot} for character {data.Name}.", WiznetFlags.Bugs, AdminLevels.Implementor);
                            }
                        }
                        else
                        {
                            Wiznet.Log($"Item blueprint Id {equippedItemData.Item.ItemId} was supposed to be equipped in first empty slot {equippedItemData.Slot} for character {data.Name} but this slot doesn't exist anymore.", WiznetFlags.Bugs, AdminLevels.Implementor);
                        }
                    }
                    else
                    {
                        Wiznet.Log($"Item blueprint Id {equippedItemData.Item.ItemId} cannot be created in slot {equippedItemData.Slot} for character {data.Name}.", WiznetFlags.Bugs, AdminLevels.Implementor);
                    }
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
            {
                QuestManager.AddQuest(questData, this);
            }
        }
        // Auras
        if (data.Auras != null)
        {
            foreach (AuraData auraData in data.Auras)
                AuraManager.AddAura(this, auraData, false);
        }
        // Learn abilities
        if (data.LearnedAbilities != null)
        {
            foreach (LearnedAbilityData learnedAbilityData in data.LearnedAbilities)
            {
                var abilityDefinition = AbilityManager[learnedAbilityData.Name];
                if (abilityDefinition == null)
                    Wiznet.Log($"LearnedAbility:  Ability {learnedAbilityData.Name} doesn't exist anymore", WiznetFlags.Bugs, AdminLevels.Implementor);
                else
                {
                    var abilityLearned = new AbilityLearned(learnedAbilityData, abilityDefinition);
                    AddLearnedAbility(abilityLearned);
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
                    Wiznet.Log($"LearnedAbilityGroup:  Ability group {learnedAbilityGroupData.Name} doesn't exist anymore", WiznetFlags.Bugs, AdminLevels.Implementor);
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
                    Wiznet.Log($"Cooldown: ability {cooldown.Key} doesn't exist anymore", WiznetFlags.Bugs, AdminLevels.Implementor);
                else
                    SetCooldown(cooldown.Key, Pulse.FromPulse(cooldown.Value));
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
                    Wiznet.Log($"Pet blueprint id {petData.BlueprintId} doesn't exist anymore", WiznetFlags.Bugs, AdminLevels.Implementor);
                }
                else
                {
                    INonPlayableCharacter pet = CharacterManager.AddNonPlayableCharacter(Guid.NewGuid(), blueprint, petData, room);
                    AddPet(pet);
                }
            }
        }

        //
        Room = room;
        room.Enter(this);

        RecomputeKnownAbilities();
        ResetAttributes();
        RecomputeCurrentResourceKinds();
        AddAurasFromBaseFlags();
    }

    #region IPlayableCharacter

    #region ICharacter

    #region IEntity

    #region IActor

    public override IReadOnlyTrie<IGameActionInfo> GameActions => GameActionManager.GetGameActions<PlayableCharacter>();

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

    public override string DebugName => $"{DisplayName}[{ImpersonatedBy?.DisplayName ?? "???"}]";

    public override string RelativeDisplayName(ICharacter beholder, bool capitalizeFirstLetter = false)
    {
        StringBuilder displayName = new();
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
        ResetCooldowns();
        DeleteInventory();
        DeleteEquipments();
        Room = RoomManager.NullRoom; // this will avoid a lot of problem, will be set to null in Cleanup phase
    }

    #endregion

    public override int MaxCarryWeight => IsImmortal
        ? 10000000
        : base.MaxCarryWeight;

    public override int MaxCarryNumber => IsImmortal
        ? 1000
        : base.MaxCarryNumber;

    // Combat
    public override void MultiHit(ICharacter? victim, IMultiHitModifier? multiHitModifier) // 'this' starts a combat with 'victim'
    {
        if (victim == null)
            return;

        // no attacks for stunnies
        if (Stunned > 0)
        {
            Stunned--;
            if (Stunned == 0)
                Act(ActOptions.ToAll, "{W{0:N} regain{0:v} {0:s} equilibrium.{x", this);
            return;
        }

        StandUpInCombatIfPossible();

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
        // additional hits (dual wield, second, third attack, ...)
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

    public override void KillingPayoff(ICharacter victim, IItemCorpse? corpse) // Gain xp/gold/reputation/...
    {
        // Gain xp and alignment
        if (this != victim && victim is INonPlayableCharacter) // gain xp only for non-playable victim
        {
            var members = (Group?.Members ?? this.Yield()).ToArray();
            int sumLevels = members.Sum(x => x.Level);
            // Gain xp and change alignment
            foreach (var member in members)
            {
                var (experience, alignment) = ComputeExperienceAndAlignment(victim, sumLevels);
                if (experience > 0)
                {
                    member.Send("%y%You gain {0} experience points.%x%", experience);
                    member.GainExperience(experience);
                }
                member.UpdateAlignment(alignment);
            }
            // TODO: reputation
            if (corpse != null && !corpse.IsPlayableCharacterCorpse)
            {
                // autoloot
                if (AutoFlags.HasFlag(AutoFlags.Loot) && corpse.Content.Any())
                {
                    var corpseContent = new ReadOnlyCollection<IItem>(corpse.Content.Where(CanSee).ToList());
                    foreach (var item in corpseContent)
                        GetItem(item, corpse);
                }

                // autogold
                if (AutoFlags.HasFlag(AutoFlags.Gold) && corpse.Content.Any())
                {
                    var corpseContent = new ReadOnlyCollection<IItemMoney>(corpse.Content.OfType<IItemMoney>().Where(CanSee).ToList());
                    foreach (var money in corpseContent)
                        GetItem(money, corpse);
                }

                // autosac
                if (AutoFlags.HasFlag(AutoFlags.Sacrifice) && !corpse.Content.Any()) // TODO: corpse empty only if autoloot is set?
                    SacrificeItem(corpse);
            }
        }
    }

    // Abilities
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
        if (abilityLearned != null && abilityLearned.Level <= Level)
            learned = abilityLearned.Learned;

        if (Daze > 0)
        {
            if (abilityLearned?.AbilityDefinition.Type == AbilityTypes.Spell)
                learned /= 2;
            else
                learned = (learned * 2) / 3;
        }

        if (this[Conditions.Drunk] > 10)
            learned = (learned * 9 ) / 10;

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

    public bool IsImmortal { get; protected set; }

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
        Wimpy = Math.Clamp(wimpy, 0, MaxHitPoints/2);
    }

    public AutoFlags AutoFlags { get; protected set; }

    public void AddAutoFlags(AutoFlags autoFlags)
    {
        AutoFlags |= autoFlags;
    }

    public void RemoveAutoFlags(AutoFlags autoFlags)
    {
        AutoFlags &= ~autoFlags;
    }

    public int this[Conditions condition]
    {
        get
        {
            int index = (int)condition;
            if (index >= _conditions.Length)
            {
                Wiznet.Log($"Trying to get current condition for condition {condition} (index {index}) but current condition length is smaller", WiznetFlags.Bugs, AdminLevels.Implementor);
                return NoCondition;
            }
            return _conditions[index];
        }
        protected set
        {
            int index = (int)condition;
            if (index >= _conditions.Length)
            {
                Wiznet.Log($"Trying to get current condition for condition {condition} (index {index}) but current condition length is smaller", WiznetFlags.Bugs, AdminLevels.Implementor);
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

    public override bool CanSee(IItem? target)
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
                    Wiznet.Log($"{DebugName} has attained level {Level}", WiznetFlags.Levels);
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
                SetHitPoints(MaxHitPoints);
                SetMovePoints(MaxMovePoints);
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
            Wiznet.Log($"PlayableCharacter.CheckAbilityImprove: multiplier had invalid value {multiplier}", WiznetFlags.Bugs, AdminLevels.Implementor);
            multiplier = 1;
        }
        int difficultyMultiplier = abilityLearned.Rating;
        if (difficultyMultiplier <= 0)
        {
            Wiznet.Log($"PlayableCharacter.CheckAbilityImprove: difficulty multiplier had invalid value {multiplier} for KnownAbility {abilityLearned.Name} Player {DebugName}", WiznetFlags.Bugs, AdminLevels.Implementor);
            difficultyMultiplier = 1;
        }
        // TODO: percentage depends on intelligence replace CurrentAttributes(CharacterAttributes.Intelligence) with values from 3 to 85
        int chance = 10 * TableValues.LearnBonus(this) / (multiplier * difficultyMultiplier * 4) + Level;
        if (RandomManager.Range(1, 1000) > chance)
            return false;
        // now that the character has a CHANCE to learn, see if they really have
        if (abilityUsedSuccessfully)
        {
            chance = Math.Clamp(100 - abilityLearned.Learned, 5, 95);
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
            chance = Math.Clamp(abilityLearned.Learned / 2, 5, 30);
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

    public void AddLearnedAbilityGroup(IAbilityGroupUsage abilityGroupUsage)
    {
        var learnedAbilityGroup = new AbilityGroupLearned(abilityGroupUsage);
        if (!_learnedAbilityGroups.ContainsKey(learnedAbilityGroup.Name))
            _learnedAbilityGroups.Add(learnedAbilityGroup.Name, learnedAbilityGroup);
        RecomputeKnownAbilities();
    }

    // Immortality
    public void ChangeImmortalState(bool isImmortal)
    {
        if (IsImmortal && !isImmortal)
            Wiznet.Log($"{DebugName} is not immortal anymore.", WiznetFlags.Immortal, AdminLevels.God);
        else if (!IsImmortal && isImmortal)
            Wiznet.Log($"{DebugName} is now immortal.", WiznetFlags.Immortal, AdminLevels.God);
        IsImmortal = isImmortal;
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
        Wiznet.Log($"{DebugName} sacrifices {item.DebugName} as a burnt offering.", WiznetFlags.Saccing);
        ItemManager.RemoveItem(item);
        //
        long silver = Math.Max(1, item.Level * 3);
        if (item is not IItemCorpse)
            silver = Math.Min(silver, item.Cost);
        if (silver <= 0)
        {
            Send("Mota doesn't give you anything for your sacrifice."); // TODO: god
            Wiznet.Log($"DoSacrifice: {item.DebugName} gives zero or negative money {silver}!", WiznetFlags.Bugs, AdminLevels.Implementor);
        }
        else if (silver == 1)
            Send("Mota gives you one silver coin for your sacrifice.");
        else
            Send("Mota gives you {0} silver coins for your sacrifice.", silver);
        if (silver > 0)
            SilverCoins += silver;
        // autosplit
        if (silver > 0 && AutoFlags.HasFlag(AutoFlags.Split))
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
        if (shareSilver == 0 || shareGold == 0)
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
    public PlayableCharacterData MapPlayableCharacterData()
    {
        PlayableCharacterData data = new()
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
            HitPoints = CurrentHitPoints,
            MovePoints = CurrentMovePoints,
            CurrentResources = Enum.GetValues<ResourceKinds>().ToDictionary(x => x, x => this[x]),
            MaxResources = Enum.GetValues<ResourceKinds>().ToDictionary(x => x, MaxResource),
            Alignment = Alignment,
            Wimpy = Wimpy,
            Experience = Experience,
            Trains = Trains,
            Practices = Practices,
            AutoFlags = AutoFlags,
            Conditions = Enum.GetValues<Conditions>().ToDictionary(x => x, x => this[x]),
            Equipments = Equipments.Where(x => x.Item != null).Select(x => x.MapEquippedData()).ToArray(),
            Inventory = Inventory.Select(x => x.MapItemData()).ToArray(),
            CurrentQuests = Quests.Select(x => x.MapQuestData()).ToArray(),
            Auras = MapAuraData(),
            CharacterFlags = BaseCharacterFlags.Serialize(),
            Immunities = BaseImmunities.Serialize(),
            Resistances = BaseResistances.Serialize(),
            Vulnerabilities = BaseVulnerabilities.Serialize(),
            ShieldFlags = BaseShieldFlags.Serialize(),
            Attributes = Enum.GetValues<CharacterAttributes>().ToDictionary(x => x, BaseAttribute),
            LearnedAbilities = LearnedAbilities.Select(x => x.MapLearnedAbilityData()).ToArray(),
            LearnedAbilityGroups = LearnedAbilityGroups.Select(x => x.MapLearnedAbilityGroupData()).ToArray(),
            Aliases = Aliases.ToDictionary(x => x.Key, x => x.Value),
            Cooldowns = AbilitiesInCooldown.ToDictionary(x => x.Key, x => x.Value),
            Pets = Pets.Select(x => x.MapPetData()).ToArray(),
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
        if (got && AutoFlags.HasFlag(AutoFlags.Split)
                && (silver > 0 || gold > 0))
            SplitMoney(silver, gold);
        return true;
    }

    protected override bool AutomaticallyDisplayRoom => true;

    protected override (decimal hit, decimal move, decimal mana, decimal psy) CalculateResourcesDeltaByMinute()
    {
        decimal hitGain = Math.Max(3, this[CharacterAttributes.Constitution] - 3 + Level / 2);
        decimal moveGain = Math.Max(15, Level);
        decimal manaGain = (this[CharacterAttributes.Wisdom] + this[CharacterAttributes.Intelligence] + Level) / 2;
        decimal psyGain = (this[CharacterAttributes.Wisdom] + this[CharacterAttributes.Intelligence] + Level) / 2; // TODO: correct formula
        // regen
        if (CharacterFlags.IsSet("Regeneration"))
            hitGain *= 2;
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
        if (CurrentMovePoints < moveCost)
        {
            Send("You are too exhausted.");
            return false;
        }
        UpdateMovePoints(-moveCost);

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
            var followers = new ReadOnlyCollection<ICharacter>(fromRoom.People.Where(x => x.Leader == this).ToList()); // clone because Move will modify fromRoom.People
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

    protected override IEnumerable<ICharacter> GetActTargets(ActOptions option)
    {
        if (option == ActOptions.ToGroup)
            return Group?.Members ?? this.Yield();
        return base.GetActTargets(option);
    }

    protected override void HandleDeath()
    {
        ChangePosition(Positions.Resting);
        SetHitPoints(1);
        SetMovePoints(1);
        foreach (var resourceKind in Enum.GetValues<ResourceKinds>())
            SetResource(resourceKind, 1);
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
        if (ImpersonatedBy != null) // If impersonated, no real death
        {
            var room = RoomManager.DefaultDeathRoom ?? RoomManager.DefaultRecallRoom;
            ChangeRoom(room, true);
            ChangePosition(Positions.Sleeping);
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
        if (CurrentHitPoints > 0 && CurrentHitPoints <= Wimpy && GlobalCooldown < Pulse.PulseViolence / 2)
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
        // 5/6 way back to previous level.
        var loss = -5 * ExperienceToLevel / 6;
        GainExperience(loss);
    }

    protected void AddLearnedAbilityGroup(IAbilityGroupLearned abilityGroupLearned)
    {
        if (!_learnedAbilityGroups.ContainsKey(abilityGroupLearned.Name))
            _learnedAbilityGroups.Add(abilityGroupLearned.Name, abilityGroupLearned);
    }

    protected override void RecomputeKnownAbilities()
    {
        if (Race is IPlayableRace playableRace)
            MergeAbilities(playableRace.Abilities, true, false);
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
                        MergeAbility(abilityUsage, false, abilityGroupUsage.IsBasics);
                    }
                }
            }
        }
    }

    #endregion

    private int ExperienceByLevel => 1000 * ((Race as IPlayableRace)?.ClassExperiencePercentageMultiplier(Class) ?? 100) / 100;

    private (int experience, int alignment) ComputeExperienceAndAlignment(ICharacter victim, int totalLevel)
    {
        int experience;
        int alignment = 0;

        int levelDiff = victim.Level - Level;
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

        SetBaseAttributes(CharacterAttributes.MaxHitPoints, MaxHitPoints + addHitpoints, false);
        SetMaxResource(ResourceKinds.Mana, this[ResourceKinds.Mana] + addMana, false);
        SetBaseAttributes(CharacterAttributes.MaxMovePoints, MaxMovePoints + addHitpoints, false);
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
                sb.AppendLine(abilityLearned.Name);
                abilityLearned.IncrementLearned(1); // set to 1%
            }
            sb.Append("%x%");
            Send(sb);
        }
    }

    //
    private string DebuggerDisplay => $"PC {Name} IMP:{ImpersonatedBy?.Name} IMM:{IsImmortal} R:{Room?.Id} F:{Fighting?.Name}";
}
