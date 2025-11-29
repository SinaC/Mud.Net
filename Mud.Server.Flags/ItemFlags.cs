using Mud.Common.Attributes;
using Mud.DataStructures.Flags;
using Mud.Server.Flags.Interfaces;

namespace Mud.Server.Flags;

[Export(typeof(IItemFlags))]
public class ItemFlags : Flags<IItemFlagValues>, IItemFlags
{
    public ItemFlags(IItemFlagValues flagValues)
        : base(flagValues)
    {
    }
}
