using Mud.Server.Flags.Interfaces;

namespace Mud.Server.Flags;

public class ShieldFlags : DataStructures.Flags.Flags, IShieldFlags
{
    public ShieldFlags(params string[] flags)
        : base(flags)
    {
    }
}
