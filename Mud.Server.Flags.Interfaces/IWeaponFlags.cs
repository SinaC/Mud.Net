using Mud.DataStructures.Flags;
using System.Text;

namespace Mud.Server.Flags.Interfaces;

public interface IWeaponFlags : IFlags<string, IWeaponFlagValues>
{
    StringBuilder Append(StringBuilder sb, bool shortDisplay);
}

public interface IWeaponFlagValues : IFlagValues<string>
{
    string PrettyPrint(string flag, bool shortDisplay);
}
