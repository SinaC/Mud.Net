using Mud.Domain;

namespace Mud.Server.Ability.Interfaces;

public interface IAbilityResourceCost
{
    public ResourceKinds ResourceKind { get; }
    public int CostAmount { get; }
    public CostAmountOperators CostAmountOperator { get; }
}
