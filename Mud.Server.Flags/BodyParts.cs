using Mud.DataStructures.Flags;
using Mud.Server.Flags.Interfaces;

namespace Mud.Server.Flags
{
    public class BodyParts : Flags<IBodyPartValues>, IBodyParts
    {
        public BodyParts(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
        }

        public BodyParts(IServiceProvider serviceProvider, string flags)
            : base(serviceProvider, flags)
        {
        }

        public BodyParts(IServiceProvider serviceProvider, params string[] flags)
            : base(serviceProvider, flags)
        {
        }
    }
}
