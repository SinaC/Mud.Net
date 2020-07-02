using Mud.DataStructures.Flags;
using Mud.Server.Flags.Interfaces;

namespace Mud.Server.Flags
{
    public class ActFlags : Flags<IActFlagValues>, IActFlags
    {
        public ActFlags()
            : base()
        {
        }

        public ActFlags(string flags)
            : base(flags)
        {
        }

        public ActFlags(params string[] flags)
            : base(flags)
        {
        }
    }
}
