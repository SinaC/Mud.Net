using Mud.POC.Abilities2.Domain;

namespace Mud.POC.Abilities2
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
