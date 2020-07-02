using Mud.DataStructures.Flags;
using Mud.Server.Flags.Interfaces;

namespace Mud.Server.Flags
{
    public class BodyForms : Flags<IBodyFormValues>, IBodyForms
    {
        public BodyForms()
            : base()
        {
        }

        public BodyForms(string flags)
            : base(flags)
        {
        }

        public BodyForms(params string[] flags)
            : base(flags)
        {
        }
    }
}
