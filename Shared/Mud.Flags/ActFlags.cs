using Mud.Flags.Interfaces;

namespace Mud.Flags;

public class ActFlags : DataStructures.Flags.Flags, IActFlags
{
    public ActFlags(params string[] flags)
        : base(flags)
    {
    }
}
