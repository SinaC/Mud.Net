using Mud.Flags.Interfaces;

namespace Mud.Flags;

public class AutoFlags : DataStructures.Flags.Flags, IAutoFlags
{
    public AutoFlags(params string[] flags)
        : base(flags)
    {
    }
}
