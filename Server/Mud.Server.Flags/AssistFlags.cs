using Mud.Server.Flags.Interfaces;

namespace Mud.Server.Flags;

public class AssistFlags : DataStructures.Flags.Flags, IAssistFlags
{
    public AssistFlags(params string[] flags)
        : base(flags)
    {
    }
}
