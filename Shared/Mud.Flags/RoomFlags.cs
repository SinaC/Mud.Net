using Mud.Flags.Interfaces;

namespace Mud.Flags;

public class RoomFlags : DataStructures.Flags.Flags, IRoomFlags
{
    public RoomFlags(params string[] flags)
        : base(flags)
    {
    }
}
