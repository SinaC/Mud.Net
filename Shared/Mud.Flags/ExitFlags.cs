using Mud.Flags.Interfaces;

namespace Mud.Flags;

public class ExitFlags(params string[] flags) : DataStructures.Flags.Flags(flags), IExitFlags
{
}
