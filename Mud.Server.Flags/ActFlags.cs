using Mud.DataStructures.Flags;
using Mud.Server.Flags.Interfaces;

namespace Mud.Server.Flags;

public class ActFlags : Flags<IActFlagValues>, IActFlags
{
    public ActFlags(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
    }

    public ActFlags(IServiceProvider serviceProvider, string flags)
        : base(serviceProvider, flags)
    {
    }

    public ActFlags(IServiceProvider serviceProvider, params string[] flags)
        : base(serviceProvider, flags)
    {
    }
}
