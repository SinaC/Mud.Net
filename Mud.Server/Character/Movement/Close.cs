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

namespace Mud.Server.Character.Movement
{
    [CharacterCommand("close", "Movement", MinPosition = Positions.Resting)]
    [Syntax(
            "[cmd] <container|portal>",
            "[cmd] <direction|door>")]
    public class Close : CharacterGameAction
    {
        public ICloseable What { get; protected set; }
        public ExitDirections ExitDirection { get; protected set; }

        public override string Guards(IActionInput actionInput)
        {
            string baseGuards = base.Guards(actionInput);
            if (baseGuards != null)
                return baseGuards;

            if (actionInput.Parameters.Length == 0)
                return "Close what?";
            // Search item: in room, then inventory, then in equipment
            IItem item = FindHelpers.FindItemHere(Actor, actionInput.Parameters[0]);
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
            var door = Actor.Room.VerboseFindDoor(Actor, actionInput.Parameters[0]);
            if (door.exit == null)
                return ""; // no specific message
            if (door.exit.IsClosed)
                Actor.Send("It's already closed."); ;
            What = door.exit;
            ExitDirection = door.exitDirection;
            return null;
        }

        public override void Execute(IActionInput actionInput)
        {
            if (What is IItemCloseable)
            {
                What.Close();
                Actor.Send("Ok.");
                Actor.Act(ActOptions.ToRoom, "{0:N} closes {1}.", Actor, What);
                return;
            }

            if (What is IExit exit)
            {
                // Close this side
                exit.Close();
                Actor.Send("Ok.");
                Actor.Act(ActOptions.ToRoom, "{0:N} closes {1}.", Actor, exit);

                // Close the other side
                IExit otherSideExit = exit.Destination[ExitDirection.ReverseDirection()];
                if (otherSideExit != null)
                {
                    otherSideExit.Close();
                    Actor.Act(exit.Destination.People, "The {0} closes.", otherSideExit);
                }
                else
                    Log.Default.WriteLine(LogLevels.Warning, $"Non bidirectional exit in room {Actor.Room.Blueprint.Id} direction {ExitDirection}");
                return;
            }
        }
    }
}
