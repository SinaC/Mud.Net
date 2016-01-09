using Mud.Server.Constants;

namespace Mud.Server.Abilities
{
    // Single target + fixed of energy
    public abstract class SingleTargetEnergyBase : SingleTargetInRoomAbilityBase
    {
        public override ResourceKinds ResourceKind
        {
            get { return ResourceKinds.Energy; }
        }

        public override AmountOperators CostType
        {
            get { return AmountOperators.Fixed; }
        }
    }
}
