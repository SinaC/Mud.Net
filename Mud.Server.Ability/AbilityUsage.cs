using Mud.Domain;
using Mud.Server.Interfaces.Ability;

namespace Mud.Server.Ability;

public class AbilityUsage : IAbilityUsage
{
    public string Name { get; }

    public int Level { get; }

    public ResourceKinds? ResourceKind { get; }

    public int CostAmount { get; }

    public CostAmountOperators CostAmountOperator { get; }

    public int Rating { get; }

    public int MinLearned { get; }

    public IAbilityDefinition AbilityDefinition { get; }

    public AbilityUsage(string name, int level, ResourceKinds? resourceKind, int costAmount, CostAmountOperators costAmountOperator, int rating, int minLearned, IAbilityDefinition abilityDefinition)
    {
        Name = name;
        Level = level;
        ResourceKind = resourceKind;
        CostAmount = costAmount;
        CostAmountOperator = costAmountOperator;
        Rating = rating;
        MinLearned = minLearned;
        AbilityDefinition = abilityDefinition;
    }
}
