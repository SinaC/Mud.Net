using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mud.Blueprints.Character;
using Mud.Common;
using Mud.Common.Attributes;
using Mud.DataStructures.Trie;
using Mud.Domain;
using Mud.Domain.SerializationData.Avatar;
using Mud.Server.Ability.Skill;
using Mud.Server.Ability.Spell;
using Mud.Server.Affects.Character;
using Mud.Server.Common;
using Mud.Server.Common.Extensions;
using Mud.Server.Common.Helpers;
using Mud.Server.Domain;
using Mud.Server.Flags;
using Mud.Server.Flags.Interfaces;
using Mud.Server.GameAction;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.Ability;
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
using Mud.Server.Interfaces.Race;
using Mud.Server.Interfaces.Room;
using Mud.Server.Interfaces.Special;
using Mud.Server.Interfaces.Table;
using Mud.Server.Options;
using Mud.Server.Quest.Objectives;
using Mud.Server.Random;
using System.Diagnostics;
using System.Text;

namespace Mud.Server.Character.NonPlayableCharacter;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
[Export(typeof(INonPlayableCharacter))]
public class NonPlayableCharacter : CharacterBase, INonPlayableCharacter
{
    private IRaceManager RaceManager { get; }
    private IClassManager ClassManager { get; }
    private ISpecialBehaviorManager SpecialBehaviorManager { get; }

    public NonPlayableCharacter(ILogger<NonPlayableCharacter> logger, IGameActionManager gameActionManager, ICommandParser commandParser, IAbilityManager abilityManager, IOptions<MessageForwardOptions> messageForwardOptions, IRandomManager randomManager, ITableValues tableValues, IRoomManager roomManager, IItemManager itemManager, ICharacterManager characterManager, IAuraManager auraManager, IWeaponEffectManager weaponEffectManager, IWiznet wiznet, IRaceManager raceManager, IClassManager classManager, IResistanceCalculator resistanceCalculator, IRageGenerator rageGenerator, IAffectManager affectManager, IFlagsManager flagsManager, ISpecialBehaviorManager specialBehaviorManager)
        : base(logger, gameActionManager, commandParser, abilityManager, messageForwardOptions, randomManager, tableValues, roomManager, itemManager, characterManager, auraManager, resistanceCalculator, rageGenerator, weaponEffectManager, affectManager, flagsManager, wiznet)
    {
        RaceManager = raceManager;
        ClassManager = classManager;
        SpecialBehaviorManager = specialBehaviorManager;
    }

    protected void Initialize(Guid guid, string name, string description, CharacterBlueprintBase blueprint, IRoom room)
    {
        Initialize(guid, name, description);

        Blueprint = blueprint;

        Level = blueprint.Level;
        Position = Positions.Standing;
        Race = RaceManager[blueprint.Race]!;
        if (Race == null && !string.IsNullOrWhiteSpace(blueprint.Race))
            Logger.LogWarning("Unknown race '{race}' for npc {blueprintId}", blueprint.Race, blueprint.Id);
        Class = ClassManager[blueprint.Class]!;
        if (Class == null && !string.IsNullOrWhiteSpace(blueprint.Class))
            Logger.LogWarning("Unknown class '{class}' for npc {blueprintId}", blueprint.Class, blueprint.Id);
        DamageNoun = blueprint.DamageNoun;
        DamageType = blueprint.DamageType;
        DamageDiceCount = blueprint.DamageDiceCount;
        DamageDiceValue = blueprint.DamageDiceValue;
        DamageDiceBonus = blueprint.DamageDiceBonus;

        ActFlags = NewAndCopyAndSet(() => new ActFlags(), blueprint.ActFlags, Race?.ActFlags);
        OffensiveFlags = NewAndCopyAndSet(() => new OffensiveFlags(), blueprint.OffensiveFlags, Race?.OffensiveFlags);
        AssistFlags = NewAndCopyAndSet(() => new AssistFlags(), blueprint.AssistFlags, Race?.AssistFlags);
        BaseBodyForms = NewAndCopyAndSet(() => new BodyForms(), Blueprint.BodyForms, Race?.BodyForms);
        BaseBodyParts = NewAndCopyAndSet(() => new BodyParts(), Blueprint.BodyParts, Race?.BodyParts);
        BaseCharacterFlags = NewAndCopyAndSet(() => new CharacterFlags(), blueprint.CharacterFlags, Race?.CharacterFlags);
        BaseImmunities = NewAndCopyAndSet(() => new IRVFlags(), blueprint.Immunities, Race?.Immunities);
        BaseResistances = NewAndCopyAndSet(() => new IRVFlags(), blueprint.Resistances, Race?.Resistances);
        BaseVulnerabilities = NewAndCopyAndSet(() => new IRVFlags(), blueprint.Vulnerabilities, Race?.Vulnerabilities);

        BaseSex = blueprint.Sex;
        BaseSize = blueprint.Size;
        Alignment = Math.Clamp(blueprint.Alignment, -1000, 1000);
        if (blueprint.Wealth == 0)
        {
            SilverCoins = 0;
            GoldCoins = 0;
        }
        else
        {
            long wealth = RandomManager.Range(blueprint.Wealth / 2, 3 * blueprint.Wealth / 2);
            GoldCoins = RandomManager.Range(wealth / 200, wealth / 100);
            SilverCoins = wealth - (GoldCoins * 100);
        }
        // TODO: see db.C:Create_Mobile
        // TODO: following values must be extracted from blueprint
        SetBaseCharacterAttributes(blueprint);
        SetBaseAttributes(CharacterAttributes.SavingThrow, 0, false); // TODO
        SetBaseAttributes(CharacterAttributes.HitRoll, blueprint.HitRollBonus, false); // OK
        SetBaseAttributes(CharacterAttributes.DamRoll, Level, false); // TODO
        SetBaseAttributes(CharacterAttributes.ArmorBash, blueprint.ArmorBash, false); // OK
        SetBaseAttributes(CharacterAttributes.ArmorPierce, blueprint.ArmorPierce, false); // OK
        SetBaseAttributes(CharacterAttributes.ArmorSlash, blueprint.ArmorSlash, false); // OK
        SetBaseAttributes(CharacterAttributes.ArmorExotic, blueprint.ArmorExotic, false); // OK

        // resources (should be extracted from blueprint)
        // hit points
        var maxHitPoints = RandomManager.Dice(blueprint.HitPointDiceCount, blueprint.HitPointDiceValue) + blueprint.HitPointDiceBonus;
        SetBaseMaxResource(ResourceKinds.HitPoints, maxHitPoints, false); // OK
        SetCurrentMaxResource(ResourceKinds.HitPoints, maxHitPoints);
        SetResource(ResourceKinds.HitPoints, maxHitPoints);
        // move points
        SetBaseMaxResource(ResourceKinds.MovePoints, 100, false); // TODO
        SetCurrentMaxResource(ResourceKinds.MovePoints, 100);
        SetResource(ResourceKinds.MovePoints, 100);
        // mana
        var maxManaResource = RandomManager.Dice(blueprint.ManaDiceCount, blueprint.ManaDiceValue) + blueprint.ManaDiceBonus;
        SetBaseMaxResource(ResourceKinds.Mana, maxManaResource, false);
        SetCurrentMaxResource(ResourceKinds.Mana, maxManaResource);
        SetResource(ResourceKinds.Mana, maxManaResource);
        // TODO: psy

        //
        Position = blueprint.StartPosition;

        if (!string.IsNullOrWhiteSpace(blueprint.SpecialBehavior))
            SpecialBehavior = SpecialBehaviorManager.CreateInstance(blueprint.SpecialBehavior);

        BuildEquipmentSlots();

        Room = room;
        room.Enter(this);
    }

    public void Initialize(Guid guid, CharacterBlueprintBase blueprint, IRoom room) // NPC
    {
        Initialize(guid, blueprint.Name, blueprint.Description, blueprint, room);

        RecomputeKnownAbilities();
        ResetAttributesAndResourcesAndFlags();
        RecomputeCurrentResourceKinds();
        AddAurasFromBaseFlags();
    }

    public void Initialize(Guid guid, CharacterBlueprintBase blueprint, PetData petData, IRoom room) // Pet
    {
        Initialize(guid, petData.Name, petData.Description, blueprint, room);

        BaseSex = petData.Sex;
        BaseSize = petData.Size;
        BaseCharacterFlags = new CharacterFlags(petData.CharacterFlags);
        BaseImmunities = new IRVFlags(petData.Immunities);
        BaseResistances = new IRVFlags(petData.Resistances);
        BaseVulnerabilities = new IRVFlags(petData.Vulnerabilities);
        BaseShieldFlags = new ShieldFlags(petData.ShieldFlags);
        // attributes
        if (petData.Attributes != null)
        {
            foreach (var attributeData in petData.Attributes)
                SetBaseAttributes(attributeData.Key, attributeData.Value, false);
        }
        else
        {
            Wiznet.Log($"NonPlayableCharacter.ctor: attributes not found in pfile for {petData.Name}", WiznetFlags.Bugs, AdminLevels.Implementor);
            // set to 1 if not found
            foreach (CharacterAttributes attribute in Enum.GetValues<CharacterAttributes>())
                this[attribute] = 1;
        }
        // resources
        if (petData.MaxResources != null)
        {
            foreach (var maxResourceData in petData.MaxResources)
                SetBaseMaxResource(maxResourceData.Key, maxResourceData.Value, false);
        }
        if (petData.CurrentResources != null)
        {
            foreach (var currentResourceData in petData.CurrentResources)
                SetResource(currentResourceData.Key, currentResourceData.Value);
        }
        else
        {
            Wiznet.Log($"NonPlayableCharacter.ctor: currentResources not found in pfile for {petData.Name}", WiznetFlags.Bugs, AdminLevels.Implementor);
            // set to 1 if not found
            foreach (var resource in Enum.GetValues<ResourceKinds>())
                SetResource(resource, 1);
        }

        // Equipped items
        if (petData.Equipments != null)
        {
            // Create item in inventory and try to equip it
            foreach (var equippedItemData in petData.Equipments)
            {
                if (equippedItemData.Item == null)
                {
                    Wiznet.Log($"Item to equip in slot {equippedItemData.Slot} for character {petData.Name} is null.", WiznetFlags.Bugs, AdminLevels.Implementor);
                }
                else
                {
                    // Create in inventory
                    var item = ItemManager.AddItem(Guid.NewGuid(), equippedItemData.Item!, this);
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
                                Wiznet.Log($"Item blueprint Id {equippedItemData.Item!.ItemId} cannot be equipped anymore in slot {equippedItemData.Slot} for character {petData.Name}.", WiznetFlags.Bugs, AdminLevels.Implementor);
                            }
                        }
                        else
                        {
                            Wiznet.Log($"Item blueprint Id {equippedItemData.Item!.ItemId} was supposed to be equipped in first empty slot {equippedItemData.Slot} for character {petData.Name} but this slot doesn't exist anymore.", WiznetFlags.Bugs, AdminLevels.Implementor);
                        }
                    }
                    else
                    {
                        Wiznet.Log($"Item blueprint Id {equippedItemData.Item.ItemId} cannot be created in slot {equippedItemData.Slot} for character {petData.Name}.", WiznetFlags.Bugs, AdminLevels.Implementor);
                    }
                }
            }
        }
        // Inventory
        if (petData.Inventory != null)
        {
            foreach (var itemData in petData.Inventory)
                ItemManager.AddItem(Guid.NewGuid(), itemData, this);
        }
        // Auras
        if (petData.Auras != null)
        {
            foreach (var auraData in petData.Auras)
                AuraManager.AddAura(this, auraData, false);
        }

        RecomputeKnownAbilities();
        ResetAttributesAndResourcesAndFlags();
        RecomputeCurrentResourceKinds();
        AddAurasFromBaseFlags();
    }

    #region INonPlayableCharacter

    #region ICharacter

    #region IEntity

    #region IActor

    public override IReadOnlyTrie<IGameActionInfo> GameActions => GameActionManager.GetGameActions<NonPlayableCharacter>();

    public override void Send(string message, bool addTrailingNewLine)
    {
        base.Send(message, addTrailingNewLine);
        if (ForwardSlaveMessages && Master != null)
        {
            if (PrefixForwardedMessages)
                message = "<CTRL|" + DisplayName + ">" + message;
            Master.Send(message, addTrailingNewLine);
        }
    }

    public override void Page(StringBuilder text)
    {
        base.Page(text);
        if (ForwardSlaveMessages)
            Master?.Page(text);
    }

    #endregion

    public override string DisplayName => Blueprint?.ShortDescription?.UpperFirstLetter() ?? "???";

    public override string DebugName => $"{DisplayName}[{Blueprint?.Id ?? -1}]";

    public override string RelativeDisplayName(ICharacter beholder, bool capitalizeFirstLetter = false)
    {
        StringBuilder displayName = new();
        var playableBeholder = beholder as IPlayableCharacter;
        if (playableBeholder != null && IsQuestObjective(playableBeholder, true))
            displayName.Append(StringHelpers.QuestPrefix);
        if (beholder.CanSee(this))
            displayName.Append(DisplayName);
        else if (capitalizeFirstLetter)
            displayName.Append("Someone");
        else
            displayName.Append("someone");
        if (beholder.ImmortalMode.HasFlag(ImmortalModeFlags.Holylight))
            displayName.Append($" [id: {Blueprint?.Id.ToString() ?? " ??? "}]");
        return displayName.ToString();
    }

    public override void OnRemoved() // called before removing a character from the game
    {
        // Free from slavery, must be done before base.OnRemoved because CharacterBase.OnRemoved will call Leader.RemoveFollower which will reset Master
        Master?.RemovePet(this);

        base.OnRemoved();

        StopFighting(true);
        // TODO: what if character is incarnated
        ResetCooldowns();
        DeleteInventory();
        DeleteEquipments();
        Room = RoomManager.NullRoom; // this will avoid a lot of problem, will be set to null in Cleanup phase
    }

    public override void OnCleaned() // called when removing definitively an entity from the game
    {
        Blueprint = null!;
        Room = null!;
    }

    #endregion

    public override bool CanSee(IItem? target)
    {
        if (target is IItemQuest questItem)
            return false;
        return base.CanSee(target);
    }

    public override ImmortalModeFlags ImmortalMode => ImmortalModeFlags.None;

    public override int MaxCarryWeight => ActFlags.IsSet("Pet")
        ? 0
        : base.MaxCarryWeight;

    public override int MaxCarryNumber => ActFlags.IsSet("Pet")
        ? 0
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
                Act(ActOptions.ToAll, "%W%{0:N} regain{0:v} {0:s} equilibrium.%x%", this);
            return;
        }

        var mainHand = GetEquipment<IItemWeapon>(EquipmentSlots.MainHand);
        // main hand attack
        int attackCount = 0;
        OneHit(victim, mainHand, multiHitModifier);
        attackCount++;
        if (Fighting != victim)
            return;
        if (multiHitModifier?.MaxAttackCount <= attackCount)
            return;
        // area attack
        if (OffensiveFlags.IsSet("AreaAttack"))
        {
            if (Room != null)
            {
                var people = Room.People.Where(x => x != this && x.Fighting == this).ToArray();
                foreach (var character in people)
                    OneHit(character, mainHand, multiHitModifier);
                attackCount++;
            }
        }
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
        // additional hits from abilities (dual wield, second, third attack, ...)
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
        //// 3rd hand
        //var thirdWieldLearnInfo = GetLearnInfo("Third wield");
        //var thirdWieldChance = thirdWieldLearnInfo.learned / 6;
        //if (CharacterFlags.HasFlag(CharacterFlags.Slow))
        //    thirdWieldChance = 0;
        //if (RandomManager.Chance(thirdWieldChance))
        //    OneHit(victim, mainHand, multiHitModifier);
        //if (Fighting != victim)
        //    return;
        //if (multiHitModifier?.MaxAttackCount <= 5)
        //    return;
        //// 4th hand
        //var FourthWieldLearnInfo = GetLearnInfo("Fourth wield");
        //var FourthWieldChance = FourthWieldLearnInfo.learned / 8;
        //if (CharacterFlags.HasFlag(CharacterFlags.Slow))
        //    FourthWieldChance = 0;
        //if (RandomManager.Chance(FourthWieldChance))
        //    OneHit(victim, mainHand, multiHitModifier);
        //if (Fighting != victim)
        //    return;
        //if (multiHitModifier?.MaxAttackCount <= 6)
        //    return;
        // fun stuff

        if (GlobalCooldown > 0 || Position < Positions.Standing) // wait until GCD is elapsed and standing up
            return;

        int number = RandomManager.Range(0, 9);
        switch (number)
        {
            case 0: if (OffensiveFlags.IsSet("Bash"))
                    UseSkill("Bash", CommandParser.NoParameters);
                break;
            case 1: if (OffensiveFlags.IsSet("Berserk") && !CharacterFlags.IsSet("Berserk"))
                    UseSkill("Berserk", CommandParser.NoParameters);
                break;
            case 2: if (OffensiveFlags.IsSet("Disarm")
                    || ActFlags.HasAny("Warrior", "Thief")) // TODO: check if weapon skill is not hand to hand
                    UseSkill("Disarm", CommandParser.NoParameters);
                break;
            case 3: if (OffensiveFlags.IsSet("Kick"))
                    UseSkill("Kick", CommandParser.NoParameters);
                break;
            case 4: if (OffensiveFlags.IsSet("DirtKick"))
                    UseSkill("Dirt Kicking", CommandParser.NoParameters);
                break;
            case 5: if (OffensiveFlags.IsSet("Tail"))
                    UseSkill("Tail", CommandParser.NoParameters);
                break;
            case 6: if (OffensiveFlags.IsSet("Trip"))
                    UseSkill("Trip", CommandParser.NoParameters);
                break;
            case 7: if (OffensiveFlags.IsSet("Crush"))
                    UseSkill("Crush", CommandParser.NoParameters);
                break;
            case 8: if (OffensiveFlags.IsSet("Backstab"))
                    UseSkill("Backstab", CommandParser.NoParameters); // TODO: this will never works because we cannot backstab while in combat -> replace with circle
                break;
            case 9: if (OffensiveFlags.IsSet("Bite"))
                    UseSkill("Bite", CommandParser.NoParameters);
                break;
        }
    }

    public override void KillingPayoff(ICharacter victim, IItemCorpse? corpse)
    {
        // NOP
    }

    #endregion

    public CharacterBlueprintBase Blueprint { get; private set; } = null!;

    public string DamageNoun { get; protected set; } = null!;
    public SchoolTypes DamageType { get; protected set; }
    public int DamageDiceCount { get; protected set; }
    public int DamageDiceValue { get; protected set; }
    public int DamageDiceBonus { get; protected set; }

    public IActFlags ActFlags { get; protected set; } = null!;

    public IOffensiveFlags OffensiveFlags { get; protected set; } = null!;

    public IAssistFlags AssistFlags { get; protected set; } = null!;

    // special behavior
    public ISpecialBehavior? SpecialBehavior { get; protected set; } = null!;

    public bool IsQuestObjective(IPlayableCharacter questingCharacter, bool checkCompleted)
    {
        // If 'this' is NPC and in objective list or in kill loot table
        return questingCharacter.Quests.Where(q => !checkCompleted || (checkCompleted && !q.AreObjectivesFulfilled)).SelectMany(q => q.Objectives).OfType<KillQuestObjective>().Any(o => !o.IsCompleted && o.TargetBlueprint.Id == Blueprint.Id)
                || questingCharacter.Quests.Where(q => !checkCompleted || (checkCompleted && !q.AreObjectivesFulfilled)).Any(q => q.KillLootTable.ContainsKey(Blueprint.Id));
    }


    public IPlayableCharacter? Master { get; protected set; }

    public void ChangeMaster(IPlayableCharacter? master)
    {
        if (Master != null && master != null)
            return; // cannot change from one master to another
        Master = master;
    }

    public bool Order(string commandLine)
    {
        if (Master == null)
            return false;
        Act(ActOptions.ToCharacter, "{0:N} orders you to '{1}'.", Master, commandLine);
        CommandParser.ExtractCommandAndParameters(commandLine, out string command, out ICommandParameter[] parameters);
        bool executed = ExecuteCommand(commandLine, command, parameters);
        return executed;
    }

    //
    public bool CastSpell(string spellName, IEntity target)
    {
        var spellDefinition = AbilityManager[spellName];
        if (spellDefinition == null)
        {
            Logger.LogError("NPC:CastSpell: spell {spellName} not found", spellName);
            return false;
        }
        var spellInstance = AbilityManager.CreateInstance<ISpell>(spellDefinition.Name);
        if (spellInstance == null)
        {
            Logger.LogError("NPC:CastSpell: cannot create instance of spell {spellName}", spellDefinition.Name);
            return false;
        }
        var spellActionInput = new SpellActionInput(spellDefinition, this, Level, new CommandParameter(target.Name, target.Name, 1));
        var spellInstanceGuards = spellInstance.Setup(spellActionInput);
        if (spellInstanceGuards != null)
        {
            Logger.LogWarning("NPC:CastSpell: cannot setup spell {spellName} on target {targetName}: {spellInstanceGuards}", spellDefinition.Name, target.Name, spellInstanceGuards);
            Send(spellInstanceGuards);
            return false;
        }
        spellInstance.Execute();
        return true;
    }

    // Mapping
    public PetData MapPetData()
    {
        PetData data = new()
        {
            BlueprintId = Blueprint.Id,
            Description = Description,
            Name = Name,
            //RoomId = Room?.Blueprint?.Id ?? 0,
            Race = Race?.Name ?? string.Empty,
            Class = Class?.Name ?? string.Empty,
            Level = Level,
            Sex = BaseSex,
            Size = BaseSize,
            //SilverCoins = SilverCoins,
            //GoldCoins = GoldCoins,
            CurrentResources = Enum.GetValues<ResourceKinds>().ToDictionary(x => x, x => this[x]),
            MaxResources = Enum.GetValues<ResourceKinds>().ToDictionary(x => x, MaxResource),
            Equipments = Equipments.Where(x => x.Item != null).Select(x => x.MapEquippedData()).ToArray(),
            Inventory = Inventory.Select(x => x.MapItemData()).ToArray(),
            Auras = MapAuraData(),
            CharacterFlags = BaseCharacterFlags.Serialize(),
            Immunities = BaseImmunities.Serialize(),
            Resistances = BaseResistances.Serialize(),
            Vulnerabilities = BaseVulnerabilities.Serialize(),
            ShieldFlags = BaseShieldFlags.Serialize(),
            Attributes = Enum.GetValues<CharacterAttributes>().ToDictionary(x => x, BaseAttribute),
            //KnownAbilities = KnownAbilities.Select(x => x.MapKnownAbilityData()).ToArray(),
            //Cooldowns = AbilitiesInCooldown.ToDictionary(x => x.Key.Id, x => x.Value),
        };
        return data;
    }

    #endregion

    #region CharacterBase

    // Abilities
    public override (int percentage, IAbilityLearned? abilityLearned) GetWeaponLearnedAndPercentage(IItemWeapon? weapon)
    {
        int learned;
        if (weapon == null) // hand to hand
            learned = 40 + 2 * Level;
        else
        {
            learned = weapon.Type switch
            {
                WeaponTypes.Exotic => 3 * Level,
                _ => 40 + (5 * Level) / 2,
            };
        }

        learned = Math.Clamp(learned, 0, 100);

        return (learned, null);
    }

    public override (int percentage, IAbilityLearned? abilityLearned) GetAbilityLearnedAndPercentage(string abilityName) // TODO: replace with npc class
    {
        var abilityLearned = GetAbilityLearned(abilityName);
        //int learned = 0;
        //if (knownAbility != null && knownAbility.Level <= Level)
        //    learned = knownAbility.Learned;

        // TODO: spells
        int learned;
        if (StringCompareHelpers.StringEquals(abilityName, "Sneak") || StringCompareHelpers.StringEquals(abilityName, "Hide"))
            learned = 20 + 2 * Level;
        else if (StringCompareHelpers.StringEquals(abilityName, "Dodge")
            && OffensiveFlags.IsSet("Dodge"))
            learned = 2 * Level;
        else if (StringCompareHelpers.StringEquals(abilityName, "Parry")
            && OffensiveFlags.IsSet("Parry"))
            learned = 2 * Level;
        else if (StringCompareHelpers.StringEquals(abilityName, "Shield Block"))
            learned = 10 + 2 * Level;
        else if (StringCompareHelpers.StringEquals(abilityName, "Second attack")
            && ActFlags.HasAny("Warrior", "Thief"))
            learned = 10 + 3 * Level;
        else if (StringCompareHelpers.StringEquals(abilityName, "Third attack")
            && ActFlags.IsSet("Warrior"))
            learned = 4 * Level - 40;
        else if (StringCompareHelpers.StringEquals(abilityName, "Hand to hand"))
            learned = 40 + 2 * Level;
        else if (StringCompareHelpers.StringEquals(abilityName, "Trip")
            && OffensiveFlags.IsSet("Trip"))
            learned = 10 + 3 * Level;
        else if (StringCompareHelpers.StringEquals(abilityName, "Bash")
            && OffensiveFlags.IsSet("Bash"))
            learned = 10 + 3 * Level;
        else if (StringCompareHelpers.StringEquals(abilityName, "Disarm")
            && (OffensiveFlags.IsSet("Disarm") || ActFlags.HasAny("Warrior", "Thief")))
            learned = 20 + 3 * Level;
        else if (StringCompareHelpers.StringEquals(abilityName, "Berserk")
            && OffensiveFlags.IsSet("Berserk"))
            learned = 3 * Level;
        else if (StringCompareHelpers.StringEquals(abilityName, "Kick")
            && OffensiveFlags.IsSet("Kick"))
            learned = 10 + 3 * Level;
        else if (StringCompareHelpers.StringEquals(abilityName, "Backstab")
            && ActFlags.IsSet("Thief"))
            learned = 20 + 2 * Level;
        else if (StringCompareHelpers.StringEquals(abilityName, "Rescue"))
            learned = 40 + Level;
        else if (StringCompareHelpers.StringEquals(abilityName, "Tail")
            && OffensiveFlags.IsSet("Tail"))
            learned = 10 + 3 * Level;
        else if (StringCompareHelpers.StringEquals(abilityName, "Bite")
            && OffensiveFlags.IsSet("Bite"))
             learned = 10 + 3 * Level;
        else if (StringCompareHelpers.StringEquals(abilityName, "Crush")
            && OffensiveFlags.IsSet("Crush"))
            learned = 10 + 3 * Level;
        else if (StringCompareHelpers.StringEquals(abilityName, "Recall"))
            learned = 40 + Level;
        else if (StringCompareHelpers.StringEquals(abilityName, "Axe")
            || StringCompareHelpers.StringEquals(abilityName, "Dagger")
            || StringCompareHelpers.StringEquals(abilityName, "Flail")
            || StringCompareHelpers.StringEquals(abilityName, "Mace")
            || StringCompareHelpers.StringEquals(abilityName, "Polearm")
            || StringCompareHelpers.StringEquals(abilityName, "Spear")
            || StringCompareHelpers.StringEquals(abilityName, "Staff(weapon)")
            || StringCompareHelpers.StringEquals(abilityName, "Sword")
            || StringCompareHelpers.StringEquals(abilityName, "Whip"))
                learned = 40 + 5 * Level / 2;
        else
            learned = 0;

        if (Daze > 0)
        {
            if (abilityLearned?.AbilityUsage.AbilityDefinition.Type == AbilityTypes.Spell)
                learned /= 2;
            else
                learned = (learned * 2) / 3;
        }

        learned = Math.Clamp(learned, 0, 100);
        return (learned, abilityLearned);
    }

    protected override Positions DefaultPosition
        => Blueprint.DefaultPosition;

    protected override bool CannotDie => Blueprint is CharacterShopBlueprintBase || Blueprint is CharacterQuestorBlueprint || ActFlags.HasAny("Train", "Gain", "Practice", "IsHealer");

    protected override bool CheckEquippedItemsDuringRecompute() // no equipment check for NPC
        => false;

    protected override int MaxAllowedBasicAttribute(BasicAttributes basicAttribute)
        => 25;

    protected override (decimal hit, decimal move, decimal mana, decimal psy) CalculateResourcesDeltaByMinute()
    {
        decimal hitGain = 5 + Level;
        decimal moveGain = Level;
        decimal manaGain = 5 + Level;
        decimal psyGain = 5 + Level;
        if (CharacterFlags.IsSet("Regeneration"))
            hitGain *= 2;
        switch (Position)
        {
            case Positions.Sleeping:
                hitGain = (3 * hitGain) / 2;
                manaGain = (3 * manaGain) / 2;
                psyGain = (3 * psyGain) / 2;
                break;
            case Positions.Resting:
                // nop
                break;
            default:
                if (Fighting != null)
                {
                    hitGain /= 3;
                    manaGain /= 3;
                    psyGain /= 3;
                }
                else
                {
                    hitGain /= 2;
                    manaGain /= 2;
                    psyGain /= 2;
                }

                break;
        }
        return (hitGain, moveGain, manaGain, psyGain);
    }

    protected override (decimal energy, decimal rage) CalculateResourcesDeltaBySecond()
        => (10, -1);

    protected override bool CanGoTo(IRoom destination)
    {
        return !destination.RoomFlags.IsSet("NoMob")
            && !(ActFlags.IsSet("Aggressive") && destination.RoomFlags.IsSet("Law"));
    }

    protected override ExitDirections ChangeDirectionBeforeMove(ExitDirections direction, IRoom fromRoom)
    {
        return direction; // no direction change
    }

    protected override bool BeforeMove(ExitDirections direction, IRoom fromRoom, IRoom toRoom)
    {
        return true; // nop
    }

    protected override void AfterMove(ExitDirections direction, IRoom fromRoom, IRoom toRoom)
    {
        if (IncarnatedBy != null)
        {
            StringBuilder sb = new ();
            Room?.Append(sb, this);
            Send(sb);
        }
    }

    protected override void HandleDeath()
    {
        CharacterManager.RemoveCharacter(this);
    }

    protected override void HandleWimpy()
    {
        if (GlobalCooldown < Pulse.PulseViolence/2)
        {
            if ((ActFlags.IsSet("Wimpy") && this[ResourceKinds.HitPoints] < MaxResource(ResourceKinds.HitPoints) / 5 && RandomManager.Chance(25))
                || (CharacterFlags.IsSet("Charm") && Master != null && Master.Room != Room))
                Flee();
        }
    }

    protected override (int thac0_00, int thac0_32) GetThac0()
    {
        if (Class != null)
            return Class.Thac0;

        int thac0_00 = 20;
        int thac0_32 = -4; // as good as thief

        if (ActFlags.IsSet("Warrior"))
            thac0_32 = -10;
        else if (ActFlags.IsSet("Thief"))
            thac0_32 = -4;
        else if (ActFlags.IsSet("Cleric"))
            thac0_32 = 2;
        else if (ActFlags.IsSet("Mage"))
            thac0_32 = 6;

        return (thac0_00, thac0_32);
    }

    protected override SchoolTypes NoWeaponDamageType => DamageType;

    protected override int NoWeaponBaseDamage => RandomManager.Dice(DamageDiceCount, DamageDiceValue) + DamageDiceBonus;

    protected override string NoWeaponDamageNoun => DamageNoun;

    protected override void DeathPayoff(ICharacter? killer)
    {
        // NOP
    }

    protected override void RecomputeKnownAbilities()
    {
        if (Class != null) // NPC gain access to all abilities from their class
            MergeAbilities(Class.AvailableAbilities, false, false);
    }

    protected override void AddAurasFromBaseFlags()
    {
        base.AddAurasFromBaseFlags();

        if (OffensiveFlags.IsSet("Fast") && !BaseCharacterFlags.IsSet("Haste") && !Auras.HasAffect<ICharacterFlagsAffect>(x => x.Modifier.IsSet("Haste")))
        {
            // TODO: code copied from haste spell (except duration and aura flags) use effect ??
            var hasteAbilityDefinition = AbilityManager["Haste"];
            var modifier = 1 + (Level >= 18 ? 1 : 0) + (Level >= 25 ? 1 : 0) + (Level >= 32 ? 1 : 0);
            AuraManager.AddAura(this, hasteAbilityDefinition?.Name ?? "Haste", this, Level, AuraFlags.Permanent, false,
                new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.Dexterity, Modifier = modifier, Operator = AffectOperators.Add },
                new CharacterFlagsAffect { Modifier = new CharacterFlags("Haste"), Operator = AffectOperators.Or },
                new CharacterAdditionalHitAffect { AdditionalHitCount = 1 });
        }
    }

    #endregion

    protected bool UseSkill(string skillName, params ICommandParameter[] parameters)
    {
        Logger.LogInformation("{name} tries to use {skillName} on {fightingName}.", DebugName, skillName, Fighting?.DebugName ?? "???");
        var abilityDefinition = AbilityManager[skillName];
        if (abilityDefinition == null)
        {
            Logger.LogWarning("Unknown skill {skillName}.", skillName);
            Send("This skill doesn't exist.");
            return false;
        }
        if (abilityDefinition.Type != AbilityTypes.Skill)
        {
            Logger.LogWarning("{skillName} is not a skill.", skillName);
            Send("This is not a skill.");
            return false;
        }
        var skillInstance = AbilityManager.CreateInstance<ISkill>(abilityDefinition.Name);
        if (skillInstance == null)
        {
            Logger.LogWarning("Skill {skillName} cannot be instantiated.", skillName);
            Send("This skill cannot be used.");
            return false;
        }
        var skillActionInput = new SkillActionInput(abilityDefinition, this, parameters);
        var setupResult = skillInstance.Setup(skillActionInput);
        if (setupResult != null)
        {
            Send(setupResult);
            return false;
        }
        skillInstance.Execute();
        return true;
    }

    // TODO: should be moved to rom24
    private void SetBaseCharacterAttributes(CharacterBlueprintBase blueprint)
    {
        var strength = Math.Min(25, 11 + Level / 4);
        var intelligence = Math.Min(25, 11 + Level / 4);
        var wisdom = Math.Min(25, 11 + Level / 4);
        var dexterity = Math.Min(25, 11 + Level / 4);
        var constitution = Math.Min(25, 11 + Level / 4);

        if (blueprint.ActFlags.IsSet("Warrior"))
        {
            strength += 3;
            constitution += 2;
            intelligence -= 1;
        }
        if (blueprint.ActFlags.IsSet("Thief"))
        {
            dexterity += 3;
            intelligence += 1;
            wisdom -= 1;
        }
        if (blueprint.ActFlags.IsSet("Cleric"))
        {
            wisdom += 3;
            strength += 1;
            dexterity -= 1;
        }
        if (blueprint.ActFlags.IsSet("Mage"))
        {
            intelligence += 3;
            dexterity += 1;
            strength -= 1;
        }
        if (blueprint.OffensiveFlags.IsSet("Fast"))
            dexterity += 2;
        strength += blueprint.Size - Sizes.Medium;
        constitution += (blueprint.Size - Sizes.Medium) / 2;

        SetBaseAttributes(CharacterAttributes.Strength, strength, false);
        SetBaseAttributes(CharacterAttributes.Intelligence, intelligence, false);
        SetBaseAttributes(CharacterAttributes.Wisdom, wisdom, false);
        SetBaseAttributes(CharacterAttributes.Dexterity, dexterity, false);
        SetBaseAttributes(CharacterAttributes.Constitution, constitution, false);
    }

    //
    private string DebuggerDisplay => $"PC {Name} BId:{Blueprint?.Id} INC:{IncarnatedBy?.Name} R:{Room?.Blueprint?.Id} F:{Fighting?.Name}";
}
