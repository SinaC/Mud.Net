using Mud.Server.Flags.Interfaces;

namespace Mud.Server.Flags;

public class RoomFlags : DataStructures.Flags.Flags, IRoomFlags
{
    public RoomFlags(params string[] flags)
        : base(flags)
    {
    }
}
