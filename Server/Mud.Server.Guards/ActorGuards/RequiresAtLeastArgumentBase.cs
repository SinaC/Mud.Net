using Mud.Server.Guards.Interfaces;
using Mud.Server.Interfaces.Actor;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.Guards.ActorGuards;

public abstract class RequiresAtLeastArgumentBase : IGuard<IActor>
{
    private int MinArgumentCount { get; }

    protected RequiresAtLeastArgumentBase(int minArgumentCount)
    {
        MinArgumentCount = minArgumentCount;
    }

    public string? Message { get; init; }

    public string? Guards(IActor actor, IActionInput actionInput, IGameAction gameAction)
    {
        if (actionInput.Parameters.Length < MinArgumentCount)
            return Message ?? gameAction.BuildCommandSyntax();
        return null;
    }
}
