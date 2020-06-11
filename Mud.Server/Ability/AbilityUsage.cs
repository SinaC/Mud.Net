using Mud.Domain;
using Mud.Server.Interfaces.Ability;

namespace Mud.Server.Ability
{
    public class AbilityUsage : IAbilityUsage
    {
        public IAbility Ability { get; set; }

        public int Level { get; set; }

        public ResourceKinds? ResourceKind { get; set; }

        public int CostAmount { get; set; }

        public CostAmountOperators CostAmountOperator { get; set; }

        public int Rating { get; set; }

        public AbilityUsage()
        {
            ResourceKind = null;
            CostAmount = 0;
            CostAmountOperator = CostAmountOperators.None;
            Rating = 1;
        }
    }
}
