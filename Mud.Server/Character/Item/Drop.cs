using Mud.Common;
using Mud.Domain;
using Mud.Server.Common;
using Mud.Server.GameAction;
using Mud.Server.Helpers;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Item;
using System;
using System.Linq;

namespace Mud.Server.Character.Item
{
    [CharacterCommand("drop", "Item", "Inventory", MinPosition = Positions.Resting)]
    [Syntax(
            "[cmd] <item>",
            "[cmd] <amount> coin|coins|silver|gold",
            "[cmd] all")]
    // Drop item
    // Drop all
    // Drop all.item
    public class Drop : CharacterGameAction
    {
        private IItemManager ItemManager { get; }

        public long Silver { get; protected set; }
        public long Gold { get; protected set; }
        public IItem[] What { get; protected set; }

        public Drop(IItemManager itemManager)
        {
            ItemManager = itemManager;
        }

        public override string Guards(IActionInput actionInput)
        {
            string baseGuards = base.Guards(actionInput);
            if (baseGuards != null)
                return baseGuards;

            if (actionInput.Parameters.Length == 0)
                return "Drop what?";

            // money?
            if (actionInput.Parameters[0].IsLong && actionInput.Parameters.Length > 1)
            {
                long amount = actionInput.Parameters[0].AsLong;
                string what = actionInput.Parameters[1].Value;
                if (what == "coin" || what == "coins" || what == "silver" || what == "gold")
                {
                    // check parameters
                    if (amount <= 0)
                        return "Sorry, you can't do that.";

                    if (what == "coin" || what == "coins" || what == "silver")
                    {
                        if (amount > Actor.SilverCoins)
                            return "You don't have that much silver.";
                        Silver = amount;
                    }
                    else
                    {
                        if (amount > Actor.GoldCoins)
                            return "You don't have that much gold.";
                        Gold = amount;
                    }
                    return null;
                }
            }

            ICommandParameter whatParameter = actionInput.Parameters[0];

            // drop all, drop all.item
            if (actionInput.Parameters[0].IsAll)
            {
                // list must be cloned because it'll be modified when dropping an item
                What = !string.IsNullOrWhiteSpace(whatParameter.Value)
                    // drop all.item
                    ? FindHelpers.FindAllByName(Actor.Inventory.Where(x => Actor.CanSee(x) && !(x is IItemQuest)), whatParameter).ToArray()
                    // drop all
                    : Actor.Inventory.Where(x => Actor.CanSee(x) && !(x is IItemQuest)).ToArray();
                if (!What.Any())
                    return StringHelpers.ItemInventoryNotFound;
                return null;
            }
            // drop item
            IItem item = FindHelpers.FindByName(Actor.Inventory.Where(x => Actor.CanSee(x)), whatParameter);
            if (item == null)
                return StringHelpers.ItemInventoryNotFound;
            if (item is IItemQuest)
                return "You cannot drop quest items.";
            What = item.Yield().ToArray();
            return null;
        }

        public override void Execute(IActionInput actionInput)
        {
            // money
            if (Silver > 0 || Gold > 0)
            {
                // group money in the room and recreate a new money item
                var clone = Actor.Room.Content.OfType<IItemMoney>().ToArray();
                foreach (IItemMoney moneyInRoom in clone)
                {
                    Silver += moneyInRoom.SilverCoins;
                    Gold += moneyInRoom.GoldCoins;
                    ItemManager.RemoveItem(moneyInRoom);
                }
                // recreate new money item
                ItemManager.AddItemMoney(Guid.NewGuid(), Silver, Gold, Actor.Room);
                Actor.Act(ActOptions.ToRoom, "{0:N} drops some coins", Actor);
                Actor.Send("Ok.");
                return;
            }
            // item(s)
            foreach (IItem item in What)
                DropItem(item);
        }

        protected virtual bool DropItem(IItem item)
        {
            //
            if (item.ItemFlags.HasFlag(ItemFlags.NoDrop))
            {
                Actor.Send("You can't let go of it.");
                return false;
            }

            //
            Actor.Act(ActOptions.ToAll, "{0:N} drop{0:v} {1}.", Actor, item);
            item.ChangeContainer(Actor.Room);

            //
            if (item.ItemFlags.HasFlag(ItemFlags.MeltOnDrop))
            {
                Actor.Act(ActOptions.ToAll, "{0} dissolves into smoke.", item);
                ItemManager.RemoveItem(item);
            }

            return true;
        }
    }
}
