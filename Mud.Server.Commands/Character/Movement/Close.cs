using Microsoft.Extensions.Logging;
using Mud.Domain;
using Mud.Domain.Extensions;
using Mud.Server.Common;
using Mud.Server.Common.Helpers;
using Mud.Server.GameAction;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Item;
using Mud.Server.Interfaces.Room;

namespace Mud.Server.Commands.Character.Movement;

[CharacterCommand("close", "Movement", MinPosition = Positions.Resting)]
[Syntax(
        "[cmd] <container|portal>",
        "[cmd] <direction|door>")]
[Help(@"[cmd] close an object or a door.")]
public class Close : CharacterGameAction
{
    private ILogger<Close> Logger { get; }

    public Close(ILogger<Close> logger)
    {
        Logger = logger;
    }

    protected ICloseable What { get; set; } = default!;
    protected ExitDirections ExitDirection { get; set; }

    public override string? Guards(IActionInput actionInput)
    {
        var baseGuards = base.Guards(actionInput);
        if (baseGuards != null)
            return baseGuards;

        if (actionInput.Parameters.Length == 0)
            return "Close what?";
        // Search item: in room, then inventory, then in equipment
        var item = FindHelpers.FindItemHere(Actor, actionInput.Parameters[0]);
        if (item != null)
        {
            if (item is IItemCloseable itemCloseable)
            {
                if (!itemCloseable.IsCloseable)
                    return "You can't do that.";
                if (itemCloseable.IsClosed)
                    return "It's already closed.";
            }
            else
                return "You can't do that.";
            What = itemCloseable;
            return null;
        }

        // No item found, search door
        var (exit, exitDirection) = Actor.Room.VerboseFindDoor(Actor, actionInput.Parameters[0]);
        if (exit == null)
            return string.Empty; // no specific message
        if (exit.IsClosed)
            Actor.Send("It's already closed.");
        What = exit;
        ExitDirection = exitDirection;
        return null;
    }

    public override void Execute(IActionInput actionInput)
    {
        // Item
        if (What is IItemCloseable)
        {
            What.Close();
            Actor.Send("Ok.");
            Actor.Act(ActOptions.ToRoom, "{0:N} closes {1}.", Actor, What);
        }
        // Door
        else if (What is IExit exit)
        {
            // Close this side
            exit.Close();
            Actor.Send("Ok.");
            Actor.Act(ActOptions.ToRoom, "{0:N} closes {1}.", Actor, exit);

            // Close the other side
            var otherSideExit = exit.Destination[ExitDirection.ReverseDirection()];
            if (otherSideExit != null)
            {
                otherSideExit.Close();
                Actor.Act(exit.Destination.People, "The {0} closes.", otherSideExit);
            }
            else
                Logger.LogWarning("Non bidirectional exit in room {bluePrintId} direction {exitDirection}", Actor.Room.Blueprint.Id, ExitDirection);
        }
    }
}
