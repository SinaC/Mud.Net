using Mud.Flags.Interfaces;

namespace Mud.Flags;

public class AssistFlags : DataStructures.Flags.Flags, IAssistFlags
{
    public AssistFlags(params string[] flags)
        : base(flags)
    {
    }
}
