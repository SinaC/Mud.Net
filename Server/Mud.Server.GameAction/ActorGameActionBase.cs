using Mud.Server.Interfaces.Actor;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.GameAction;

public abstract class ActorGameActionBase<TActor, TActorGameActionInfo> : GameActionBase<TActor, TActorGameActionInfo>
    where TActor: class, IActor
    where TActorGameActionInfo : class, IActorGameActionInfo
{
}
