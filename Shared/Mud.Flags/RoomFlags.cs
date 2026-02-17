using Mud.Flags.Interfaces;

namespace Mud.Flags;

public class RoomFlags(params string[] flags) : DataStructures.Flags.Flags(flags), IRoomFlags
{
}
