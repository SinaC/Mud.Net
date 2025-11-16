using Mud.DataStructures.Flags;
using Mud.Server.Flags.Interfaces;

namespace Mud.Server.Flags;

public class AssistFlags : Flags<IAssistFlagValues>, IAssistFlags
{
    public AssistFlags(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
    }

    public AssistFlags(IServiceProvider serviceProvider, string flags)
        : base(serviceProvider, flags)
    {
    }

    public AssistFlags(IServiceProvider serviceProvider, params string[] flags)
        : base(serviceProvider, flags)
    {
    }
}
