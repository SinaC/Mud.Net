using Microsoft.Extensions.Logging;
using Mud.Domain;
using Mud.Server.Common.Attributes;
using Mud.Server.Common.Extensions;
using Mud.Server.Common.Helpers;
using Mud.Server.GameAction;
using Mud.Server.Guards.CharacterGuards;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Guards;
using Mud.Server.Interfaces.Item;
using Mud.Server.Interfaces.Room;

namespace Mud.Server.Commands.Character.Movement;

[CharacterCommand("open", "Movement")]
[Syntax(
    "[cmd] <container|portal>",
    "[cmd] <direction|door>")]
[Help(@"[cmd] open an object or a door.")]
public class Open : CharacterGameAction
{
    protected override IGuard<ICharacter>[] Guards => [new RequiresMinPosition(Positions.Resting), new RequiresAtLeastOneArgument { Message = "Open what ?" }];

    private ILogger<Open> Logger { get; }

    public Open(ILogger<Open> logger)
    {
        Logger = logger;
    }

    private ICloseable What { get; set; } = default!;
    private ExitDirections ExitDirection { get; set; }

    public override string? CanExecute(IActionInput actionInput)
    {
        var baseGuards = base.CanExecute(actionInput);
        if (baseGuards != null)
            return baseGuards;

        // Search item: in room, then inventory, then in equipment
        var item = FindHelpers.FindItemHere(Actor, actionInput.Parameters[0]);
        if (item != null)
        {
            if (item is IItemCloseable itemCloseable)
            {
                if (!itemCloseable.IsCloseable)
                    return "You can't do that.";
                if (!itemCloseable.IsClosed)
                    return "It's already opened.";
                if (itemCloseable.IsLocked)
                    return "It's locked.";
            }
            else
                return "You can't do that.";
            What = itemCloseable;
            return null;
        }

        // No item found, search door
        var (exit, exitDirection) = Actor.Room.VerboseFindDoor(Actor, actionInput.Parameters[0]);
        if (exit == null)
            return ""; // no specific message
        if (!exit.IsClosed)
            return "It's already open.";
        if (exit.IsLocked)
            return "It's locked.";
        What = exit;
        ExitDirection = exitDirection;
        return null;
    }

    public override void Execute(IActionInput actionInput)
    {
        // Item
        if (What is IItemCloseable)
        {
            What.Open();
            Actor.Send("Ok.");
            Actor.Act(ActOptions.ToRoom, "{0:N} opens {1}.", Actor, What);
        }
        // Door
        else if (What is IExit exit)
        {
            // Open this side side
            exit.Open();
            Actor.Send("Ok.");
            Actor.Act(ActOptions.ToRoom, "{0:N} opens the {1}.", Actor, exit);

            // Open the other side
            var otherSideExit = exit.Destination[ExitDirection.ReverseDirection()];
            if (otherSideExit != null)
            {
                otherSideExit.Open();
                Actor.Act(exit.Destination.People, "The {0} opens.", otherSideExit);
            }
            else
                Logger.LogWarning("Non bidirectional exit in room {bluePrintId} direction {exitDirection}", Actor.Room.Blueprint.Id, ExitDirection);
        }
    }
}
