using System.Collections.Generic;
using Mud.Common;
using Mud.Domain;
using Mud.Logger;
using Mud.Server.Ability;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Class;

namespace Mud.Server.Class;

public abstract class ClassBase : IClass
{
    private readonly List<AbilityUsage> _abilities;

    protected IAbilityManager AbilityManager { get; }

    #region IClass

    public abstract string Name { get; }

    public string DisplayName => Name.UpperFirstLetter();

    public abstract string ShortName { get; }

    public abstract IEnumerable<ResourceKinds> ResourceKinds { get; }

    public abstract IEnumerable<ResourceKinds> CurrentResourceKinds(Forms form);

    public abstract BasicAttributes PrimeAttribute { get; }

    public IEnumerable<IAbilityUsage> Abilities => _abilities;

    public abstract int MaxPracticePercentage { get; }

    public abstract (int thac0_00, int thac0_32) Thac0 { get; }

    public abstract int MinHitPointGainPerLevel { get; }

    public abstract int MaxHitPointGainPerLevel { get; }

    #endregion

    protected ClassBase(IAbilityManager abilityManager)
    {
        AbilityManager = abilityManager;

        _abilities = [];
    }

    protected void AddAbility(int level, string abilityName, ResourceKinds? resourceKind, int costAmount, CostAmountOperators costAmountOperator, int rating)
    {
        var abilityInfo = AbilityManager[abilityName];
        if (abilityInfo == null)
        {
            Log.Default.WriteLine(LogLevels.Error, "Trying to add unknown ability [{0}] to class [{1}]", abilityName, Name);
            return;
        }
        //
        _abilities.Add(new AbilityUsage(abilityName, level, resourceKind, costAmount, costAmountOperator, rating, abilityInfo));
    }
}
