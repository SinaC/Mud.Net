using Mud.DataStructures.Flags;
using Mud.Server.Flags.Interfaces;

namespace Mud.Server.Flags
{
    public class RoomFlags : Flags<IRoomFlagValues>, IRoomFlags
    {
        public RoomFlags(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
        }

        public RoomFlags(IServiceProvider serviceProvider, string flags)
            : base(serviceProvider, flags)
        {
        }

        public RoomFlags(IServiceProvider serviceProvider, params string[] flags)
            : base(serviceProvider, flags)
        {
        }
    }
}
