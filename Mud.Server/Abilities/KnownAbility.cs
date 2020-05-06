using Mud.Domain;

namespace Mud.Server.Abilities
{
    public class KnownAbility
    {
        public IAbility Ability { get; set; }

        public ResourceKinds ResourceKind { get; set; }

        public int CostAmount { get; set; }

        public CostAmountOperators CostAmountOperator { get; set; }

        public int Level { get; set; } // level at which ability can be learned
        
        public int Learned { get; set; } // practice percentage, 0 means not learned, 100 mean fully learned

        public int DifficulityMultiplier { get; set; } // how difficult is it to improve/gain/practice
    }
}
