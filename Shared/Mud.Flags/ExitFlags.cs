using Mud.Flags.Interfaces;

namespace Mud.Flags;

public class ExitFlags : DataStructures.Flags.Flags, IExitFlags
{
    public ExitFlags(params string[] flags)
        : base(flags)
    {
    }
}
