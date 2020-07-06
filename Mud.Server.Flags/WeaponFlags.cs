using Mud.DataStructures.Flags;
using Mud.Server.Flags.Interfaces;
using System.Text;

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

        public virtual StringBuilder Append(StringBuilder sb, bool shortDisplay)
        {
            foreach (string flag in Items)
                sb.AppendFormat("{0}", FlagValues.PrettyPrint(flag, shortDisplay));
            return sb;
        }
    }
}
