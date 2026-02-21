using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mud.Blueprints.Character;
using Mud.Blueprints.MobProgram;
using Mud.Common;
using Mud.Common.Attributes;
using Mud.Domain;
using Mud.Domain.SerializationData.Avatar;
using Mud.Flags;
using Mud.Flags.Interfaces;
using Mud.Random;
using Mud.Server.Ability.Interfaces;
using Mud.Server.Ability.Passive.Interfaces;
using Mud.Server.Ability.Skill;
using Mud.Server.Ability.Skill.Interfaces;
using Mud.Server.Ability.Spell;
using Mud.Server.Ability.Spell.Interfaces;
using Mud.Server.Affects.Character;
using Mud.Server.Class.Interfaces;
using Mud.Server.Common;
using Mud.Server.Common.Extensions;
using Mud.Server.Common.Helpers;
using Mud.Server.Domain;
using Mud.Server.Effects.Interfaces;
using Mud.Server.Flags.Interfaces;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.Affect;
using Mud.Server.Interfaces.Affect.Character;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Combat;
using Mud.Server.Interfaces.Entity;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Item;
using Mud.Server.Interfaces.MobProgram;
using Mud.Server.Interfaces.Room;
using Mud.Server.Interfaces.Special;
using Mud.Server.Interfaces.Table;
using Mud.Server.Loot.Interfaces;
using Mud.Server.Options;
using Mud.Server.Parser;
using Mud.Server.Parser.Interfaces;
using Mud.Server.Quest.Objectives;
using Mud.Server.Race.Interfaces;
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
    private IMobProgramProcessor MobProgramProcessor { get; }

    public NonPlayableCharacter(ILogger<NonPlayableCharacter> logger, IGameActionManager gameActionManager, IParser parser, IOptions<MessageForwardOptions> messageForwardOptions, IAbilityManager abilityManager, IRandomManager randomManager, ITableValues tableValues, IRoomManager roomManager, IItemManager itemManager, ICharacterManager characterManager, IAuraManager auraManager, IWeaponEffectManager weaponEffectManager, IWiznet wiznet, ILootManager lootManager, IAggroManager aggroManager, IRaceManager raceManager, IClassManager classManager, IResistanceCalculator resistanceCalculator, IRageGenerator rageGenerator, IAffectManager affectManager, IFlagsManager flagsManager, ISpecialBehaviorManager specialBehaviorManager, IMobProgramProcessor mobProgramProcessor)
        : base(logger, gameActionManager, parser, messageForwardOptions, abilityManager, randomManager, tableValues, roomManager, itemManager, characterManager, auraManager, resistanceCalculator, rageGenerator, weaponEffectManager, affectManager, flagsManager, wiznet, lootManager, aggroManager)
    {
        RaceManager = raceManager;
        ClassManager = classManager;
        SpecialBehaviorManager = specialBehaviorManager;
        MobProgramProcessor = mobProgramProcessor;
        ImmortalMode = new ImmortalModes();
    }

    protected void Initialize(Guid guid, string name, string description, CharacterBlueprintBase blueprint, IRoom room)
    {
        Initialize(guid, name, description);

        Blueprint = blueprint;

        MobProgramDelay = -1;
        Level = blueprint.Level;
        Position = Positions.Standing;
        BaseRace = RaceManager[blueprint.Race]!;
        if (BaseRace == null && !string.IsNullOrWhiteSpace(blueprint.Race))
            Logger.LogWarning("Unknown race '{race}' for npc {blueprintId}", blueprint.Race, blueprint.Id);
        var @class = ClassManager[blueprint.Class]!;
        if (@class == null)
        {
            if (!string.IsNullOrWhiteSpace(blueprint.Class))
                Logger.LogWarning("Unknown class '{class}' for npc {blueprintId}", blueprint.Class, blueprint.Id);
            Classes = [];
        }
        else
            Classes = [@class];
        DamageNoun = blueprint.DamageNoun;
        DamageType = blueprint.DamageType;
        DamageDiceCount = blueprint.DamageDiceCount;
        DamageDiceValue = blueprint.DamageDiceValue;
        DamageDiceBonus = blueprint.DamageDiceBonus;

        ActFlags = NewAndCopyAndSet(() => new ActFlags(), blueprint.ActFlags, BaseRace?.ActFlags);
        OffensiveFlags = NewAndCopyAndSet(() => new OffensiveFlags(), blueprint.OffensiveFlags, BaseRace?.OffensiveFlags);
        AssistFlags = NewAndCopyAndSet(() => new AssistFlags(), blueprint.AssistFlags, BaseRace?.AssistFlags);
        BaseBodyForms = NewAndCopyAndSet(() => new BodyForms(), Blueprint.BodyForms, BaseRace?.BodyForms);
        BaseBodyParts = NewAndCopyAndSet(() => new BodyParts(), Blueprint.BodyParts, BaseRace?.BodyParts);
        BaseCharacterFlags = NewAndCopyAndSet(() => new CharacterFlags(), blueprint.CharacterFlags, BaseRace?.CharacterFlags);
        BaseImmunities = NewAndCopyAndSet(() => new IRVFlags(), blueprint.Immunities, BaseRace?.Immunities);
        BaseResistances = NewAndCopyAndSet(() => new IRVFlags(), blueprint.Resistances, BaseRace?.Resistances);
        BaseVulnerabilities = NewAndCopyAndSet(() => new IRVFlags(), blueprint.Vulnerabilities, BaseRace?.Vulnerabilities);

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
            var wealth = RandomManager.Range(blueprint.Wealth / 2, 3 * blueprint.Wealth / 2);
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

        UpdateEquimentSlots(BaseRace);

        Room = room;
        room.Enter(this);
        SpawnRoom = room;
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
            Wiznet.Log($"NonPlayableCharacter.ctor: attributes not found in pfile for {petData.Name}", new WiznetFlags("Bugs"), AdminLevels.Implementor);
            // set to 1 if not found
            foreach (var attribute in Enum.GetValues<CharacterAttributes>())
                this[attribute] = 1;
        }
        // resources
        if (petData.MaxResources != null)
        {
            foreach (var maxResourceData in petData.MaxResources)
            {
                SetBaseMaxResource(maxResourceData.Key, maxResourceData.Value, false);
                SetCurrentMaxResource(maxResourceData.Key, maxResourceData.Value);
            }
        }
        if (petData.CurrentResources != null)
        {
            foreach (var currentResourceData in petData.CurrentResources)
                SetResource(currentResourceData.Key, currentResourceData.Value);
        }
        else
        {
            Wiznet.Log($"NonPlayableCharacter.ctor: currentResources not found in pfile for {petData.Name}", new WiznetFlags("Bugs"), AdminLevels.Implementor);
            // set to 1 if not found
            foreach (var resource in Enum.GetValues<ResourceKinds>())
                SetResource(resource, 1);
        }

        // Auras
        if (petData.Auras != null)
        {
            foreach (var auraData in petData.Auras)
                AuraManager.AddAura(this, auraData, false);
        }

        // Equipped items
        if (petData.Equipments != null)
        {
            CreateAndTryToEquipItems(petData);
        }
        // Inventory
        if (petData.Inventory != null)
        {
            foreach (var itemData in petData.Inventory)
                ItemManager.AddItem(Guid.NewGuid(), itemData, this);
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

    public override string DebugName => $"{DisplayName}[BId:{Blueprint?.Id ?? -1}]";

    public override void OnAuraRemoved(IAura aura, bool displayWearOffMessage)
    {
        base.OnAuraRemoved(aura, displayWearOffMessage);

        if (StringCompareHelpers.StringEquals(aura.AbilityName, "Charm Person")) // if charm person wears off, remove master
            ChangeMaster(null);
    }

    public override string RelativeDisplayName(ICharacter beholder, bool capitalizeFirstLetter = false)
    {
        StringBuilder displayName = new();
        if (beholder is IPlayableCharacter playableBeholder && IsQuestObjective(playableBeholder, true))
            displayName.Append(StringHelpers.QuestPrefix);
        if (beholder.CanSee(this))
            displayName.Append(DisplayName);
        else if (capitalizeFirstLetter)
            displayName.Append("Someone");
        else
            displayName.Append("someone");
        if (beholder.ImmortalMode.IsSet("Holylight"))
            displayName.Append($" [id: {Blueprint?.Id.ToString() ?? " ??? "}]");
        return displayName.ToString();
    }

    public override void OnRemoved(IRoom nullRoom) // called before removing a character from the game
    {
        // Free from slavery, must be done before base.OnRemoved because CharacterBase.OnRemoved will call Leader.RemoveFollower which will reset Master
        Master?.RemovePet(this);

        base.OnRemoved(nullRoom);
        // TODO: what if character is incarnated
    }

    public override void OnCleaned() // called when removing definitively an entity from the game
    {
        Blueprint = null!;
        Room = null!;
    }

    #endregion

    public override bool CanSee(IItem? target)
    {
        if (target is IItemQuest)
            return false;
        return base.CanSee(target);
    }

    public override bool CanLoot(IItem? target)
    {
        if (target is IItemQuest)
            return false;
        return base.CanLoot(target);
    }

    public override IImmortalModes ImmortalMode { get; protected set; }

    public override int MaxCarryWeight => ActFlags.IsSet("Pet")
        ? 0
        : base.MaxCarryWeight;

    public override int MaxCarryNumber => ActFlags.IsSet("Pet")
        ? 0
        : base.MaxCarryNumber;

    public override IEnumerable<IPlayableCharacter> GetPlayableCharactersImpactedByKill()
    {
        if (Master == null)
            return [];
        var characters = new List<IPlayableCharacter>();
        if (Master.Group != null)
        {
            foreach (var character in Master.Group.Members)
                characters.Add(character);
        }
        else
            characters.Add(Master);
        return characters;
    }

    public override bool IsAllowedToEnterTo(IRoom destination)
        => !destination.RoomFlags.IsSet("NoMob");

    // Combat
    public override void MultiHit(ICharacter? victim, IMultiHitModifier? multiHitModifier) // 'this' starts a combat with 'victim'
    {
        var multiHitResult = MultiHits(victim, multiHitModifier);
        if (multiHitResult == MultiHitResult.NoVictim || multiHitResult == MultiHitResult.Stunned || multiHitResult == MultiHitResult.NotFightingVictimAnymore)
            return; // stops here

        // fun stuff
        if (GlobalCooldown > 0 || Position < Positions.Standing) // wait until GCD is elapsed and standing up
            return;

        UseCombatSkill();
    }

    public override void HandleAutoGold(IItemCorpse corpse)
    {
        // NOP
    }

    public override void HandleAutoLoot(IItemCorpse corpse)
    {
        // NOP
    }

    public override void HandleAutoSacrifice(IItemCorpse corpse)
    {
        // NOP
    }


    #endregion

    public CharacterBlueprintBase Blueprint { get; private set; } = null!;

    public IRoom SpawnRoom { get; private set; } = null!;

    public string DamageNoun { get; protected set; } = null!;
    public SchoolTypes DamageType { get; protected set; }
    public int DamageDiceCount { get; protected set; }
    public int DamageDiceValue { get; protected set; }
    public int DamageDiceBonus { get; protected set; }

    public IActFlags ActFlags { get; protected set; } = null!;

    public IOffensiveFlags OffensiveFlags { get; protected set; } = null!;

    public IAssistFlags AssistFlags { get; protected set; } = null!;

    public bool IsAlive
        => IsValid && this[ResourceKinds.HitPoints] > 0;

    // special behavior
    public ISpecialBehavior? SpecialBehavior { get; protected set; } = null!;

    public bool IsQuestObjective(IPlayableCharacter questingCharacter, bool checkCompleted)
    {
        // If 'this' is NPC and in objective list or in kill loot table
        return questingCharacter.ActiveQuests.Where(q => !checkCompleted || (checkCompleted && !q.AreObjectivesFulfilled)).SelectMany(q => q.Objectives).OfType<KillQuestObjective>().Any(o => !o.IsCompleted && o.TargetBlueprint.Id == Blueprint.Id)
                || questingCharacter.ActiveQuests.Where(q => !checkCompleted || (checkCompleted && !q.AreObjectivesFulfilled)).Any(q => q.Objectives.OfType<LootItemQuestObjective>().Where(o => !o.IsCompleted).Any(x => q.KillLootTable.GetValueOrDefault(Blueprint.Id)?.ObjectiveIds?.Contains(x.Id) == true));
    }

    // pet/charmies
    public bool IsPetOrCharmie
    {
        get
        {
            if (Master != null)
                return true;
            if (ActFlags.IsSet("pet"))
                return true;
            if (CharacterFlags.IsSet("charm"))
                return true;
            return false;
        }
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
        var parseResult = Parser.Parse(commandLine);
        if (parseResult == null)
            return false;
        var executed = ExecuteCommand(commandLine, parseResult);
        return executed;
    }

    //
    public bool CastSpell(string spellName, IEntity? target)
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
        var spellActionInput = target is not null
            ? new SpellActionInput(spellDefinition, this, Level, false, new CommandParameter(target.Name, target.Name, 1))
            : new SpellActionInput(spellDefinition, this, Level, false);
        var spellInstanceGuards = spellInstance.Setup(spellActionInput);
        if (spellInstanceGuards != null)
        {
            Logger.LogWarning("NPC:CastSpell: cannot setup spell {spellName} on target {targetName}: {spellInstanceGuards}", spellDefinition.Name, target?.DebugName, spellInstanceGuards);
            Send(spellInstanceGuards);
            return false;
        }
        spellInstance.Execute();
        return true;
    }

    // MobProgram triggers
    public int MobProgramDelay { get; private set; }

    public void ResetMobProgramDelay()
    {
        MobProgramDelay = -1;
    }

    public void DecreaseMobProgramDelay()
    {
        MobProgramDelay = Math.Max(0, MobProgramDelay - 1);
    }

    public void SetMobProgramDelay(int pulseCount)
    {
        if (pulseCount > MobProgramDelay)
        {
            Logger.LogTrace("SETTING MobProgramDelay to {pulseCount} for {name}", pulseCount, DebugName);
            MobProgramDelay = pulseCount;
        }
    }

    public ICharacter? MobProgramTarget { get; private set; }

    public void SetMobProgramTarget(ICharacter? target)
    {
        MobProgramTarget = target;
    }

    public bool OnAct(ICharacter triggerer, string text)
    {
        // only PC can trigger this
        if (triggerer is not IPlayableCharacter)
            return false;
        var triggered = false;
        foreach (var mobProgram in Blueprint.MobPrograms.OfType<MobProgramAct>())
        {
            if (StringCompareHelpers.StringContains(text, mobProgram.Phrase))
            {
                Logger.LogInformation("OnAct: {name}: {triggerer} {phrase}", DebugName, triggerer.DebugName, mobProgram.Phrase);

                MobProgramProcessor.Execute(this, mobProgram, triggerer, text);
                triggered = true;
            }
        }
        return triggered;
    }

    public bool OnBribe(ICharacter triggerer, long amount)
    {
        var triggered = false;
        foreach (var mobProgram in Blueprint.MobPrograms.OfType<MobProgramBribe>())
        {
            if (amount >= mobProgram.Amount)
            {
                Logger.LogInformation("OnBribe: {name}: {triggerer} {amount} >= {amountThreshold}", DebugName, triggerer.DebugName, amount, mobProgram.Amount);

                MobProgramProcessor.Execute(this, mobProgram, triggerer, amount);
                triggered = true;
            }
        }
        return triggered;
    }

    public bool OnGive(ICharacter triggerer, IItem item)
    {
        var triggered = false;
        foreach (var mobProgram in Blueprint.MobPrograms.OfType<MobProgramGive>())
        {
            var matchingMobProgramFound = false;
            if (mobProgram.IsAll)
            {
                Logger.LogInformation("OnGive: {name}: IsAll {triggerer} {itemName}", DebugName, triggerer.DebugName, item.DebugName);
                matchingMobProgramFound = true;
            }
            else if (mobProgram.ObjectId is not null && mobProgram.ObjectId.Value == item.Blueprint.Id)
            {
                Logger.LogInformation("OnGive: {name}: objectId {objectId} {triggerer} {itemName}", DebugName, mobProgram.ObjectId.Value, triggerer.DebugName, item.DebugName);
                matchingMobProgramFound = true;
            }
            else if (mobProgram.ObjectName is not null && StringCompareHelpers.AnyStringEquals(item.Keywords, mobProgram.ObjectName))
            {
                Logger.LogInformation("OnGive: {name}: objectName {objectName} {triggerer} {itemName}", DebugName, mobProgram.ObjectName, triggerer.DebugName, item.DebugName);
                matchingMobProgramFound = true;
            }
            if (matchingMobProgramFound)
            {
                MobProgramProcessor.Execute(this, mobProgram, triggerer, item);
                triggered = true;
            }
        }
        return triggered;
    }

    public bool OnSocial(ICharacter triggerer, SocialDefinition socialDefinition)
    {
        // only PC can trigger this
        if (triggerer is not IPlayableCharacter)
            return false;
        var triggered = false;
        foreach (var mobProgram in Blueprint.MobPrograms.OfType<MobProgramSocial>())
        {
            if (StringCompareHelpers.StringEquals(socialDefinition.Name, mobProgram.Social))
            {
                Logger.LogInformation("OnSocial: {name}: {triggerer} {social}", DebugName, triggerer.DebugName, socialDefinition.Name);

                MobProgramProcessor.Execute(this, mobProgram, triggerer);
                triggered = true;
            }
        }
        return triggered;
    }

    public bool OnSpeech(ICharacter triggerer, string text)
    {
        // only PC can trigger this
        if (triggerer is not IPlayableCharacter)
            return false;
        var triggered = false;
        foreach (var mobProgram in Blueprint.MobPrograms.OfType<MobProgramSpeech>())
        {
            if (StringCompareHelpers.StringContains(text, mobProgram.Phrase))
            {
                Logger.LogInformation("OnSpeech: {name}: {triggerer} {phrase}", DebugName, triggerer.DebugName, mobProgram.Phrase);

                MobProgramProcessor.Execute(this, mobProgram, triggerer, text);
                triggered = true;
            }
        }
        return triggered;
    }

    public bool OnEntry()
    {
        var triggered = false;
        foreach (var mobProgram in Blueprint.MobPrograms.OfType<MobProgramEntry>())
        {
            if (RandomManager.Chance(mobProgram.Percentage))
            {
                Logger.LogInformation("OnEntry: {name}", DebugName);

                MobProgramProcessor.Execute(this, mobProgram);
                triggered = true;
            }
        }
        return triggered;
    }

    public bool OnGreet(ICharacter triggerer)
    {
        // only PC can trigger this
        if (triggerer is not IPlayableCharacter)
            return false;
        var triggered = false;
        foreach (var mobProgram in Blueprint.MobPrograms.OfType<MobProgramGreet>())
        {
            var matchingMobProgramFound = false;
            if (!mobProgram.IsAll && CanSee(triggerer) && Blueprint.DefaultPosition == Position)
            {
                if (RandomManager.Chance(mobProgram.Percentage))
                {
                    Logger.LogInformation("OnGreet: {name}: {triggerer}", DebugName, triggerer.DebugName);
                    matchingMobProgramFound = true;
                }
            }
            else if (mobProgram.IsAll)
            {
                if (RandomManager.Chance(mobProgram.Percentage))
                {
                    Logger.LogInformation("OnGreet: IsAll {name}: {triggerer}", DebugName, triggerer.DebugName);
                    matchingMobProgramFound = true;
                }
            }
            if (matchingMobProgramFound)
            {
                MobProgramProcessor.Execute(this, mobProgram, triggerer);
                triggered = true;
            }
        }
        return triggered;
    }

    public bool OnExit(ICharacter triggerer, ExitDirections direction)
    {
        // only PC can trigger this
        if (triggerer is not IPlayableCharacter)
            return false;
        var triggered = false;
        foreach (var mobProgram in Blueprint.MobPrograms.OfType<MobProgramExit>().Where(x => x.Direction == direction))
        {
            if (!mobProgram.IsAll && CanSee(triggerer) && Blueprint.DefaultPosition == Position)
            {
                Logger.LogInformation("OnExit: {name}: {triggerer} {direction}", DebugName, triggerer.DebugName, direction);
                triggered = true;
            }
            else if (mobProgram.IsAll)
            {
                Logger.LogInformation("OnExit: {name}: IsAll {triggerer} {direction}", DebugName, triggerer.DebugName, direction);
                triggered = true;
            }
            if (triggered)
                MobProgramProcessor.Execute(this, mobProgram, triggerer, direction);
        }
        return triggered;
    }

    public bool OnKill(ICharacter triggerer)
    {
        var triggered = false;
        foreach (var mobProgram in Blueprint.MobPrograms.OfType<MobProgramKill>())
        {
            if (RandomManager.Chance(mobProgram.Percentage))
            {
                Logger.LogInformation("OnKill: {name}: {triggerer}", DebugName, triggerer.DebugName);

                MobProgramProcessor.Execute(this, mobProgram, triggerer);
                triggered = true;
            }
        }
        return triggered;
    }

    public bool OnDeath(ICharacter? killer)
    {
        var triggered = false;
        foreach (var mobProgram in Blueprint.MobPrograms.OfType<MobProgramDeath>())
        {
            if (RandomManager.Chance(mobProgram.Percentage))
            {
                Logger.LogInformation("OnDeath: {name}: {killer}", DebugName, killer?.DebugName);

                MobProgramProcessor.Execute(this, mobProgram, killer);
                triggered = true;
            }
        }
        return triggered;
    }

    public bool OnFight(ICharacter fighting)
    {
        var triggered = false;
        foreach (var mobProgram in Blueprint.MobPrograms.OfType<MobProgramFight>())
        {
            if (RandomManager.Chance(mobProgram.Percentage))
            {
                Logger.LogInformation("OnFight: {name}: {fighting}", DebugName, fighting?.DebugName);

                MobProgramProcessor.Execute(this, mobProgram, fighting);
                triggered = true;
            }
        }
        return triggered;
    }

    public bool OnHitPointPercentage(ICharacter fighting)
    {
        var triggered = false;

        var hitpointPercentage = (100 * this[ResourceKinds.HitPoints]) / this.MaxResource(ResourceKinds.HitPoints);
        foreach (var mobProgram in Blueprint.MobPrograms.OfType<MobProgramHitPointPercentage>().Where(x => hitpointPercentage < x.Percentage))
        {
            Logger.LogInformation("OnHitPointPercentage: {name}: {fighting}", DebugName, fighting?.DebugName);

            MobProgramProcessor.Execute(this, mobProgram, fighting);
            triggered = true;
        }
        return triggered;
    }

    public bool OnRandom()
    {
        var triggered = false;
        foreach (var mobProgram in Blueprint.MobPrograms.OfType<MobProgramRandom>())
        {
            if (RandomManager.Chance(mobProgram.Percentage))
            {
                Logger.LogInformation("OnRandom: {name}", DebugName);

                MobProgramProcessor.Execute(this, mobProgram);
                triggered = true;
            }
        }
        return triggered;
    }

    public bool OnDelay()
    {
        var triggered = false;
        foreach (var mobProgram in Blueprint.MobPrograms.OfType<MobProgramDelay>())
        {
            if (RandomManager.Chance(mobProgram.Percentage))
            {
                Logger.LogInformation("OnDelay: {name}", DebugName);

                MobProgramProcessor.Execute(this, mobProgram);
                triggered = true;
            }
        }
        return triggered;
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
            Race = BaseRace?.Name ?? string.Empty,
            Classes = Classes.Select(x => x.Name).ToArray(),
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

    protected override IWiznetFlags DeathWiznetFlags => new WiznetFlags("MobDeaths");

    protected override bool CreateCorpseOnDeath => !ActFlags.IsSet("NoCorpse");

    protected override int CharacterTypeSpecificDamageModifier(int damage)
        => damage; // nop

    protected override bool CanMove
    {
        get
        {
            if (Master != null && Master.Room == Room)
            {
                // Slave cannot leave a room without Master
                Send("What? And leave your beloved master?");
                return false;
            }
            return true;
        }
    }

    protected override bool HasBoat
        => Inventory.OfType<IItemBoat>().Any() || (Master != null && Master.Inventory.OfType<IItemBoat>().Any());

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
        // Free from slavery, must be done before RemoveCharacter because CharacterBase.OnRemoved will call Leader.RemoveFollower which will reset Master
        Master?.RemovePet(this);

        CharacterManager.RemoveCharacter(this);
    }

    protected override void HandleWimpy()
    {
        if (GlobalCooldown < Pulse.PulseViolence/2)
        {
            if ((ActFlags.IsSet("Wimpy") && this[ResourceKinds.HitPoints] < MaxResource(ResourceKinds.HitPoints) / 5 && RandomManager.Chance(25)) // wimpy
                || (CharacterFlags.IsSet("Charm") && Master != null && Master.Room != Room)) // charmies flee when master is not in the same room
                Flee();
        }
    }

    protected override (int thac0_00, int thac0_32) GetThac0()
    {
        if (Classes.Any())
            return Classes.Thac0();

        var thac0_00 = 20;
        var thac0_32 = -4; // as good as thief

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
        if (Race is IPlayableRace playableRace)
            MergeAbilities(playableRace.Abilities, true);
        if (Classes.Any()) // NPC gain access to all abilities from their classes
            MergeAbilities(Classes.AvailableAbilities(), false);
    }

    protected override void AddAurasFromBaseFlags()
    {
        base.AddAurasFromBaseFlags();

        if (OffensiveFlags.IsSet("Fast") && !BaseCharacterFlags.IsSet("Haste") && !Auras.HasAffect<ICharacterFlagsAffect>(x => x.Modifier.IsSet("Haste")))
        {
            // TODO: code copied from haste spell (except duration and aura flags) use effect ??
            var hasteAbilityDefinition = AbilityManager["Haste"];
            var modifier = 1 + (Level >= 18 ? 1 : 0) + (Level >= 25 ? 1 : 0) + (Level >= 32 ? 1 : 0);
            AuraManager.AddAura(this, hasteAbilityDefinition?.Name ?? "Haste", this, Level, new AuraFlags("Permanent"), false,
                new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.Dexterity, Modifier = modifier, Operator = AffectOperators.Add },
                new CharacterFlagsAffect { Modifier = new CharacterFlags("Haste"), Operator = AffectOperators.Or },
                new CharacterAdditionalHitAffect { AdditionalHitCount = 1 },
                new CharacterRegenModifierAffect { Modifier = 2, Operator = AffectOperators.Divide });
        }
        if (BaseCharacterFlags.IsSet("Slow"))
        {
            // TODO: code copied from slow spell (except duration and aura flags) use effect ??
            var slowAbilityDefinition = AbilityManager["Slow"];
            var duration = Level / 2;
            var modifier = -1 - (Level >= 18 ? 1 : 0) - (Level >= 25 ? 1 : 0) - (Level >= 32 ? 1 : 0);
            AuraManager.AddAura(this, slowAbilityDefinition?.Name ?? "Slow", this, Level, TimeSpan.FromMinutes(duration), true,
                new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.Dexterity, Modifier = modifier, Operator = AffectOperators.Add },
                new CharacterFlagsAffect { Modifier = new CharacterFlags("Slow"), Operator = AffectOperators.Or },
                new CharacterRegenModifierAffect { Modifier = 2, Operator = AffectOperators.Divide });
        }
        if (BaseCharacterFlags.IsSet("Poison"))
        {
            var poisonAffect = AffectManager.CreateInstance("Poison");
            var poisonAbilityDefinition = AbilityManager["Poison"];
            var duration = Level;
            AuraManager.AddAura(this, poisonAbilityDefinition?.Name ?? "Poison", this, Level, TimeSpan.FromMinutes(duration), true,
               new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.Strength, Modifier = -2, Operator = AffectOperators.Add },
               new CharacterFlagsAffect { Modifier = new CharacterFlags("Poison"), Operator = AffectOperators.Or },
               poisonAffect,
               new CharacterRegenModifierAffect { Modifier = 4, Operator = AffectOperators.Divide });
        }
        if (BaseCharacterFlags.IsSet("Plague"))
        {
            var plagueAffect = AffectManager.CreateInstance("Plague");
            var plagueAbilityDefinition = AbilityManager["Plague"];
            var duration = RandomManager.Range(1, 2 * Level);
            AuraManager.AddAura(this, plagueAbilityDefinition?.Name ?? "Plague", this, Level, TimeSpan.FromMinutes(duration), true,
                new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.Strength, Modifier = -5, Operator = AffectOperators.Add },
                new CharacterFlagsAffect { Modifier = new CharacterFlags("Plague"), Operator = AffectOperators.Or },
                plagueAffect,
                new CharacterRegenModifierAffect { Modifier = 8, Operator = AffectOperators.Divide });
        }
    }

    #endregion

    protected bool UseSkill(string skillName, params ICommandParameter[] parameters)
    {
        Logger.LogInformation("{name} uses {skillName} on {fightingName}.", DebugName, skillName, Fighting?.DebugName ?? "???");
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
    private enum MultiHitResult
    {
        Ok,
        NoVictim,
        Stunned,
        NotFightingVictimAnymore,
        MaxAttackReached,
        StopBecauseAdditionHitFailed,
    }

    private MultiHitResult MultiHits(ICharacter? victim, IMultiHitModifier? multiHitModifier) // 'this' starts a combat with 'victim'
    {
        if (victim == null)
            return MultiHitResult.NoVictim;

        // no attacks for stunnies
        if (IsStunned)
        {
            DecreaseStun();
            if (!IsStunned)
                Act(ActOptions.ToAll, "%W%{0:N} regain{0:v} {0:s} equilibrium.%x%", this);
            return MultiHitResult.Stunned;
        }

        var mainHand = GetEquipment<IItemWeapon>(EquipmentSlots.MainHand);

        // main hand attack
        var attackCount = 0;
        OneHit(victim, mainHand, multiHitModifier);
        attackCount++;
        if (Fighting != victim)
            return MultiHitResult.NotFightingVictimAnymore;
        if (multiHitModifier?.MaxAttackCount <= attackCount)
            return MultiHitResult.MaxAttackReached;

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
                    return MultiHitResult.NotFightingVictimAnymore;
                if (multiHitModifier?.MaxAttackCount <= attackCount)
                    return MultiHitResult.MaxAttackReached;
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
                    return MultiHitResult.NotFightingVictimAnymore;
                if (multiHitModifier?.MaxAttackCount <= attackCount)
                    return MultiHitResult.MaxAttackReached;
            }
            else if (additionalHitAbility.StopMultiHitIfFailed)
                return MultiHitResult.StopBecauseAdditionHitFailed; // stop once an additional fails
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
                    return MultiHitResult.NotFightingVictimAnymore;
                if (multiHitModifier?.MaxAttackCount <= attackCount)
                    return MultiHitResult.MaxAttackReached;
            }
            else if (additionalWieldAbility.StopMultiHitIfFailed)
                return MultiHitResult.StopBecauseAdditionHitFailed; // stop once an additional fails
        }

        return MultiHitResult.Ok;
    }

    private void UseCombatSkill()
    {
        var number = RandomManager.Range(0, 9);
        switch (number)
        {
            case 0:
                if (OffensiveFlags.IsSet("Bash"))
                    UseSkill("Bash", Parser.NoParameters);
                break;
            case 1:
                if (OffensiveFlags.IsSet("Berserk") && !CharacterFlags.IsSet("Berserk"))
                    UseSkill("Berserk", Parser.NoParameters);
                break;
            case 2:
                if (OffensiveFlags.IsSet("Disarm")
                    || ActFlags.HasAny("Warrior", "Thief")) // TODO: check if weapon skill is not hand to hand
                    UseSkill("Disarm", Parser.NoParameters);
                break;
            case 3:
                if (OffensiveFlags.IsSet("Kick"))
                    UseSkill("Kick", Parser.NoParameters);
                break;
            case 4:
                if (OffensiveFlags.IsSet("DirtKick"))
                    UseSkill("Dirt Kicking", Parser.NoParameters);
                break;
            case 5:
                if (OffensiveFlags.IsSet("Tail"))
                    UseSkill("Tail", Parser.NoParameters);
                break;
            case 6:
                if (OffensiveFlags.IsSet("Trip"))
                    UseSkill("Trip", Parser.NoParameters);
                break;
            case 7:
                if (OffensiveFlags.IsSet("Crush"))
                    UseSkill("Crush", Parser.NoParameters);
                break;
            case 8:
                if (OffensiveFlags.IsSet("Backstab"))
                    UseSkill("Backstab", Parser.NoParameters); // TODO: this will never works because we cannot backstab while in combat -> replace with circle
                break;
            case 9:
                if (OffensiveFlags.IsSet("Bite"))
                    UseSkill("Bite", Parser.NoParameters);
                break;
        }
    }

    //
    private string DebuggerDisplay => $"PC {Name} BId:{Blueprint?.Id} INC:{IncarnatedBy?.Name} R:{Room?.Blueprint?.Id} F:{Fighting?.Name}";
}
