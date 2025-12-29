using Mud.Domain;

namespace Mud.Server.Interfaces.Ability;

public interface IAbilityResourceCost
{
    public ResourceKinds ResourceKind { get; }
    public int CostAmount { get; }
    public CostAmountOperators CostAmountOperator { get; }
}
