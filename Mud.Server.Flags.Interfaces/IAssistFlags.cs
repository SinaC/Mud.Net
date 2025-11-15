using Mud.DataStructures.Flags;

namespace Mud.Server.Flags.Interfaces;

public interface IAssistFlags : IFlags<string, IAssistFlagValues>
{
}

public interface IAssistFlagValues : IFlagValues<string>
{
}
