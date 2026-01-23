using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mud.Blueprints;
using Mud.Blueprints.Item;
using Mud.Blueprints.Item.Affects;
using Mud.Common;
using Mud.Domain;
using Mud.Domain.SerializationData.Avatar;
using Mud.Flags;
using Mud.Flags.Interfaces;
using Mud.Random;
using Mud.Server.Affects.Character;
using Mud.Server.Common;
using Mud.Server.Common.Helpers;
using Mud.Server.Domain;
using Mud.Server.Entity;
using Mud.Server.Interfaces.Affect;
using Mud.Server.Interfaces.Affect.Item;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Entity;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Item;
using Mud.Server.Interfaces.Room;
using Mud.Server.Options;
using System.Diagnostics;
using System.Text;

namespace Mud.Server.Item;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
public abstract class ItemBase: EntityBase, IItem
{
    protected int MaxLevel { get; }
    protected IRandomManager RandomManager { get; }
    protected IAuraManager AuraManager { get; }

    protected ItemBase(ILogger<ItemBase> logger, IGameActionManager gameActionManager, ICommandParser commandParser, IOptions<MessageForwardOptions> messageForwardOptions, IOptions<WorldOptions> worldOptions, IRandomManager randomManager, IAuraManager auraManager)
        : base(logger, gameActionManager, commandParser, messageForwardOptions)
    { 
        MaxLevel = worldOptions.Value.MaxLevel;
        RandomManager = randomManager;
        AuraManager = auraManager;

        ItemFlags = new ItemFlags();
    }

    protected void Initialize<TBlueprint>(Guid guid, TBlueprint blueprint, string name, string shortDescription, string description, string source, IContainer containedInto)
        where TBlueprint : ItemBlueprintBase
    {
        Initialize(guid, name, description);

        Blueprint = blueprint;
        Source = source;
        ShortDescription = shortDescription;
        containedInto.PutInContainer(this); // put in container
        ContainedInto = containedInto; // set above container as our container
        WearLocation = blueprint.WearLocation;
        Level = blueprint.Level;
        Cost = blueprint.Cost;
        Weight = blueprint.Weight;
        NoTake = blueprint.NoTake;

        if (blueprint.ItemFlags != null && blueprint.ItemFlags.IsSet("RandomStats"))
        {
            Cost = RandomManager.Range(blueprint.Cost * 80 / 100, blueprint.Cost * 120 / 100);
            if (Level < MaxLevel)
                Level = Math.Clamp(RandomManager.Range(blueprint.Level - 5, blueprint.Level + 5), 1, MaxLevel);
        }

        // Affects
        if (blueprint.ItemAffects != null && blueprint.ItemAffects.Length > 0)
        {
            var affects = blueprint.ItemAffects.Select(GenerateAffectFromItemAffect).Where(x => x != null).ToArray();
            if (affects.Length > 0)
                AuraManager.AddAura(this, this, blueprint.Level, AuraFlags.Permanent | AuraFlags.NoDispel | AuraFlags.Inherent, false, affects);
        }

        BaseItemFlags = NewAndCopyAndSet(() => new ItemFlags(), blueprint.ItemFlags, null);
    }

    public void Initialize<TBlueprint>(Guid guid, TBlueprint blueprint, string source, IContainer containedInto)
        where TBlueprint : ItemBlueprintBase
    {
        Initialize(guid, blueprint, blueprint.Name, blueprint.ShortDescription, blueprint.Description, source, containedInto);
        ResetAttributesAndResourcesAndFlags();
    }

    public void Initialize<TBlueprint, TData>(Guid guid, TBlueprint blueprint, TData data, string name, string shortDescription, string description, IContainer containedInto)
        where TBlueprint : ItemBlueprintBase
        where TData : ItemData
    {
        Initialize(guid, blueprint, name, shortDescription, description, data.Source, containedInto);

        Level = data.Level;
        Cost = data.Cost;
        DecayPulseLeft = data.DecayPulseLeft;
        BaseItemFlags = NewAndCopyAndSet(() => new ItemFlags(), new ItemFlags(data.ItemFlags), null);
        // Auras
        if (data.Auras != null && data.Auras.Length > 0)
        {
            RemoveAuras(x => true, false, false); // remove auras added from blueprint
            foreach (var auraData in data.Auras)
                AuraManager.AddAura(this, auraData, false);
        }
        ResetAttributesAndResourcesAndFlags();
    }

    public void Initialize<TBlueprint, TData>(Guid guid, TBlueprint blueprint, TData data, IContainer containedInto)
        where TBlueprint : ItemBlueprintBase
        where TData : ItemData
    {
        Initialize(guid, blueprint, data, blueprint.Name, data.ShortDescription ?? blueprint.ShortDescription, data.Description ?? blueprint.ShortDescription, containedInto);
    }

    #region IItem

    #region IEntity

    public override string DisplayName => ShortDescription ?? Blueprint.ShortDescription ?? Name.UpperFirstLetter();

    public override string DebugName => Blueprint == null ? DisplayName : $"{DisplayName}[{Blueprint.Id}]";


    // Recompute

    public override void Recompute()
    {
        Logger.LogDebug("ItemBase.Recompute: {name}", DebugName);

        // 0) Reset
        ResetAttributesAndResourcesAndFlags();

        // 1) Apply auras from room containing item if in a room
        if (ContainedInto is IRoom room && room.IsValid)
        {
            ApplyAuras<IItem>(room, this);
        }

        // 2) Apply auras from character equiping item if equipped by a character
        if (EquippedBy != null && EquippedBy.IsValid)
        {
            ApplyAuras<IItem>(EquippedBy, this);
        }

        // 3) Apply own auras
        ApplyAuras<IItem>(this, this);
    }

    //
    public override void OnAuraRemoved(IAura aura, bool displayWearOffMessage)
    {
        base.OnAuraRemoved(aura, displayWearOffMessage);

        if (displayWearOffMessage)
        {
            if (aura.AbilityDefinition != null && aura.AbilityDefinition.HasItemWearOffMessage)
            {
                var holder = ContainedInto as ICharacter ?? EquippedBy;
                holder?.Act(ActOptions.ToCharacter, aura.AbilityDefinition.ItemWearOffMessage!, this);
            }
        }
    }

    //
    public override string RelativeDisplayName(ICharacter beholder, bool capitalizeFirstLetter = false)
    {
        StringBuilder displayName = new();
        var playableBeholder = beholder as IPlayableCharacter;
        if (playableBeholder != null && IsQuestObjective(playableBeholder, false))
            displayName.Append(StringHelpers.QuestPrefix);
        if (beholder.CanSee(this))
            displayName.Append(DisplayName);
        else if (capitalizeFirstLetter)
            displayName.Append("Something");
        else
            displayName.Append("something");
        if (beholder.ImmortalMode.HasFlag(ImmortalModeFlags.Holylight))
            displayName.Append($" [id: {Blueprint?.Id.ToString() ?? " ??? "}]");
        return displayName.ToString();
    }

    public override string RelativeDescription(ICharacter beholder) // Add (Quest) to description if beholder is on a quest with 'this' as objective
    {
        StringBuilder description = new();
        if (beholder is IPlayableCharacter playableBeholder && IsQuestObjective(playableBeholder, false))
            description.Append(StringHelpers.QuestPrefix);
        description.Append(Description);
        return description.ToString();
    }

    public virtual void OnRemoved(IRoom nullRoom) // called before removing an item from the game
    {
        base.OnRemoved();

        ContainedInto?.GetFromContainer(this);
        ContainedInto = nullRoom; // this will avoid a lot of problem, will be set to null in Cleanup phase
    }

    public override void OnCleaned() // called when removing definitively an entity from the game
    {
        Blueprint = null!;
        ContainedInto = null!;
    }

    #endregion

    public IContainer ContainedInto { get; private set; } = null!;

    public ItemBlueprintBase Blueprint { get; private set; } = null!;

    public string Source { get; private set; } = null!;

    public string ShortDescription { get; private set; } = null!;

    public IEnumerable<ExtraDescription> ExtraDescriptions => Blueprint.ExtraDescriptions;

    public WearLocations WearLocation { get; private set; }

    public ICharacter? EquippedBy { get; private set; }

    public int DecayPulseLeft { get; protected set; } // 0: means no decay

    public int Level { get; protected set; }

    public int Weight { get; private set; }

    public int Cost { get; private set; }

    public bool NoTake { get; protected set; }

    public virtual int TotalWeight => Weight;

    public virtual int CarryCount => 1;

    public IItemFlags BaseItemFlags { get; protected set; } = null!;

    public IItemFlags ItemFlags { get; protected set; } = null!;

    public virtual bool IsQuestObjective(IPlayableCharacter questingCharacter, bool checkCompleted)
    {
        return false; // by default, an item is not a quest objective
    }

    public virtual bool ChangeContainer(IContainer? container)
    {
        if (container == this)
        {
            Logger.LogError("Trying to put a container in itself!!");
            return false;
        }

        Logger.LogInformation("ChangeContainer: {name} : {containedInto} -> {container}", DebugName, ContainedInto == null ? "<<??>>" : ContainedInto.DebugName, container == null ? "<<??>>" : container.DebugName);

        ContainedInto?.GetFromContainer(this);
        //Debug.Assert(container != null, "ChangeContainer: an item cannot be outside a container"); // False, equipment are not stored in any container
        var putInContainer = container?.PutInContainer(this) ?? true;
        if (!putInContainer)
            return false;
        ContainedInto = container!;

        return true;
    }

    public bool ChangeEquippedBy(ICharacter? character, bool recompute)
    {
        var previousEquippedBy = EquippedBy;
        EquippedBy?.Unequip(this);
        Logger.LogInformation("ChangeEquippedBy: {name} : {equippedBy} -> {character}", DebugName, EquippedBy?.DebugName ?? "<<??>>", character?.DebugName ?? "<<??>>");
        EquippedBy = character;
        character?.Equip(this);
        if (recompute)
            previousEquippedBy?.Recompute();
        if (recompute)
            EquippedBy?.Recompute();
        return true;
    }

    public void DecreaseDecayPulseLeft(int pulseCount)
    {
        if (DecayPulseLeft > 0)
            DecayPulseLeft = Math.Max(0, DecayPulseLeft - pulseCount);
    }

    public void SetTimer(TimeSpan duration)
    {
        DecayPulseLeft = Pulse.FromTimeSpan(duration);
    }

    public void AddBaseItemFlags(bool recompute, params string[] flags)
    {
        BaseItemFlags.Set(flags);
        if (recompute)
            Recompute();
    }

    public void RemoveBaseItemFlags(bool recompute, params string[] flags)
    {
        BaseItemFlags.Unset(flags);
        if (recompute)
            Recompute();
    }

    public void Disenchant()
    {
        RemoveAuras(_ => true, false, true);
        BaseItemFlags = new ItemFlags();
        Recompute();
    }

    public void SetShortDescription(string shortDescription)
    {
        ShortDescription = shortDescription;
    }

    public void IncreaseLevel()
    {
        Level++;
    }

    public void SetLevel(int level)
    {
        Level = level;
    }

    public void SetCost(int cost)
    {
        Cost = cost;
    }

    public virtual StringBuilder Append(StringBuilder sb, ICharacter viewer, bool shortDisplay)
    {
        // Item flags
        if (ItemFlags.IsSet("Invis") && viewer.CharacterFlags.IsSet("DetectInvis")) sb.Append("%y%(Invis)%x%");
        if (ItemFlags.IsSet("Evil") && viewer.CharacterFlags.IsSet("DetectEvil")) sb.Append("%R%(Evil)%x%");
        if (ItemFlags.IsSet("Bless") && viewer.CharacterFlags.IsSet("DetectGood")) sb.Append("%C%(Blessed)%x%");
        if (ItemFlags.IsSet("Magic") && viewer.CharacterFlags.IsSet("DetectMagic")) sb.Append("%b%(Magical)%x%");
        if (ItemFlags.IsSet("Glowing")) sb.Append("%Y%(Glowing)%x%");
        if (ItemFlags.IsSet("Humming")) sb.Append("%y%(Humming)%x%");

        // Description
        if (shortDisplay)
            sb.Append(RelativeDisplayName(viewer));
        else
            sb.Append(RelativeDescription(viewer));
        //
        return sb;
    }

    public void ApplyAffect(IItemFlagsAffect affect)
    {
        switch (affect.Operator)
        {
            case AffectOperators.Add:
            case AffectOperators.Or:
                ItemFlags.Set(affect.Modifier);
                break;
            case AffectOperators.Assign:
                ItemFlags.Copy(affect.Modifier);
                break;
            case AffectOperators.Nor:
                ItemFlags.Unset(affect.Modifier);
                break;
        }
    }

    public virtual ItemData MapItemData()
    {
        return new ItemData
        {
            ItemId = Blueprint.Id,
            Source = Source,
            ShortDescription = ShortDescription,
            Description = Description,
            Level = Level,
            Cost = Cost,
            DecayPulseLeft = DecayPulseLeft,
            ItemFlags = BaseItemFlags.Serialize(), // Current will be recompute with auras
            Auras = MapAuraData(),
        };
    }

    #endregion

    protected override void ResetAttributesAndResourcesAndFlags()
    {
        ItemFlags.Copy(BaseItemFlags);
    }

    protected void ApplyAuras<T>(IEntity source, T target)
        where T : IItem
    {
        if (!source.IsValid)
            return;
        foreach (IAura aura in source.Auras.Where(x => x.IsValid))
        {
            foreach (IItemAffect<T> affect in aura.Affects.OfType<IItemAffect<T>>())
            {
                affect.Apply(target);
            }
        }
    }


    private IAffect? GenerateAffectFromItemAffect(ItemAffectBase itemAffect)
        => itemAffect switch
        {
            ItemAffectImmFlags imm => new CharacterIRVAffect { Location = IRVAffectLocations.Immunities, Operator = AffectOperators.Add, Modifier = imm.IRVFlags },
            ItemAffectResFlags res => new CharacterIRVAffect { Location = IRVAffectLocations.Resistances, Operator = AffectOperators.Add, Modifier = res.IRVFlags },
            ItemAffectVulnFlags vuln => new CharacterIRVAffect { Location = IRVAffectLocations.Vulnerabilities, Operator = AffectOperators.Add, Modifier = vuln.IRVFlags },
            ItemAffectCharacterAttribute attr => new CharacterAttributeAffect { Location = attr.Attribute, Operator = AffectOperators.Add, Modifier = attr.Modifier },
            ItemAffectSex sex => new CharacterSexAffect{ Value = sex.Sex },
            ItemAffectResource resource => new CharacterResourceAffect { Location = resource.Location, Operator = AffectOperators.Add, Modifier = resource.Modifier },
            ItemAffectCharacterFlags ch => new CharacterFlagsAffect { Operator = AffectOperators.Add, Modifier = ch.CharacterFlags },
            ItemAffectShieldFlags sh => new CharacterShieldFlagsAffect { Operator = AffectOperators.Add, Modifier = sh.ShieldFlags },
            _ => null,
        };

    //
    private string DebuggerDisplay => $"I {Name} INC:{IncarnatedBy?.Name} BId:{Blueprint?.Id}";
}
