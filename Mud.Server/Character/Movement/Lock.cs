using Mud.Domain;
using Mud.Domain.Extensions;
using Mud.Logger;
using Mud.Server.Common;
using Mud.Server.GameAction;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Item;
using Mud.Server.Interfaces.Room;
using System.Linq;

namespace Mud.Server.Character.Movement
{
    [CharacterCommand("lock", "Movement", MinPosition = Positions.Resting)]
    [Syntax(
            "[cmd] <container|portal>",
            "[cmd] <direction|door>")]
    public class Lock : CharacterGameAction
    {
        public ICloseable What { get; protected set; }
        public ExitDirections ExitDirection { get; protected set; }

        public override string Guards(IActionInput actionInput)
        {
            string baseGuards = base.Guards(actionInput);
            if (baseGuards != null)
                return baseGuards;

            if (actionInput.Parameters.Length == 0)
                return "Unlock what?";

            // Search item: in room, then inventory, then in equipment
            IItem item = FindHelpers.FindItemHere(Actor, actionInput.Parameters[0]);
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
            var door = Actor.Room.VerboseFindDoor(Actor, actionInput.Parameters[0]);
            if (door.exit == null)
                return ""; // no specific message
            if (!door.exit.IsClosed)
                return "It's not closed.";
            if (door.exit.Blueprint.Key <= 0)
                return "It can't be locked.";
            // search key
            bool keyFound = HasKey(door.exit.Blueprint.Key);
            if (!keyFound)
                return "You lack the key.";
            if (door.exit.IsLocked)
                return "It's already locked.";
            What = door.exit;
            ExitDirection = door.exitDirection;
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
                IExit otherSideExit = exit.Destination[ExitDirection.ReverseDirection()];
                if (otherSideExit != null)
                    otherSideExit.Lock();
                else
                    Log.Default.WriteLine(LogLevels.Warning, $"Non bidirectional exit in room {Actor.Room.Blueprint.Id} direction {ExitDirection}");
            }
        }

        private bool HasKey(int keyId) => Actor.Inventory.OfType<IItemKey>().Any(x => x.Blueprint.Id == keyId) || (Actor as IPlayableCharacter)?.IsImmortal == true;
    }
}
