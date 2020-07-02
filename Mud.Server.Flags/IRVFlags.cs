using Mud.DataStructures.Flags;
using Mud.Server.Flags.Interfaces;

namespace Mud.Server.Flags
{
    public class IRVFlags : Flags<IIRVFlagValues>, IIRVFlags
    {
        public IRVFlags()
            : base()
        {
        }

        public IRVFlags(string flags)
            : base(flags)
        {
        }

        public IRVFlags(params string[] flags)
            : base(flags)
        {
        }
    }
}
