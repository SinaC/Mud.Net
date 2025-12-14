using Mud.Server.Flags.Interfaces;

namespace Mud.Server.Flags;

public class ItemFlags : DataStructures.Flags.Flags, IItemFlags
{
    public ItemFlags(params string[] flags)
        : base(flags)
    {
    }
}
