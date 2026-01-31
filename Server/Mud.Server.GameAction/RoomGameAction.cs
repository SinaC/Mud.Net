using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Room;

namespace Mud.Server.GameAction;

public abstract class RoomGameAction : ActorGameActionBase<IRoom, IRoomGameActionInfo>
{
}
