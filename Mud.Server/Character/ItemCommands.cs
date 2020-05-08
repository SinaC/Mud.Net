using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Mud.Domain;
using Mud.Logger;
using Mud.Server.Common;
using Mud.Server.Helpers;
using Mud.Server.Input;
using Mud.Server.Item;
// ReSharper disable UnusedMember.Global

namespace Mud.Server.Character
{
    public partial class CharacterBase
    {
        [Command("wear", "Item")]
        [Syntax(
            "[cmd] <item>",
            "[cmd] all")]
        // Wear item
        // Wear all
        // Wear all.item
        protected virtual CommandExecutionResults DoWear(string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length == 0)
            {
                Send("Wear, wield, or hold what?");
                return CommandExecutionResults.SyntaxErrorNoDisplay;
            }
            // wear all, wear all.item
            if (parameters[0].IsAll)
            {
                CommandParameter whatParameter = parameters[0];
                // We have to clone list because it'll be modified when wearing an item
                IReadOnlyCollection<IEquipableItem> list; // list must be cloned because it'll be modified when wearing an item
                if (!string.IsNullOrWhiteSpace(whatParameter.Value)) // get all.item
                    list = new ReadOnlyCollection<IEquipableItem>(FindHelpers.FindAllByName(Inventory.Where(CanSee).OfType<IEquipableItem>(), whatParameter).ToList());
                else // get all
                    list = new ReadOnlyCollection<IEquipableItem>(Inventory.Where(CanSee).OfType<IEquipableItem>().ToList());
                bool itemEquipped = false;
                if (list.Any())
                {
                    foreach (IEquipableItem equipableItem in list)
                    {
                        if (WearItem(equipableItem, false))
                            itemEquipped = true;
                    }

                    if (itemEquipped)
                        Recompute();
                    return CommandExecutionResults.Ok;
                }
                Send(StringHelpers.ItemInventoryNotFound);
                return CommandExecutionResults.TargetNotFound;
            }
            // wear item
            IItem item = FindHelpers.FindByName(Inventory.Where(CanSee), parameters[0]);
            if (item == null)
            {
                Send(StringHelpers.ItemInventoryNotFound);
                return CommandExecutionResults.TargetNotFound;
            }
            IEquipableItem equipable = item as IEquipableItem;
            if (equipable == null)
            {
                Send("It cannot be equiped.");
                return CommandExecutionResults.InvalidTarget;
            }
            bool succeed = WearItem(equipable, true);
            if (succeed)
                Recompute();
            return CommandExecutionResults.Ok;
        }

        [Command("wield", "Item")]
        [Syntax("[cmd] <weapon>")]
        // Wield item
        protected virtual CommandExecutionResults DoWield(string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length == 0)
            {
                Send("Wield what?");
                return CommandExecutionResults.SyntaxErrorNoDisplay;
            }
            IItem item = FindHelpers.FindByName(Inventory.Where(CanSee), parameters[0]);
            if (item == null)
            {
                Send(StringHelpers.ItemInventoryNotFound);
                return CommandExecutionResults.TargetNotFound;
            }
            IEquipableItem equipable = item as IEquipableItem;
            if (equipable == null)
            {
                Send("It cannot be wielded.");
                return CommandExecutionResults.InvalidTarget;
            }
            if (!(item is IItemWeapon))
            {
                Send("Only weapons can be wielded.");
                return CommandExecutionResults.InvalidTarget;
            }
            //
            WearItem(equipable, true);
            Recompute();
            return CommandExecutionResults.Ok;
        }

        [Command("hold", "Item")]
        [Syntax("[cmd] <item>")]
        // Hold item
        protected virtual CommandExecutionResults DoHold(string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length == 0)
            {
                Send("Hold what?");
                return CommandExecutionResults.SyntaxErrorNoDisplay;
            }
            IItem item = FindHelpers.FindByName(Inventory.Where(CanSee), parameters[0]);
            if (item == null)
            {
                Send(StringHelpers.ItemInventoryNotFound);
                return CommandExecutionResults.TargetNotFound;
            }
            IEquipableItem equipable = item as IEquipableItem;
            if (equipable == null || equipable.WearLocation != WearLocations.Hold || equipable.WearLocation != WearLocations.Shield)
            {
                Send("It cannot be hold.");
                return CommandExecutionResults.InvalidTarget;
            }
            //
            WearItem(equipable, true);
            Recompute();
            return CommandExecutionResults.Ok;
        }

        [Command("remove", "Item")]
        [Syntax("[cmd] <item>")]
        // Remove item
        protected virtual CommandExecutionResults DoRemove(string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length == 0)
            {
                Send("Remove what?");
                return CommandExecutionResults.SyntaxErrorNoDisplay;
            }
            //
            EquipedItem equipmentSlot = FindHelpers.FindByName(Equipments.Where(x => x.Item != null && CanSee(x.Item)), x => x.Item, parameters[0]);
            if (equipmentSlot?.Item == null)
            {
                Send(StringHelpers.ItemInventoryNotFound);
                return CommandExecutionResults.TargetNotFound;
            }
            //
            bool removed = RemoveItem(equipmentSlot);
            if (!removed)
                return CommandExecutionResults.InvalidTarget;
            Recompute();
            return CommandExecutionResults.Ok;
        }

        [Command("get", "Item")]
        [Command("take", "Item")]
        [Syntax(
            "[cmd] <item>",
            "[cmd] <item> <container>")]
        // Get item
        // Get item [from] container
        // Get all
        // Get all.item
        // Get all [from] container
        // Get all.item [from] container
        protected virtual CommandExecutionResults DoGet(string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length == 0)
            {
                Send("Get what?");
                return CommandExecutionResults.SyntaxErrorNoDisplay;
            }
            CommandParameter whatParameter = parameters[0];
            // get item, get all, get all.item
            if (parameters.Length == 1)
            {
                // get all or get all.
                if (whatParameter.IsAll)
                {
                    // TODO: same code as below (***) except source collection (Room.Content)
                    IReadOnlyCollection<IItem> list; // list must be cloned because it'll be modified when getting an item
                    bool allDot = false;
                    if (!string.IsNullOrWhiteSpace(whatParameter.Value)) // get all.item
                    {
                        list = new ReadOnlyCollection<IItem>(FindHelpers.FindAllByName(Room.Content.Where(CanSee), whatParameter).ToList());
                        allDot = true;
                    }
                    else // get all
                        list = new ReadOnlyCollection<IItem>(Room.Content.Where(CanSee).ToList());
                    if (list.Any())
                    {
                        foreach (IItem itemInList in list)
                            GetItem(itemInList);
                        return CommandExecutionResults.Ok;
                    }
                    if (allDot)
                    {
                        Send("I see nothing like that here.");
                        return CommandExecutionResults.TargetNotFound;
                    }
                    Send("I see nothing here.");
                    return CommandExecutionResults.TargetNotFound;
                }
                // get item
                IItem itemInRoom = FindHelpers.FindByName(Room.Content.Where(CanSee), parameters[0]);
                if (itemInRoom == null)
                {
                    Send("I see no {0} here.", parameters[0]);
                    return CommandExecutionResults.TargetNotFound;
                }
                GetItem(itemInRoom);
                return CommandExecutionResults.Ok;
            }
            // get item [from] container, get all [from] container, get all.item [from] container
            CommandParameter whereParameter = StringCompareHelpers.StringEquals(parameters[1].Value, "from") ? parameters[2] : parameters[1];
            if (whereParameter.IsAll)
            {
                Send("You can't do that");
                return CommandExecutionResults.InvalidParameter;
            }
            // search container
            IItem containerItem = FindHelpers.FindItemHere(this, whereParameter);
            if (containerItem == null)
            {
                Send("I see no {0} here.", whereParameter);
                return CommandExecutionResults.TargetNotFound;
            }
            IContainer container = containerItem as IContainer;
            if (container == null)
            {
                Send("That's not a container.");
                return CommandExecutionResults.InvalidTarget;
            }
            // TODO: check if closed
            if (whatParameter.IsAll) // get all [from] container, get all.item [from] container
            {
                // TODO: same code as above (***) except source collection (container.Content)
                IReadOnlyCollection<IItem> list; // list must be cloned because it'll be modified when getting an item
                bool allDot = false;
                if (!string.IsNullOrWhiteSpace(whatParameter.Value)) // get all.item [from] container
                {
                    list = new ReadOnlyCollection<IItem>(FindHelpers.FindAllByName(container.Content.Where(CanSee), whatParameter).ToList());
                    allDot = true;
                }
                else // get all [from] container
                    list = new ReadOnlyCollection<IItem>(container.Content.Where(CanSee).ToList());
                if (list.Any())
                {
                    foreach (IItem itemInList in list)
                        GetItem(itemInList, container);
                    return CommandExecutionResults.Ok;
                }
                if (allDot)
                {
                    Send("I see nothing like that in the {0}.", whereParameter);
                    return CommandExecutionResults.TargetNotFound;
                }
                Send("I see nothing in the {0}.", whereParameter);
                return CommandExecutionResults.TargetNotFound;
            }
            // get item [from] container
            IItem item = FindHelpers.FindByName(container.Content.Where(CanSee), whatParameter);
            if (item == null)
            {
                Send("I see nothing like that in the {0}.", whereParameter);
                return CommandExecutionResults.TargetNotFound;
            }
            GetItem(item, container);
            return CommandExecutionResults.Ok;
        }

        [Command("drop", "Item")]
        [Syntax(
            "[cmd] <item>",
            "[cmd] all")]
        // Drop item
        // Drop all
        // Drop all.item
        protected virtual CommandExecutionResults DoDrop(string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length == 0)
            {
                Send("Drop what?");
                return CommandExecutionResults.SyntaxErrorNoDisplay;
            }
            // drop all, drop all.item
            if (parameters[0].IsAll)
            {
                CommandParameter whatParameter = parameters[0];
                IReadOnlyCollection<IItem> list; // list must be cloned because it'll be modified when dropping an item
                if (!string.IsNullOrWhiteSpace(whatParameter.Value)) // drop all.item
                    list = new ReadOnlyCollection<IItem>(FindHelpers.FindAllByName(Inventory.Where(x => CanSee(x) && !(x is ItemQuest)), whatParameter).ToList());
                else // drop all
                    list = new ReadOnlyCollection<IItem>(Inventory.Where(x => CanSee(x) && !(x is ItemQuest)).ToList());
                if (list.Any())
                {
                    foreach (IItem itemInList in list)
                        DropItem(itemInList);
                    return CommandExecutionResults.Ok;
                }
                else
                {
                    Send(StringHelpers.ItemInventoryNotFound);
                    return CommandExecutionResults.TargetNotFound;
                }
            }
            // drop item
            IItem item = FindHelpers.FindByName(Inventory.Where(CanSee), parameters[0]);
            if (item == null)
            {
                Send(StringHelpers.ItemInventoryNotFound);
                return CommandExecutionResults.TargetNotFound;
            }
            if (item is ItemQuest)
            {
                Act(ActOptions.ToCharacter, "You cannot drop quest items.");
                return CommandExecutionResults.InvalidTarget;
            }
            DropItem(item);
            return CommandExecutionResults.Ok;
        }

        [Command("give", "Item")]
        [Syntax("[cmd] <item> <character>")]
        // Give item victim
        protected virtual CommandExecutionResults DoGive(string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length < 2)
            {
                Send("Give what to whom?");
                return CommandExecutionResults.SyntaxErrorNoDisplay;
            }

            IItem what = FindHelpers.FindByName(Inventory.Where(CanSee), parameters[0]);
            if (what == null)
            {
                Send(StringHelpers.ItemInventoryNotFound);
                return CommandExecutionResults.TargetNotFound;
            }

            ICharacter whom = FindHelpers.FindByName(Room.People.Where(CanSee), parameters[1]);
            if (whom == null)
            {
                Send(StringHelpers.CharacterNotFound);
                return CommandExecutionResults.TargetNotFound;
            }

            if (!whom.CanSee(what))
            {
                Act(ActOptions.ToCharacter, "{0:n} can't see it.", whom);
                return CommandExecutionResults.InvalidTarget;
            }

            if (what is ItemQuest)
            {
                Act(ActOptions.ToCharacter, "You cannot give quest items.");
                return CommandExecutionResults.InvalidTarget;
            }

            // Give item to victim
            what.ChangeContainer(whom);
            whom.Recompute();
            Recompute();

            ActToNotVictim(whom, "{0} gives {1} to {2}.", this, what, whom);
            whom.Act(ActOptions.ToCharacter, "{0} gives you {1}.", this, what);
            Act(ActOptions.ToCharacter, "You give {0} to {1}.", what, whom);

            return CommandExecutionResults.Ok;
        }

        [Command("put", "Item")]
        [Syntax("[cmd] <item> <container>")]
        // Put item container
        // Put item [in] container
        // Put all.item container
        // Put all.item [in] container
        protected virtual CommandExecutionResults DoPut(string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length < 2)
            {
                Send("Put what in what?");
                return CommandExecutionResults.SyntaxErrorNoDisplay;
            }

            // Extract parameters
            CommandParameter whatParameter = parameters[0];
            CommandParameter whereParameter = StringCompareHelpers.StringEquals(parameters[1].Value, "in") ? parameters[2] : parameters[1];

            // search container
            if (whereParameter.IsAll)
            {
                Send("You can't do that");
                return CommandExecutionResults.InvalidParameter;
            }
            IItem where = FindHelpers.FindItemHere(this, whereParameter);
            if (where == null)
            {
                Send(StringHelpers.ItemNotFound);
                return CommandExecutionResults.TargetNotFound;
            }
            IContainer container = where as IContainer;
            if (container == null)
            {
                Send("That's not a container.");
                return CommandExecutionResults.InvalidTarget;
            }

            // TODO: check if container is closed
            if (whatParameter.IsAll) // put all [in] container, put all.item [in] container
            {
                // TODO: same code as above (***) except source collection (container.Content)
                IReadOnlyCollection<IItem> list; // list must be cloned because it'll be modified when putting an item
                if (!string.IsNullOrWhiteSpace(whatParameter.Value)) // put all.item [in] container
                    list = new ReadOnlyCollection<IItem>(FindHelpers.FindAllByName(Inventory.Where(CanSee), whatParameter).ToList());
                else // put all [in] container
                    list = new ReadOnlyCollection<IItem>(Inventory.Where(CanSee).ToList());
                if (list.Any())
                {
                    foreach (IItem itemInList in list)
                        PutItem(itemInList, container);
                    return CommandExecutionResults.Ok;
                }
                else
                {
                    Send(StringHelpers.ItemInventoryNotFound);
                    return CommandExecutionResults.TargetNotFound;
                }
            }
            // put item [in] container
            IItem item = FindHelpers.FindByName(Inventory.Where(CanSee), whatParameter);
            if (item == null)
            {
                Send(StringHelpers.ItemInventoryNotFound);
                return CommandExecutionResults.TargetNotFound;
            }
            PutItem(item, container);
            return CommandExecutionResults.Ok;
        }

        //********************************************************************
        // Helpers
        //********************************************************************
        private bool WearItem(IEquipableItem item, bool replace) // equivalent to wear_obj in act_obj.C:1467
        {
            // TODO: check level
            WearLocations wearLocation = item.WearLocation;

            if (wearLocation == WearLocations.None)
            {
                Log.Default.WriteLine(LogLevels.Warning, "Item {0} cannot be equiped", item.DebugName);
                if (replace) // replace means, only item is trying to be worn
                    Act(ActOptions.ToCharacter, "{0} cannot be worn.", item);
                return false;
            }
            EquipedItem equipmentSlot = SearchEquipmentSlot(item, replace);
            if (equipmentSlot == null)
            {
                if (replace) // we dont' want to spam if character is trying to wear all, replace is set to true only when wearing one item
                    Act(ActOptions.ToCharacter, "You cannot wear {0}.", item);
                return false;
            }
            if (replace && equipmentSlot.Item != null)
            {
                IEquipableItem removeItem = equipmentSlot.Item;
                Act(ActOptions.ToAll, "{0:N} remove{0:v} {1}.", this, removeItem);
                //equipmentSlot.Item = null  already done by ChangeEquipedBy
                removeItem.ChangeEquipedBy(null);
                removeItem.ChangeContainer(this);
            }
            // TODO: different phrase depending on wear location
            Act(ActOptions.ToAll, "{0:N} wear{0:v} {1}.", this, item);
            equipmentSlot.Item = item; // equip
            item.ChangeContainer(null); // remove from inventory
            item.ChangeEquipedBy(this); // set as equiped by this
            return true;
        }

        private bool DropItem(IItem item)
        {
            //
            if (item.ItemFlags.HasFlag(ItemFlags.NoDrop))
            {
                Send("You can't let go of it.");
                return false;
            }

            //
            Act(ActOptions.ToAll, "{0:N} drop{0:v} {1}.", this, item);
            item.ChangeContainer(Room);

            //
            if (item.ItemFlags.HasFlag(ItemFlags.MeltOnDrop))
            {
                Act(ActOptions.ToAll, "{0} dissolves into smoke.", item);
                World.RemoveItem(item);
            }

            return true;
        }

        private bool GetItem(IItem item) // equivalent to get_obj in act_obj.C:211
        {
            //
            if (item.NoTake)
            {
                Send("You can't take that.");
                return false;
            }

            // TODO: check if someone is using it as Furniture
            // TODO: check weight + item count
            Act(ActOptions.ToAll, "{0:N} get{0:v} {1}.", this, item);
            item.ChangeContainer(this);
            return true;
        }

        private bool GetItem(IItem item, IContainer container)
        {           
            //
            if (item.NoTake)
            {
                Send("You can't take that.");
                return false;
            }

            // TODO: check weight + item count
            Act(ActOptions.ToAll, "{0:N} get{0:v} {1} from {2}.", this, item, container);
            item.ChangeContainer(this);
            return true;
        }

        private bool RemoveItem(EquipedItem equipmentSlot)
        {
            //
            if (equipmentSlot.Item.ItemFlags.HasFlag(ItemFlags.NoRemove))
            {
                Act(ActOptions.ToCharacter, "You cannot remove {0}.", equipmentSlot.Item);
                return false;
            }

            // TODO: check weight + item count
            Act(ActOptions.ToAll, "{0:N} stop{0:v} using {1}.", this, equipmentSlot.Item);
            equipmentSlot.Item.ChangeContainer(this); // add in inventory
            equipmentSlot.Item.ChangeEquipedBy(null); // clear equiped by
            equipmentSlot.Item = null; // unequip
            return true;
        }

        private bool PutItem(IItem item, IContainer container)
        {//
            if (item.ItemFlags.HasFlag(ItemFlags.NoDrop))
            {
                Send("You can't let go of it.");
                return false;
            }

            // TODO: check weight + item count
            Act(ActOptions.ToAll, "{0:N} put{0:v} {1} in {2}.", this, item, container);
            item.ChangeContainer(container);
            return true;
        }
    }
}
