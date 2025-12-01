using Mud.DataStructures.Flags;
using System.Text;

namespace Mud.Server.Flags.Interfaces;

public interface IShieldFlags : IFlags<string, IShieldFlagValues>
{
    StringBuilder Append(StringBuilder sb, bool shortDisplay);
}

public interface IShieldFlagValues : IFlagValues<string>
{
    string PrettyPrint(string flag, bool shortDisplay);
}
