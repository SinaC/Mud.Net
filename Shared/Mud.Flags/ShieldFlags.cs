using Mud.Flags.Interfaces;

namespace Mud.Flags;

public class ShieldFlags : DataStructures.Flags.Flags, IShieldFlags
{
    public ShieldFlags(params string[] flags)
        : base(flags)
    {
    }
}
