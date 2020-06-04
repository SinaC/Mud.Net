using Mud.POC.Abilities2.Domain;

namespace Mud.POC.Abilities2
{
    public class AbilityUsage
    {
        public string Name { get; set; }

        public int Level { get; set; }

        public ResourceKinds? ResourceKind { get; set; }

        public int CostAmount { get; set; }

        public CostAmountOperators CostAmountOperator { get; set; }

        public int Rating { get; set; }
    }
}
