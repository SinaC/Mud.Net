using Mud.Flags.Interfaces;

namespace Mud.Flags;

public class PortalFlags : DataStructures.Flags.Flags, IPortalFlags
{
    public PortalFlags(params string[] flags)
        : base(flags)
    {
    }
}
