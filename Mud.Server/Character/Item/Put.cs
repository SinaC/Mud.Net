using Mud.Common;
using Mud.Domain;
using Mud.Server.Common;
using Mud.Server.GameAction;
using Mud.Server.Helpers;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Item;
using System.Linq;

namespace Mud.Server.Character.Item
{
    [CharacterCommand("put", "Item", "Equipment", MinPosition = Positions.Resting)]
    [Syntax("[cmd] <item> [in] <container>")]
    public class Put : CharacterGameAction
    {
        public IItem[] What { get; protected set; }
        public IItemContainer Container { get; protected set; }

        public override string Guards(IActionInput actionInput)
        {
            string baseGuards = base.Guards(actionInput);
            if (baseGuards != null)
                return baseGuards;

            if (actionInput.Parameters.Length < 2)
                return "Put what in what?";

            // Extract actionInput.Parameters
            ICommandParameter whatParameter = actionInput.Parameters[0];
            ICommandParameter whereParameter = StringCompareHelpers.StringEquals(actionInput.Parameters[1].Value, "in") ? actionInput.Parameters[2] : actionInput.Parameters[1];

            // search container
            if (whereParameter.IsAll)
                return "You can't do that";

            IItem where = FindHelpers.FindItemHere(Actor, whereParameter);
            if (where == null)
                return StringHelpers.ItemNotFound;
            Container = where as IItemContainer;
            if (Container == null)
                return "That's not a container.";
            if (Container.IsClosed)
                return Actor.ActPhrase("The {0} is closed.", Container);

            // put all [in] container, put all.item [in] container
            if (whatParameter.IsAll)
            {
                // list must be cloned because it'll be modified when putting an item
                What = !string.IsNullOrWhiteSpace(whatParameter.Value)
                    // put all.item [in] container
                    ? FindHelpers.FindAllByName(Actor.Inventory.Where(x => Actor.CanSee(x)), whatParameter).ToArray()
                    // put all [in] container
                    : Actor.Inventory.Where(x => Actor.CanSee(x)).ToArray();
                if (!What.Any())
                    return StringHelpers.ItemInventoryNotFound;
                return null;
            }

            // put item [in] container
            IItem item = FindHelpers.FindByName(Actor.Inventory.Where(x => Actor.CanSee(x)), whatParameter);
            if (item == null)
                return StringHelpers.ItemInventoryNotFound;
            What = item.Yield().ToArray();
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
            if (item.ItemFlags.HasFlag(ItemFlags.NoDrop))
            {
                Actor.Send("You can't let go of it.");
                return false;
            }
            int itemWeight = item.TotalWeight;
            if ((itemWeight + container.TotalWeight > container.MaxWeight)
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
}
