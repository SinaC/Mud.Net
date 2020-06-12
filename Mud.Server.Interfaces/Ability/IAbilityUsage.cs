using Mud.Domain;

namespace Mud.Server.Interfaces.Ability
{
    public interface IAbilityUsage
    {
        string Name { get; }

        int Level { get; }

        ResourceKinds? ResourceKind { get; }

        int CostAmount { get; }

        CostAmountOperators CostAmountOperator { get; }

        int Rating { get; }
    }
}
