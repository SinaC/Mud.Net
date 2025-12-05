using Microsoft.Extensions.Logging;
using Mud.Common;
using Mud.Domain;
using Mud.Server.Ability;
using Mud.Server.Common;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Class;
using System.Reflection;

namespace Mud.Server.Class;

public abstract class ClassBase : IClass
{
    private readonly List<AbilityUsage> _abilities;

    protected ILogger<ClassBase> Logger { get; }
    protected IAbilityManager AbilityManager { get; }

    #region IClass

    public abstract string Name { get; }

    public string DisplayName => Name.UpperFirstLetter();

    public abstract string ShortName { get; }

    public string? Help { get; }

    public abstract IEnumerable<ResourceKinds> ResourceKinds { get; }

    public abstract IEnumerable<ResourceKinds> CurrentResourceKinds(Forms form);

    public abstract BasicAttributes PrimeAttribute { get; }

    public IEnumerable<IAbilityUsage> Abilities => _abilities;

    public abstract int MaxPracticePercentage { get; }

    public abstract (int thac0_00, int thac0_32) Thac0 { get; }

    public abstract int MinHitPointGainPerLevel { get; }

    public abstract int MaxHitPointGainPerLevel { get; }

    #endregion

    protected ClassBase(ILogger<ClassBase> logger, IAbilityManager abilityManager)
    {
        Logger = logger;
        AbilityManager = abilityManager;

        _abilities = [];
        var helpAttribute = GetType().GetCustomAttribute<HelpAttribute>();
        Help = helpAttribute?.Help;
    }

    protected void AddAbility(int level, string abilityName, ResourceKinds? resourceKind, int costAmount, CostAmountOperators costAmountOperator, int rating, int baseLearned = 0)
    {
        var abilityInfo = AbilityManager[abilityName];
        if (abilityInfo == null)
        {
            Logger.LogError("Trying to add unknown ability [{abilityName}] to class [{name}]", abilityName, Name);
            return;
        }
        // TODO: check level >= 1, amount >= 0, rating >= 0, baseLearned >= 1
        //
        _abilities.Add(new AbilityUsage(abilityName, level, resourceKind, costAmount, costAmountOperator, rating, baseLearned, abilityInfo));
    }

    protected void AddSpell(int level, string abilityName, ResourceKinds? resourceKind, int costAmount, CostAmountOperators costAmountOperator, int rating, int baseLearned = 0)
        => AddAbility(level, abilityName, resourceKind, costAmount, costAmountOperator, rating, baseLearned);

    protected void AddSkill(int level, string abilityName, int rating, int baseLearned = 0)
        => AddAbility(level, abilityName, null, 0, CostAmountOperators.None, rating, baseLearned);

    protected void AddPassive(int level, string abilityName, int rating, int baseLearned = 0)
        => AddAbility(level, abilityName, null, 0, CostAmountOperators.None, rating, baseLearned);
}
