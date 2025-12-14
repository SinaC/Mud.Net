using Mud.Server.Flags.Interfaces;

namespace Mud.Server.Flags;

public class ActFlags : DataStructures.Flags.Flags, IActFlags
{
    public ActFlags(params string[] flags)
        : base(flags)
    {
    }
}
