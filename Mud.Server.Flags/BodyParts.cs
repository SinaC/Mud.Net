using Mud.DataStructures.Flags;
using Mud.Server.Flags.Interfaces;

namespace Mud.Server.Flags
{
    public class BodyParts : Flags<IBodyPartValues>, IBodyParts
    {
        public BodyParts()
            : base()
        {
        }

        public BodyParts(string flags)
            : base(flags)
        {
        }

        public BodyParts(params string[] flags)
            : base(flags)
        {
        }
    }
}
