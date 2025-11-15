using Mud.DataStructures.Flags;

namespace Mud.Server.Flags.Interfaces;

public interface IActFlags : IFlags<string, IActFlagValues>
{
}

public interface IActFlagValues : IFlagValues<string>
{
}
