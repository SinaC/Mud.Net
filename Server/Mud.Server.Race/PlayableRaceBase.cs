using Microsoft.Extensions.Logging;
using Mud.Common;
using Mud.Domain;
using Mud.Server.Ability;
using Mud.Server.Ability.Interfaces;
using Mud.Server.Class.Interfaces;
using Mud.Server.Domain;
using Mud.Server.Domain.Attributes;
using Mud.Server.Race.Interfaces;
using System.Reflection;

namespace Mud.Server.Race;

public abstract class PlayableRaceBase : RaceBase, IPlayableRace
{
    private readonly List<IAbilityUsage> _abilities;

    private readonly List<EquipmentSlots> _basicSlots =
    [
        Mud.Domain.EquipmentSlots.Light,
        Mud.Domain.EquipmentSlots.Head,
        Mud.Domain.EquipmentSlots.Amulet, // 2 amulets
        Mud.Domain.EquipmentSlots.Amulet,
        Mud.Domain.EquipmentSlots.Chest,
        Mud.Domain.EquipmentSlots.Cloak,
        Mud.Domain.EquipmentSlots.Waist,
        Mud.Domain.EquipmentSlots.Wrists, // 2 wrists
        Mud.Domain.EquipmentSlots.Wrists,
        Mud.Domain.EquipmentSlots.Arms,
        Mud.Domain.EquipmentSlots.Hands,
        Mud.Domain.EquipmentSlots.Ring, // 2 rings
        Mud.Domain.EquipmentSlots.Ring,
        Mud.Domain.EquipmentSlots.Legs,
        Mud.Domain.EquipmentSlots.Feet,
        Mud.Domain.EquipmentSlots.MainHand, // 2 hands
        Mud.Domain.EquipmentSlots.OffHand,
        Mud.Domain.EquipmentSlots.Float,
    ];

    protected ILogger<PlayableRaceBase> Logger { get; }
    protected IAbilityManager AbilityManager { get; }

    #region IPlayableRace

    public abstract string ShortName { get; }

    public string? Help { get; }

    public IEnumerable<IAbilityUsage> Abilities => _abilities;

    public override IEnumerable<EquipmentSlots> EquipmentSlots => _basicSlots;

    public abstract bool SelectableDuringCreation { get; }

    public abstract int CreationPointsStartValue { get; }

    public virtual bool EnhancedPrimeAttribute => false;

    public abstract int GetStartAttribute(BasicAttributes attribute);

    public abstract int GetMaxAttribute(BasicAttributes attribute);

    public virtual int ClassExperiencePercentageMultiplier(IClass? c) => 100;

    #endregion

    protected PlayableRaceBase(ILogger<PlayableRaceBase> logger, IAbilityManager abilityManager)
    {
        Logger = logger;
        AbilityManager = abilityManager;

        _abilities = [];
        var helpAttribute = GetType().GetCustomAttribute<HelpAttribute>();
        Help = helpAttribute?.Help;
    }

    protected void AddNaturalAbility(int level, string abilityName, IEnumerable<(ResourceKinds resourceKind, int costAmount, CostAmountOperators costAmountOperator)> costs, int rating)
    {
        var abilityDefinition = AbilityManager[abilityName];
        if (abilityDefinition == null)
        {
            Logger.LogError("Trying to add unknown ability [{abilityName}] to race [{name}]", abilityName, Name);
            return;
        }
        if (_abilities.Any(x => StringCompareHelpers.StringEquals(x.Name, abilityName)))
        {
            Logger.LogError("Trying to add ability [{abilityName}] to race [{name}] more than once", abilityName, Name);
            return;
        }
        // TODO: check level >= 1, amount >= 0, rating >= 0, baseLearned >= 1
        var abilityCosts = costs.Select(x => new AbilityResourceCost(x.resourceKind, x.costAmount, x.costAmountOperator)).ToList();
        _abilities.Add(new AbilityUsage(abilityName, level, abilityCosts, rating, 100, abilityDefinition));
    }

    protected void AddNaturalAbility(string abilityName)
    {
        AddNaturalAbility(1, abilityName, [], 1);
    }

    protected void AddNaturalAbility(string abilityName, int rating)
    {
        AddNaturalAbility(1, abilityName, [], rating);
    }

    protected void AddNaturalAbility(int level, string abilityName, ResourceKinds resourceKind, int costAmount, CostAmountOperators costAmountOperator, int rating)
        => AddNaturalAbility(level, abilityName, [(resourceKind, costAmount, costAmountOperator)], rating);
}
