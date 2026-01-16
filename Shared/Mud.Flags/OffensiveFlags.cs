using Mud.Flags.Interfaces;

namespace Mud.Flags;

public class OffensiveFlags : DataStructures.Flags.Flags, IOffensiveFlags
{
    public OffensiveFlags(params string[] flags)
        : base(flags)
    {
    }
}
