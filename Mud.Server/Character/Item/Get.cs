using Mud.Common;
using Mud.Domain;
using Mud.Server.Common;
using Mud.Server.GameAction;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.Entity;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Item;
using System.Linq;

namespace Mud.Server.Character.Item
{
    [CharacterCommand("get", "Item", "Inventory", MinPosition = Positions.Resting)]
    [Alias("take")]
    [Syntax(
        "[cmd] <item>",
        "[cmd] <item> <container>")]
    public class Get : CharacterGameAction
    {
        public IItem[] What { get; protected set; }
        public IContainer Where { get; protected set; }

        public override string Guards(IActionInput actionInput)
        {
            string baseGuards = base.Guards(actionInput);
            if (baseGuards != null)
                return baseGuards;

            if (actionInput.Parameters.Length == 0)
                return "Get what?";

            ICommandParameter whatParameter = actionInput.Parameters[0];
            // get item, get all, get all.item
            if (actionInput.Parameters.Length == 1)
            {
                // get all or get all.
                if (whatParameter.IsAll)
                {
                    // TODO: same code as below (***) except source collection (Room.Content)
                    // list must be cloned because it'll be modified when getting an item
                    bool allDot = false;
                    if (!string.IsNullOrWhiteSpace(whatParameter.Value)) // get all.item
                    {
                        What = FindHelpers.FindAllByName(Actor.Room.Content.Where(x => Actor.CanSee(x)), whatParameter).ToArray();
                        allDot = true;
                    }
                    else // get all
                        What = Actor.Room.Content.Where(x => Actor.CanSee(x)).ToArray();
                    if (!What.Any())
                    {
                        if (allDot)
                            return "I see nothing like that here.";
                        return "I see nothing here.";
                    }
                    return null;
                }
                // get item
                IItem itemInRoom = FindHelpers.FindByName(Actor.Room.Content.Where(x => Actor.CanSee(x)), actionInput.Parameters[0]);
                if (itemInRoom == null)
                    return "I see no {0} here.";
                What = itemInRoom.Yield().ToArray();
                return null;
            }
            // get item [from] container, get all [from] container, get all.item [from] container
            ICommandParameter whereParameter = StringCompareHelpers.StringEquals(actionInput.Parameters[1].Value, "from")
                ? actionInput.Parameters[2]
                : actionInput.Parameters[1];
            if (whereParameter.IsAll)
                return "You can't do that";
            // search container
            IItem targetItem = FindHelpers.FindItemHere(Actor, whereParameter);
            if (targetItem == null)
                return $"I see no {whereParameter.Value} here.";
            Where = targetItem as IContainer;
            if (Where == null)
                return "That's not a container.";
            if (Where is ICloseable closeable && closeable.IsClosed)
                return Actor.ActPhrase("The {0} is closed.", Where);
            // from here, we have a container
            // get all [from] container, get all.item [from] container
            if (whatParameter.IsAll)
            {
                // list must be cloned because it'll be modified when getting an item
                bool allDot = false;
                if (!string.IsNullOrWhiteSpace(whatParameter.Value)) // get all.item [from] container
                {
                    What = FindHelpers.FindAllByName(Where.Content.Where(x => Actor.CanSee(x)), whatParameter).ToArray();
                    allDot = true;
                }
                else // get all [from] container
                    What = Where.Content.Where(x => Actor.CanSee(x)).ToArray();
                if (!What.Any())
                {
                    if (allDot)
                        return $"I see nothing like that in the {whereParameter}.";
                    return $"I see nothing in the {whereParameter}.";
                }
                return null;
            }
            // get item [from] container
            IItem item = FindHelpers.FindByName(Where.Content.Where(x => Actor.CanSee(x)), whatParameter);
            if (item == null)
                return $"I see nothing like that in the {whereParameter}.";
            What = item.Yield().ToArray();
            return null;
        }

        public override void Execute(IActionInput actionInput)
        {
            foreach (IItem item in What)
                Actor.GetItem(item, Where);
        }
    }
}
