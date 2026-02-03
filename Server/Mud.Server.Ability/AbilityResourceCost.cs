using Mud.Domain;
using Mud.Server.Ability.Interfaces;

namespace Mud.Server.Ability;

public class AbilityResourceCost : IAbilityResourceCost
{
    public ResourceKinds ResourceKind { get; }
    public int CostAmount { get; }
    public CostAmountOperators CostAmountOperator { get; }

    public AbilityResourceCost(ResourceKinds resourceKind, int costAmount, CostAmountOperators costAmountOperator)
    {
        ResourceKind = resourceKind;
        CostAmount = costAmount;
        CostAmountOperator = costAmountOperator;
    }
}
