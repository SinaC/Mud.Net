using Mud.DataStructures.Flags;
using Mud.Server.Flags.Interfaces;

namespace Mud.Server.Flags
{
    public class AssistFlags : Flags<IAssistFlagValues>, IAssistFlags
    {
        public AssistFlags()
            : base()
        {
        }

        public AssistFlags(string flags)
            : base(flags)
        {
        }

        public AssistFlags(params string[] flags)
            : base(flags)
        {
        }
    }
}
