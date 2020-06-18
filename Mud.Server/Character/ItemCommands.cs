using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Mud.Common;
using Mud.Domain;
using Mud.Logger;
using Mud.Server.Ability.Spell;
using Mud.Server.Affects;
using Mud.Server.Blueprints.Character;
using Mud.Server.Common;
using Mud.Server.GameAction;
using Mud.Server.Helpers;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Entity;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Item;
using Mud.Server.Item;
using Mud.Server.Rom24.Affects;

namespace Mud.Server.Character
{
    public partial class CharacterBase
    {
        [CharacterCommand("wield", "Item", "Equipment", MinPosition = Positions.Resting)]
        [Syntax("[cmd] <weapon>")]
        // Wield item
        protected virtual CommandExecutionResults DoWield(string rawParameters, params ICommandParameter[] parameters)
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
            if (item.WearLocation == WearLocations.None)
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
            WearItem(item, true);
            Recompute();
            return CommandExecutionResults.Ok;
        }

        [CharacterCommand("hold", "Item", "Equipment", MinPosition = Positions.Resting)]
        [Syntax("[cmd] <item>")]
        // Hold item
        protected virtual CommandExecutionResults DoHold(string rawParameters, params ICommandParameter[] parameters)
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
            if (item.WearLocation != WearLocations.Hold && item.WearLocation != WearLocations.Shield && item.WearLocation != WearLocations.Light)
            {
                Send("It cannot be hold.");
                return CommandExecutionResults.InvalidTarget;
            }
            //
            WearItem(item, true);
            Recompute();
            return CommandExecutionResults.Ok;
        }

        [CharacterCommand("remove", "Item", "Equipment", MinPosition = Positions.Resting)]
        [Syntax("[cmd] <item>")]
        // Remove item
        protected virtual CommandExecutionResults DoRemove(string rawParameters, params ICommandParameter[] parameters)
        {
            if (parameters.Length == 0)
            {
                Send("Remove what?");
                return CommandExecutionResults.SyntaxErrorNoDisplay;
            }
            //
            IEquippedItem equippedItem = FindHelpers.FindByName(Equipments.Where(x => x.Item != null && CanSee(x.Item)), x => x.Item, parameters[0]);
            if (equippedItem?.Item == null)
            {
                Send(StringHelpers.ItemInventoryNotFound);
                return CommandExecutionResults.TargetNotFound;
            }
            //
            bool removed = RemoveItem(equippedItem);
            if (!removed)
                return CommandExecutionResults.InvalidTarget;
            Recompute();
            return CommandExecutionResults.Ok;
        }

        [CharacterCommand("get", "Item", "Inventory", MinPosition = Positions.Resting)]
        [CharacterCommand("take", "Item", "Inventory", MinPosition = Positions.Resting)]
        [Syntax(
            "[cmd] <item>",
            "[cmd] <item> <container>")]
        // Get item
        // Get item [from] container
        // Get all
        // Get all.item
        // Get all [from] container
        // Get all.item [from] container
        protected virtual CommandExecutionResults DoGet(string rawParameters, params ICommandParameter[] parameters)
        {
            if (parameters.Length == 0)
            {
                Send("Get what?");
                return CommandExecutionResults.SyntaxErrorNoDisplay;
            }
            ICommandParameter whatParameter = parameters[0];
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
                            GetItem(itemInList, null);
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
                GetItem(itemInRoom, null);
                return CommandExecutionResults.Ok;
            }
            // get item [from] container, get all [from] container, get all.item [from] container
            ICommandParameter whereParameter = StringCompareHelpers.StringEquals(parameters[1].Value, "from") ? parameters[2] : parameters[1];
            if (whereParameter.IsAll)
            {
                Send("You can't do that");
                return CommandExecutionResults.InvalidParameter;
            }
            // search container
            IItem targetItem = FindHelpers.FindItemHere(this, whereParameter);
            if (targetItem == null)
            {
                Send("I see no {0} here.", whereParameter);
                return CommandExecutionResults.TargetNotFound;
            }
            IContainer container = targetItem as IContainer;
            if (container == null)
            {
                Send("That's not a container.");
                return CommandExecutionResults.InvalidTarget;
            }
            if (container is ICloseable closeable && closeable.IsClosed)
            {
                Act(ActOptions.ToCharacter, "The {0} is closed.", container);
                return CommandExecutionResults.InvalidTarget;
            }
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


        //********************************************************************
        // Helpers
        //********************************************************************
       

        

        protected virtual bool GetItem(IItem item, IContainer container) // equivalent to get_obj in act_obj.C:211
        {
            //
            if (item.NoTake)
            {
                Send("You can't take that.");
                return false;
            }
            if (CarryNumber + item.CarryCount > MaxCarryNumber)
            {
                Act(ActOptions.ToCharacter, "{0:N}: you can't carry that many items.", item);
                return false;
            }
            if (CarryWeight + item.TotalWeight > MaxCarryWeight)
            {
                Act(ActOptions.ToCharacter, "{0:N}: you can't carry that much weight.", item);
                return false;
            }

            // TODO: from pit ?
            if (container != null)
                Act(ActOptions.ToAll, "{0:N} get{0:v} {1} from {2}.", this, item, container);
            else
                Act(ActOptions.ToAll, "{0:N} get{0:v} {1}.", this, item);

            if (item is IItemMoney money)
            {
                UpdateMoney(money.SilverCoins, money.GoldCoins);
                ItemManager.RemoveItem(money);
            }
            else
                item.ChangeContainer(this);
            return true;
        }

        protected virtual bool RemoveItem(IEquippedItem equipmentSlot)
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
            equipmentSlot.Item.ChangeEquippedBy(null, false); // clear equipped by
            equipmentSlot.Item = null; // unequip TODO: remove it's already done in Unequip
            // no need to recompute, because it's being done by caller
            return true;
        }

    }
}
