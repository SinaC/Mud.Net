using Mud.DataStructures.Flags;

namespace Mud.Server.Flags.Interfaces;

public interface IIRVFlags : IFlags<string, IIRVFlagValues>
{
}

public interface IIRVFlagValues : IFlagValues<string>
{
}
