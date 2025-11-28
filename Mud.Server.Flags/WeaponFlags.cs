using Mud.DataStructures.Flags;
using Mud.Server.Flags.Interfaces;
using System.Text;

namespace Mud.Server.Flags
{
    public class WeaponFlags : Flags<IWeaponFlagValues>, IWeaponFlags
    {
        public WeaponFlags(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
        }

        public WeaponFlags(IServiceProvider serviceProvider, string flags)
            : base(serviceProvider, flags)
        {
        }

        public WeaponFlags(IServiceProvider serviceProvider, params string[] flags)
            : base(serviceProvider, flags)
        {
        }

        public virtual StringBuilder Append(StringBuilder sb, bool shortDisplay)
        {
            foreach (string flag in Values)
                sb.AppendFormat("{0}", FlagValues.PrettyPrint(flag, shortDisplay));
            return sb;
        }
    }
}
