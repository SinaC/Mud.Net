using Mud.Server.Flags.Interfaces;

namespace Mud.Server.Flags;

public class OffensiveFlags : DataStructures.Flags.Flags, IOffensiveFlags
{
    public OffensiveFlags(params string[] flags)
        : base(flags)
    {
    }
}
