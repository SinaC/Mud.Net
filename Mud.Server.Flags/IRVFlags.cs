using Mud.DataStructures.Flags;
using Mud.Server.Flags.Interfaces;

namespace Mud.Server.Flags
{
    public class IRVFlags : Flags<IIRVFlagValues>, IIRVFlags
    {
        public IRVFlags(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
        }

        public IRVFlags(IServiceProvider serviceProvider, string flags)
            : base(serviceProvider, flags)
        {
        }

        public IRVFlags(IServiceProvider serviceProvider, params string[] flags)
            : base(serviceProvider, flags)
        {
        }
    }
}
