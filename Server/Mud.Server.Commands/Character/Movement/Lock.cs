using Microsoft.Extensions.Logging;
using Mud.Domain;
using Mud.Server.Domain.Attributes;
using Mud.Server.Common.Extensions;
using Mud.Server.Common.Helpers;
using Mud.Server.GameAction;
using Mud.Server.Guards.CharacterGuards;
using Mud.Server.Guards.Interfaces;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Item;
using Mud.Server.Interfaces.Room;

namespace Mud.Server.Commands.Character.Movement;

[CharacterCommand("lock", "Movement")]
[Syntax(
        "[cmd] <container|portal>",
        "[cmd] <direction|door>")]
[Help(@"[cmd] lock a closed object or door. You must have the requisite key to [cmd].")]
public class Lock : CharacterGameAction
{
    protected override IGuard<ICharacter>[] Guards => [new RequiresMinPosition(Positions.Resting), new RequiresAtLeastOneArgument { Message = "Lock what ?" }];

    private ILogger<Lock> Logger { get; }

    public Lock(ILogger<Lock> logger)
    {
        Logger = logger;
    }

    private ICloseable What { get; set; } = default!;
    private ExitDirections ExitDirection { get; set; } = default!;

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
                    return "It's not closed.";
                if (!itemCloseable.IsLockable || itemCloseable.KeyId <= 0)
                    return "It can't be locked.";
                bool closeableItemKeyFound = HasKey(itemCloseable.KeyId);
                if (!closeableItemKeyFound)
                    return "You lack the key.";
                if (itemCloseable.IsLocked)
                    return "It's already locked.";
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
            return "It can't be locked.";
        // search key
        var keyFound = HasKey(exit.Blueprint.Key);
        if (!keyFound)
            return "You lack the key.";
        if (exit.IsLocked)
            return "It's already locked.";
        What = exit;
        ExitDirection = exitDirection;
        return null;
    }

    public override void Execute(IActionInput actionInput)
    {
        if (What is IItemCloseable)
        {
            What.Lock();
            Actor.Send("*Click*");
            Actor.Act(ActOptions.ToRoom, "{0:N} locks {1}.", Actor, What);
            return;
        }
        if (What is IExit exit)
        {
            // Unlock this side
            exit.Lock();
            Actor.Send("*Click*");
            Actor.Act(ActOptions.ToRoom, "{0:N} locks the {1}.", Actor, exit);

            // Unlock other side
            var otherSideExit = exit.Destination[ExitDirection.ReverseDirection()];
            if (otherSideExit != null)
                otherSideExit.Lock();
            else
                Logger.LogWarning("Non-bidirectional exit in room {bluePrintId} direction {exitDirection}", Actor.Room.Blueprint.Id, ExitDirection);
        }
    }

    private bool HasKey(int keyId) => Actor.Inventory.OfType<IItemKey>().Any(x => x.Blueprint.Id == keyId) || Actor.ImmortalMode.IsSet("PassThru");
}
