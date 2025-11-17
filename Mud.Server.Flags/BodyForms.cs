using Mud.DataStructures.Flags;
using Mud.Server.Flags.Interfaces;

namespace Mud.Server.Flags
{
    public class BodyForms : Flags<IBodyFormValues>, IBodyForms
    {
        public BodyForms(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
        }

        public BodyForms(IServiceProvider serviceProvider, string flags)
            : base(serviceProvider, flags)
        {
        }

        public BodyForms(IServiceProvider serviceProvider, params string[] flags)
            : base(serviceProvider, flags)
        {
        }
    }
}
