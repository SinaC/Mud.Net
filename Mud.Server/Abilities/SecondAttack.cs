using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mud.Server.Constants;

namespace Mud.Server.Abilities
{
    // TODO: effect
    public class SecondAttack : IAbility
    {
        public ResourceKinds ResourceKind
        {
            get { return ResourceKinds.None; }
        }

        public AmountOperators CostType
        {
            get { return AmountOperators.Fixed; }
        }

        public int CostAmount
        {
            get { return 0; }
        }

        public int GlobalCooldown
        {
            get { return 0; }
        }

        public int Cooldown
        {
            get { return 0; }
        }

        public SchoolTypes School
        {
            get { return SchoolTypes.Physical; }
        }

        public AbilityFlags Flags
        {
            get { return AbilityFlags.RequireMainHand | AbilityFlags.Passive; }
        }
    }
}
