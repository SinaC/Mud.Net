using Mud.DataStructures.Flags;
using Mud.Server.Flags.Interfaces;

namespace Mud.Server.Flags
{
    public class OffensiveFlags : Flags<IOffensiveFlagValues>, IOffensiveFlags
    {
        public OffensiveFlags()
            : base()
        {
        }

        public OffensiveFlags(string flags)
            : base(flags)
        {
        }

        public OffensiveFlags(params string[] flags)
            : base(flags)
        {
        }
    }
}
