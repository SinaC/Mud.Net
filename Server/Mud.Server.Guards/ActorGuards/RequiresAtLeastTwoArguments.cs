using Mud.Server.Guards.Interfaces;
using Mud.Server.Interfaces.Actor;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.Guards.ActorGuards;

public class RequiresAtLeastTwoArguments : IGuard<IActor>
{
    public string? Message { get; init; }

    public string? Guards(IActor actor, IActionInput actionInput, IGameAction gameAction)
    {
        if (actionInput.Parameters.Length < 2)
            return Message ?? gameAction.BuildCommandSyntax();
        return null;
    }
}
