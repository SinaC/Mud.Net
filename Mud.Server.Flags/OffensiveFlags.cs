using Mud.DataStructures.Flags;
using Mud.Server.Flags.Interfaces;

namespace Mud.Server.Flags
{
    public class OffensiveFlags : Flags<IOffensiveFlagValues>, IOffensiveFlags
    {
        public OffensiveFlags(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
        }

        public OffensiveFlags(IServiceProvider serviceProvider, string flags)
            : base(serviceProvider, flags)
        {
        }

        public OffensiveFlags(IServiceProvider serviceProvider, params string[] flags)
            : base(serviceProvider, flags)
        {
        }
    }
}
