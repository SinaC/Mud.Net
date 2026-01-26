using Mud.Flags.Interfaces;

namespace Mud.Flags;

public class ContainerFlags : DataStructures.Flags.Flags, IContainerFlags
{
    public ContainerFlags(params string[] flags)
        : base(flags)
    {
    }
}
