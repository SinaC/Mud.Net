using Mud.Common;
using Mud.Domain;
using Mud.Server.Common.Attributes;
using Mud.Server.Common.Helpers;
using Mud.Server.GameAction;
using Mud.Server.Guards.Attributes;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.Entity;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Item;

namespace Mud.Server.Commands.Character.Item;

[CharacterCommand("get", "Item", "Inventory"), MinPosition(Positions.Resting), NoArgumentGuard("Get what ?")]
[Alias("take")]
[Syntax(
    "[cmd] <item>",
    "[cmd] <item> <container>")]
[Help(
@"[cmd] gets an object, either lying on the ground, or from a container, or even
from a corpse. TAKE is a synonym for get.
[cmd] understand the object names 'ALL' for all objects and
'ALL.object' for all objects with the same name.
[cmd] X.sword is also allowed to get the Xth sword of the list.")]
public class Get : CharacterGameAction
{
    protected IItem[] What { get; set; } = default!;
    protected IContainer Where { get; set; } = default!;

    public override string? Guards(IActionInput actionInput)
    {
        var baseGuards = base.Guards(actionInput);
        if (baseGuards != null)
            return baseGuards;

        var whatParameter = actionInput.Parameters[0];
        // get item, get all, get all.item
        if (actionInput.Parameters.Length == 1)
        {
            // get all or get all.
            if (whatParameter.IsAll)
            {
                // TODO: same code as below (***) except source collection (Room.Content)
                // list must be cloned because it'll be modified when getting an item
                bool allDot = false;
                if (!whatParameter.IsAllOnly) // get all.item
                {
                    What = FindHelpers.FindAllByName(Actor.Room.Content.Where(Actor.CanSee), whatParameter).ToArray();
                    allDot = true;
                }
                else // get all
                    What = Actor.Room.Content.Where(Actor.CanSee).ToArray();
                if (What.Length == 0)
                {
                    if (allDot)
                        return "I see nothing like that here.";
                    return "I see nothing here.";
                }
                return null;
            }
            // get item
            var itemInRoom = FindHelpers.FindByName(Actor.Room.Content.Where(Actor.CanSee), actionInput.Parameters[0]);
            if (itemInRoom == null)
                return $"I see no {whatParameter.Value} here.";
            What = [itemInRoom];
            return null;
        }
        // get item [from] container, get all [from] container, get all.item [from] container
        ICommandParameter whereParameter = StringCompareHelpers.StringEquals(actionInput.Parameters[1].Value, "from")
            ? actionInput.Parameters[2]
            : actionInput.Parameters[1];
        if (whereParameter.IsAll)
            return "You can't do that";
        // search container
        var targetItem = FindHelpers.FindItemHere(Actor, whereParameter);
        if (targetItem == null)
            return $"I see no {whereParameter.Value} here.";
        if (targetItem is not IContainer where)
            return "That's not a container.";
        Where = where;
        if (Where is ICloseable closeable && closeable.IsClosed)
            return Actor.ActPhrase("The {0} is closed.", Where);
        // from here, we have a container
        // get all [from] container, get all.item [from] container
        if (whatParameter.IsAll)
        {
            // list must be cloned because it'll be modified when getting an item
            bool allDot = false;
            if (!whatParameter.IsAllOnly) // get all.item [from] container
            {
                What = FindHelpers.FindAllByName(Where.Content.Where(Actor.CanSee), whatParameter).ToArray();
                allDot = true;
            }
            else // get all [from] container
                What = Where.Content.Where(Actor.CanSee).ToArray();
            if (What.Length == 0)
            {
                if (allDot)
                    return $"I see nothing like that in the {whereParameter}.";
                return $"I see nothing in the {whereParameter}.";
            }
            return null;
        }
        // get item [from] container
        var item = FindHelpers.FindByName(Where.Content.Where(Actor.CanSee), whatParameter);
        if (item == null)
            return $"I see nothing like that in the {whereParameter}.";
        What = [item];
        return null;
    }

    public override void Execute(IActionInput actionInput)
    {
        foreach (var item in What)
            Actor.GetItem(item, Where);
    }
}
