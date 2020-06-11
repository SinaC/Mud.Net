using Mud.Domain;

namespace Mud.Server.Interfaces.Ability
{
    public interface IAbilityUsage
    {
        IAbility Ability { get; set; }

        int Level { get; set; }

        ResourceKinds? ResourceKind { get; set; }

        int CostAmount { get; set; }

        CostAmountOperators CostAmountOperator { get; set; }

        int Rating { get; set; }
    }
}
