using Mud.Server.Interfaces.Guards;

namespace Mud.Server.Interfaces.GameAction;

public interface IPlayerGameActionInfo : IActorGameActionInfo
{
    IPlayerGuard[] PlayerGuards { get; }
}
