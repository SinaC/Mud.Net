using Mud.Domain;

namespace Mud.Server.Abilities
{
    public class AbilityUsage
    {
        public IAbility Ability { get; set; }

        public int Level { get; set; }

        public ResourceKinds ResourceKind { get; set; }

        public int CostAmount { get; set; }

        public CostAmountOperators CostAmountOperator { get; set; }

        public int DifficulityMultiplier { get; set; }

        public AbilityUsage()
        {
            ResourceKind = ResourceKinds.None;
            CostAmount = 0;
            CostAmountOperator = CostAmountOperators.None;
            DifficulityMultiplier = 1;
        }
    }
}
