using Microsoft.Extensions.Logging;
using Mud.Common;
using Mud.Domain;
using Mud.Server.Ability;
using Mud.Server.Ability.AbilityGroup;
using Mud.Server.Common;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.AbilityGroup;
using Mud.Server.Interfaces.Class;
using System.Reflection;

namespace Mud.Server.Class;

public abstract class ClassBase : IClass
{
    private readonly List<IAbilityUsage> _availableAbilities;
    private readonly List<IAbilityGroupUsage> _availableAbilityGroups;
    private readonly List<IAbilityGroupUsage> _basicAbilityGroups;
    private readonly List<IAbilityGroupUsage> _defaultAbilityGroups;

    protected ILogger<ClassBase> Logger { get; }
    protected IAbilityManager AbilityManager { get; }
    protected IAbilityGroupManager AbilityGroupManager { get; }

    #region IClass

    public abstract string Name { get; }

    public string DisplayName => Name.UpperFirstLetter();

    public abstract string ShortName { get; }

    public string? Help { get; }

    public abstract IEnumerable<ResourceKinds> ResourceKinds { get; }

    public abstract IEnumerable<ResourceKinds> CurrentResourceKinds(Forms form);

    public abstract BasicAttributes PrimeAttribute { get; }

    public IEnumerable<IAbilityUsage> AvailableAbilities => _availableAbilities;
    public IEnumerable<IAbilityGroupUsage> AvailableAbilityGroups => _availableAbilityGroups;
    public IEnumerable<IAbilityGroupUsage> BasicAbilityGroups => _basicAbilityGroups;
    public IEnumerable<IAbilityGroupUsage> DefaultAbilityGroups => _defaultAbilityGroups;

    public abstract int MaxPracticePercentage { get; }

    public abstract (int thac0_00, int thac0_32) Thac0 { get; }

    public abstract int MinHitPointGainPerLevel { get; }

    public abstract int MaxHitPointGainPerLevel { get; }

    #endregion

    protected ClassBase(ILogger<ClassBase> logger, IAbilityManager abilityManager, IAbilityGroupManager abilityGroupManager)
    {
        Logger = logger;
        AbilityManager = abilityManager;
        AbilityGroupManager = abilityGroupManager;

        _availableAbilities = [];
        _availableAbilityGroups = [];
        _basicAbilityGroups = [];
        _defaultAbilityGroups = [];

        var helpAttribute = GetType().GetCustomAttribute<HelpAttribute>();
        Help = helpAttribute?.Help;
    }

    protected void AddAbility(int level, string abilityName, ResourceKinds? resourceKind, int costAmount, CostAmountOperators costAmountOperator, int rating, int baseLearned = 0)
    {
        var abilityDefinition = AbilityManager[abilityName];
        if (abilityDefinition == null)
        {
            Logger.LogError("Trying to add unknown ability [{abilityName}] to class [{name}]", abilityName, Name);
            return;
        }
        // TODO: check level >= 1, amount >= 0, rating >= 0, baseLearned >= 1
        //
        _availableAbilities.Add(new AbilityUsage(abilityName, level, resourceKind, costAmount, costAmountOperator, rating, baseLearned, abilityDefinition));
    }

    protected void AddSpell(int level, string abilityName, ResourceKinds? resourceKind, int costAmount, CostAmountOperators costAmountOperator, int rating, int baseLearned = 0)
        => AddAbility(level, abilityName, resourceKind, costAmount, costAmountOperator, rating, baseLearned);

    protected void AddSkill(int level, string abilityName, int rating, int baseLearned = 0)
        => AddAbility(level, abilityName, null, 0, CostAmountOperators.None, rating, baseLearned);

    protected void AddPassive(int level, string abilityName, int rating, int baseLearned = 0)
        => AddAbility(level, abilityName, null, 0, CostAmountOperators.None, rating, baseLearned);

    protected void AddAbilityGroup(string abilityGroupName, int cost)
        => AddGroup(abilityGroupName, cost, false);

    protected void AddBasicAbilityGroup(string abilityGroupName)
    {
        if (_basicAbilityGroups.Any(x => StringCompareHelpers.StringEquals(x.Name, abilityGroupName)))
        {
            Logger.LogWarning("Trying to add ability group [{abilityGroupName}] in basic ability group but it has already been added", abilityGroupName);
            return;
        }
        // let create the ability group and add it to available ability groups
        var abilityGroupUsage = AddGroup(abilityGroupName, 0, true);
        if (abilityGroupUsage != null)
        {
            // add this group to default groups
            _basicAbilityGroups.Add(abilityGroupUsage);
        }
    }

    protected void AddDefaultAbilityGroup(string abilityGroupName, int cost)
    {
        if (_defaultAbilityGroups.Any(x => StringCompareHelpers.StringEquals(x.Name, abilityGroupName)))
        {
            Logger.LogWarning("Trying to add ability group [{abilityGroupName}] in default ability group but it has already been added", abilityGroupName);
            return;
        }
        // let create the ability group and add it to available ability groups
        var abilityGroupUsage = AddGroup(abilityGroupName, cost, false);
        if (abilityGroupUsage != null)
        {
            // add this group to default groups
            _defaultAbilityGroups.Add(abilityGroupUsage);
        }
    }

    private IAbilityGroupUsage? AddGroup(string abilityGroupName, int cost, bool isBasics)
    {
        // check if it exists
        var abilityGroupDefinition = AbilityGroupManager[abilityGroupName];
        if (abilityGroupDefinition == null)
        {
            Logger.LogError("Trying to add unknown ability group [{abilityGroupName}] to class [{name}]", abilityGroupName, Name);
            return null;
        }
        // check if already been added
        var abilityGroupUsage = _availableAbilityGroups.SingleOrDefault(x => StringCompareHelpers.StringEquals(x.Name, abilityGroupName));
        if (abilityGroupUsage != null)
        {
            Logger.LogWarning("Trying to add ability group [{abilityGroupName}] in available ability group but it has already been added", abilityGroupName);
            return abilityGroupUsage;
        }
        //
        abilityGroupUsage = new AbilityGroupUsage(abilityGroupName, cost, isBasics, abilityGroupDefinition);
        _availableAbilityGroups.Add(abilityGroupUsage);
        return abilityGroupUsage;
    }
}
