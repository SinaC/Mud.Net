using Mud.Common.Attributes;
using Mud.DataStructures.Flags;
using Mud.Server.Flags.Interfaces;

namespace Mud.Server.Flags;

[Export(typeof(IAssistFlags))]
public class AssistFlags : Flags<IAssistFlagValues>, IAssistFlags
{
    public AssistFlags(IAssistFlagValues flagValues)
        : base(flagValues)
    {
    }
}
