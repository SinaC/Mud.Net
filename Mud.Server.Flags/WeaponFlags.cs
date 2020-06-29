using Mud.DataStructures.Flags;
using Mud.Server.Flags.Interfaces;

namespace Mud.Server.Flags
{
    public class WeaponFlags : Flags<IWeaponFlagValues>, IWeaponFlags
    {
        public WeaponFlags()
            : base()
        {
        }

        public WeaponFlags(string flags)
            : base(flags)
        {
        }

        public WeaponFlags(params string[] flags)
            : base(flags)
        {
        }
    }
}
