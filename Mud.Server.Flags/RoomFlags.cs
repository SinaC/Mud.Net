using Mud.DataStructures.Flags;
using Mud.Server.Flags.Interfaces;

namespace Mud.Server.Flags
{
    public class RoomFlags : Flags<IRoomFlagValues>, IRoomFlags
    {
        public RoomFlags()
            : base()
        {
        }

        public RoomFlags(string flags)
            : base(flags)
        {
        }

        public RoomFlags(params string[] flags)
            : base(flags)
        {
        }
    }
}
