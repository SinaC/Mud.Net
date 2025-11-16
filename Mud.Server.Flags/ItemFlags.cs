using Mud.DataStructures.Flags;
using Mud.Server.Flags.Interfaces;

namespace Mud.Server.Flags
{
    public class ItemFlags : Flags<IItemFlagValues>, IItemFlags
    {
        public ItemFlags(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
        }

        public ItemFlags(IServiceProvider serviceProvider, string flags)
            : base(serviceProvider, flags)
        {
        }

        public ItemFlags(IServiceProvider serviceProvider, params string[] flags)
            : base(serviceProvider, flags)
        {
        }
    }
}
