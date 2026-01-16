using Mud.Common;
using Mud.Domain;
using Mud.Server.Common.Attributes;
using Mud.Server.Common.Helpers;
using Mud.Server.GameAction;
using Mud.Server.Guards.Attributes;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Item;

namespace Mud.Server.Commands.Character.Item;

[CharacterCommand("put", "Item", "Equipment"), MinPosition(Positions.Resting)]
[Syntax("[cmd] <item> [in] <container>")]
[Help(
@"[cmd] puts an object into a container.
[cmd] understand the object names 'ALL' for all objects and
'ALL.object' for all objects with the same name.
[cmd] X.sword is also allowed to put the Xth sword of the list.")]
public class Put : CharacterGameAction
{
    protected IItem[] What { get; set; } = default!;
    protected IItemContainer Container { get; set; } = default!;

    public override string? Guards(IActionInput actionInput)
    {
        var baseGuards = base.Guards(actionInput);
        if (baseGuards != null)
            return baseGuards;

        if (actionInput.Parameters.Length < 2)
            return "Put what in what?";

        // Extract actionInput.Parameters
        var whatParameter = actionInput.Parameters[0];
        var whereParameter = StringCompareHelpers.StringEquals(actionInput.Parameters[1].Value, "in") ? actionInput.Parameters[2] : actionInput.Parameters[1];

        // search container
        if (whereParameter.IsAll)
            return "You can't do that";

        var where = FindHelpers.FindItemHere(Actor, whereParameter);
        if (where == null)
            return StringHelpers.ItemNotFound;
        if (where is not IItemContainer container)
            return "That's not a container.";
        Container = container;
        if (Container.IsClosed)
            return Actor.ActPhrase("The {0} is closed.", Container);

        // put all [in] container, put all.item [in] container
        if (whatParameter.IsAll)
        {
            // list must be cloned because it'll be modified when putting an item
            What = !whatParameter.IsAllOnly
                // put all.item [in] container
                ? FindHelpers.FindAllByName(Actor.Inventory.Where(Actor.CanSee), whatParameter).ToArray()
                // put all [in] container
                : Actor.Inventory.Where(Actor.CanSee).ToArray();
            if (What.Length == 0)
                return StringHelpers.ItemInventoryNotFound;
            return null;
        }

        // put item [in] container
        var item = FindHelpers.FindByName(Actor.Inventory.Where(Actor.CanSee), whatParameter);
        if (item == null)
            return StringHelpers.ItemInventoryNotFound;
        What = [item];
        return null;
    }

    public override void Execute(IActionInput actionInput)
    {
        foreach (IItem item in What)
            PutItem(item, Container);
    }

    protected virtual bool PutItem(IItem item, IItemContainer container)
    {
        //
        if (item.ItemFlags.IsSet("NoDrop"))
        {
            Actor.Send("You can't let go of it.");
            return false;
        }

        if (item is IItemQuest)
        {
            Actor.Send("You cannot put that anywhere.");
            return false;
        }

        if (container.Content.Count() + 1 > container.MaxItems)
        {
            Actor.Send("It won't fit.");
            return false;
        }
        var itemWeight = item.TotalWeight;
        if (itemWeight + container.TotalWeight > container.MaxWeight
            || itemWeight > container.MaxWeightPerItem)
        {
            Actor.Send("It won't fit.");
            return false;
        }

        if (item == container)
        {
            Actor.Send("You can't fold it into itself.");
            return false;
        }

        // TODO: pit
        Actor.Act(ActOptions.ToAll, "{0:N} put{0:v} {1} in {2}.", Actor, item, container);
        item.ChangeContainer(container);
        return true;
    }
}
