using Mud.Server.Interfaces.Room;

namespace Mud.Server.Interfaces.Affect.Room;

public interface IRoomAffect : IAffect
{
    // RoomFlags
    void Apply(IRoom room);
}
