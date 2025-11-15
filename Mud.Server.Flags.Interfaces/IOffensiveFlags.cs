using Mud.DataStructures.Flags;

namespace Mud.Server.Flags.Interfaces;

public interface IOffensiveFlags : IFlags<string, IOffensiveFlagValues>
{
}

public interface IOffensiveFlagValues : IFlagValues<string>
{
}
