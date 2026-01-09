using Microsoft.Extensions.Logging;
using Mud.Domain;
using Mud.Server.Common.Attributes;
using Mud.Server.Common.Extensions;
using Mud.Server.Common.Helpers;
using Mud.Server.GameAction;
using Mud.Server.Guards.Attributes;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Item;
using Mud.Server.Interfaces.Room;

namespace Mud.Server.Commands.Character.Movement;

[CharacterCommand("unlock", "Movement"), MinPosition(Positions.Resting)]
[Syntax(
    "[cmd] <container|portal>",
    "[cmd] <direction|door>")]
[Help(@"[cmd] unlock a closed object or door. You must have the requisite key to [cmd].")]
public class Unlock : CharacterGameAction
{
    private ILogger<Unlock> Logger { get; }

    public Unlock(ILogger<Unlock> logger)
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
            return "Unlock what?";

        // Search item: in room, then inventory, then in equipment
        var item = FindHelpers.FindItemHere(Actor, actionInput.Parameters[0]);
        if (item != null)
        {
            if (item is IItemCloseable itemCloseable)
            {
                if (!itemCloseable.IsCloseable)
                    return "You can't do that.";
                if (!itemCloseable.IsClosed)
                    return "It's not closed.";
                if (itemCloseable.KeyId <= 0)
                    return "It can't be unlocked.";
                bool closeableItemKeyFound = HasKey(itemCloseable.KeyId);
                if (!closeableItemKeyFound)
                    return "You lack the key.";
                if (!itemCloseable.IsLocked)
                    return "It's already unlocked.";
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
            return "It's not closed.";
        if (exit.Blueprint.Key <= 0)
            return "It can't be unlocked.";
        // search key
        bool keyFound = HasKey(exit.Blueprint.Key);
        if (!keyFound)
            return "You lack the key.";
        if (!exit.IsLocked)
            return "It's already unlocked.";
        What = exit;
        ExitDirection = exitDirection;
        return null;
    }

    public override void Execute(IActionInput actionInput)
    {
        if (What is IItemCloseable)
        {
            What.Unlock();
            Actor.Send("*Click*");
            Actor.Act(ActOptions.ToRoom, "{0:N} unlocks {1}.", Actor, What);
            return;
        }
        if (What is IExit exit)
        {
            // Unlock this side
            exit.Unlock();
            Actor.Send("*Click*");
            Actor.Act(ActOptions.ToRoom, "{0:N} unlocks the {1}.", Actor, exit);

            // Unlock other side
            var otherSideExit = exit.Destination[ExitDirection.ReverseDirection()];
            if (otherSideExit != null)
                otherSideExit.Unlock();
            else
                Logger.LogWarning("Non bidirectional exit in room {bluePrintId} direction {exitDirection}", Actor.Room.Blueprint.Id, ExitDirection);
        }
    }

    private bool HasKey(int keyId) => Actor.Inventory.OfType<IItemKey>().Any(x => x.Blueprint.Id == keyId) || Actor.ImmortalMode.HasFlag(ImmortalModeFlags.PassThru);
}
