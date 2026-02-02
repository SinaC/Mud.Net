using Mud.Server.Guards.Interfaces;
using Mud.Server.Interfaces.Actor;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.Guards.ActorGuards;

public class RequiresAtLeastOneArgument : IGuard<IActor>
{
    public string? Message { get; init; }

    public string? Guards(IActor actor, IActionInput actionInput, IGameAction gameAction)
    {
        if (actionInput.Parameters.Length < 1)
            return Message ?? gameAction.BuildCommandSyntax();
        return null;
    }
}
