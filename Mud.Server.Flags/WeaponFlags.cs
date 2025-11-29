using Mud.Common.Attributes;
using Mud.DataStructures.Flags;
using Mud.Server.Flags.Interfaces;
using System.Text;

namespace Mud.Server.Flags;


[Export(typeof(IWeaponFlags))]
public class WeaponFlags : Flags<IWeaponFlagValues>, IWeaponFlags
{
    public WeaponFlags(IWeaponFlagValues flagValues)
        : base(flagValues)
    {
    }

    public virtual StringBuilder Append(StringBuilder sb, bool shortDisplay)
    {
        foreach (var flag in Values)
            sb.AppendFormat("{0}", FlagValues.PrettyPrint(flag, shortDisplay));
        return sb;
    }
}
