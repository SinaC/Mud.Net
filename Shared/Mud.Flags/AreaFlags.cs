using Mud.Flags.Interfaces;

namespace Mud.Flags;

public class AreaFlags : DataStructures.Flags.Flags, IAreaFlags
{
    public AreaFlags(params string[] flags)
        : base(flags)
    {
    }
}
