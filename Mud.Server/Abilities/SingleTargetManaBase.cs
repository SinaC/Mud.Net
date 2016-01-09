using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mud.Server.Constants;

namespace Mud.Server.Abilities
{
    // Single target + percentage of mana
    public abstract class SingleTargetManaBase : SingleTargetInRoomAbilityBase
    {
        public override ResourceKinds ResourceKind
        {
            get { return ResourceKinds.Mana; }
        }

        public override AmountOperators CostType
        {
            get { return AmountOperators.Percentage; }
        }

   }
}
