using Mud.DataStructures.Flags;
using Mud.Server.Flags.Interfaces;

namespace Mud.Server.Flags
{
    public class ItemFlags : Flags<IItemFlagValues>, IItemFlags
    {
        public ItemFlags()
            : base()
        {
        }

        public ItemFlags(string flags)
            : base(flags)
        {
        }

        public ItemFlags(params string[] flags)
            : base(flags)
        {
        }
    }
}
