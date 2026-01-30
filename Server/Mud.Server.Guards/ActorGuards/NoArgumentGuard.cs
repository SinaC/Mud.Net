using Mud.Server.Common.Helpers;
using Mud.Server.Interfaces.Actor;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Guards;

namespace Mud.Server.Guards.ActorGuards;

public class NoArgumentGuard(string? message) : IActorGuard
{
    public string? Message => message;

    public string? Guards(IActor actor, IActionInput actionInput, IGameAction gameAction)
    {
        if (actionInput.Parameters.Length == 0)
            return Message ?? gameAction.BuildCommandSyntax();
        return null;
    }

    public string? Guards(IActor actor, ICommandParameter[] commandParameters)
    {
        if (commandParameters.Length == 0)
            return Message ?? StringHelpers.SomethingGoesWrong;
        return null;
    }
}
