using Mud.Server.Interfaces.Actor;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.Interfaces.Guards;

public interface IGuard<TActor>
    where TActor: IActor
{
    string? Guards(TActor actor, IActionInput actionInput, IGameAction gameAction);
    string? Guards(TActor actor, ICommandParameter[] commandParameters);
}
