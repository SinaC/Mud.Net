using Mud.Flags.Interfaces;

namespace Mud.Flags;

public class ItemFlags : DataStructures.Flags.Flags, IItemFlags
{
    public ItemFlags(params string[] flags)
        : base(flags)
    {
    }
}
