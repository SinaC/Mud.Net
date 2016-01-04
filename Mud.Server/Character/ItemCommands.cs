using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Mud.Server.Constants;
using Mud.Server.Helpers;
using Mud.Server.Input;
using Mud.Server.Item;

namespace Mud.Server.Character
{
    public partial class Character
    {
        [Command("wear")]
        [Command("wield")]
        [Command("hold")]
        protected virtual bool DoWear(string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length == 0)
                Send("Wear, wield, or hold what?" + Environment.NewLine);
            else if (rawParameters == "all")
            {
                // We have to clone list, because it'll be modified when wearing an item
                IReadOnlyCollection<IItem> clone = new ReadOnlyCollection<IItem>(Content.Where(x => CanSee(x) && x.IsWearable).ToList());
                foreach (IItem item in clone)
                    WearItem(item, false);
            }
            else
            {
                IItem item = FindHelpers.FindByName(Content, parameters[0]);
                if (item == null)
                    Send(StringHelpers.ItemInventoryNotFound);
                else
                    WearItem(item, true);
            }
            return true;
        }

        [Command("get")]
        // Get item
        // Get item [from] container
        // Get all
        // Get all.item
        // Get all [from] container
        // Get all.item [from] container
        protected virtual bool DoGet(string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length == 0)
                Send("Get what?" + Environment.NewLine);
            else if (parameters.Length == 1) // get item, get all, get all.
            {
                if (parameters[0].Value == "all" || parameters[0].Value.StartsWith("all.")) // get all or get all.
                {
                    // TODO: same code as below (***) except source collection (Room.Content)
                    IReadOnlyCollection<IItem> list;
                    bool allDot = false;
                    if (parameters[0].Value.Contains("."))
                    {
                        string what = parameters[0].Value.Substring(4);
                        list = !String.IsNullOrWhiteSpace(what)
                            ? new ReadOnlyCollection<IItem>(Room.Content.Where(x => CanSee(x) && FindHelpers.StringStartWith(x.Name, what)).ToList())
                            : new ReadOnlyCollection<IItem>(Room.Content.Where(CanSee).ToList());
                        allDot = true;
                    }
                    else
                        list = new ReadOnlyCollection<IItem>(Room.Content.Where(CanSee).ToList());
                    if (list.Any())
                    {
                        foreach (IItem item in list)
                            GetItem(item);
                    }
                    else if (allDot)
                        Send("I see nothing like that here."+Environment.NewLine);
                    else
                        Send("I see nothing here."+Environment.NewLine);
                }
                else // get item
                {
                    IItem item = FindHelpers.FindByName(Room.Content.Where(CanSee), parameters[0]);
                    if (item == null)
                        Send("I see no {0} here." + Environment.NewLine, parameters[0]);
                    else
                        GetItem(item);
                }
            }
            else // get item [from] container, get all [from] container, get all.item [from] container
            {
                CommandParameter whatParameter = parameters[0];
                CommandParameter whereParameter = parameters[1].Value == "from" ? parameters[2] : parameters[1];
                // search container
                IItem containerItem = FindHelpers.FindByName(Room.Content.Where(CanSee), whereParameter);
                if (containerItem == null)
                    Send("I see no {0} here." + Environment.NewLine, whereParameter);
                else
                {
                    IContainer container = containerItem as IContainer;
                    if (container == null)
                        Send("That's not a container." + Environment.NewLine);
                    else
                    {
                        // TODO: check if closed
                        if (whatParameter.Value == "all" || whatParameter.Value.StartsWith("all.")) // get all [from] container, get all.item [from] container
                        {
                            // TODO: same code as above (***) except source collection (container.Content)
                            IReadOnlyCollection<IItem> list;
                            bool allDot = false;
                            if (parameters[0].Value.Contains("."))
                            {
                                string what = parameters[0].Value.Substring(4);
                                list = !String.IsNullOrWhiteSpace(what)
                                    ? new ReadOnlyCollection<IItem>(container.Content.Where(x => CanSee(x) && FindHelpers.StringStartWith(x.Name, what)).ToList())
                                    : new ReadOnlyCollection<IItem>(container.Content.Where(CanSee).ToList());
                                allDot = true;
                            }
                            else
                                list = new ReadOnlyCollection<IItem>(container.Content.Where(CanSee).ToList());
                            if (list.Any())
                            {
                                foreach (IItem item in list)
                                    GetItem(item, container);
                            }
                            else if (allDot)
                                Send("I see nothing like that in the {0}." + Environment.NewLine, whereParameter);
                            else
                                Send("I see nothing in the {0}." + Environment.NewLine, whereParameter);
                        }
                        else // get item [from] container
                        {
                            IItem item = FindHelpers.FindByName(container.Content.Where(CanSee), whatParameter);
                            if (item == null)
                                Send("I see nothing like that in the {0}" + Environment.NewLine, whereParameter);
                            else
                                GetItem(item, container);
                        }
                    }
                }
            }
            return true;
        }

        [Command("drop")]
        // Drop item
        // Drop all
        // Drop all.item
        protected virtual bool DoDrop(string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length == 0)
                Send("Drop what?" + Environment.NewLine);
            else if (parameters[0].Value.StartsWith("all"))
                ; // TODO
            else
            {
                IItem item = FindHelpers.FindByName(Content.Where(CanSee), parameters[0]);
                if (item == null)
                    Send(StringHelpers.ItemInventoryNotFound);
                else
                {
                    Act(ActOptions.ToCharacter, "You drop {0}.", item);
                    Act(ActOptions.ToRoom, "{0} drops {1}.", this, item);
                    item.ChangeContainer(Room);
                }
            }
            return true;
        }

        [Command("remove")]
        // Remove item
        protected virtual bool DoRemove(string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length == 0)
                Send("Remove what?" + Environment.NewLine);
            else
            {
                EquipmentSlot equipmentSlot = FindHelpers.FindByName(Equipments.Where(x => x.Item != null && CanSee(x.Item)), x => x.Item, parameters[0]);
                if (equipmentSlot.Item == null)
                    Send(StringHelpers.ItemInventoryNotFound);
                else
                    RemoveItem(equipmentSlot);
            }
            return true;
        }

        //********************************************************************
        // Helpers
        //********************************************************************
        private bool WearItem(IItem item, bool replace) // equivalent to wear_obj in act_obj.C:1467
        {
            // TODO: check level
            WearLocations wearLocation = WearLocations.None;
            if (item is ItemLight)
                wearLocation = WearLocations.Light;
            else if (item is ItemWeapon)
                wearLocation = WearLocations.Wield;
            else if (item is ItemArmor)
                wearLocation = (item as ItemArmor).WearLocation;
            // TODO: other item type
            if (wearLocation == WearLocations.None)
            {
                if (replace) // replace means, only item is trying to be worn
                    Act(ActOptions.ToCharacter, "{0} cannot be worn.", item);
                return false;
            }
            EquipmentSlot equipmentSlot = Equipments.FirstOrDefault(x => x.WearLocation == wearLocation && (replace || x.Item == null));
            if (equipmentSlot == null)
            {
                if (replace) // replace means, only item is trying to be worn
                    Act(ActOptions.ToCharacter, "You cannot wear {0}.", item);
                return false;
            }
            // TODO: different phrase depending on wear location
            Act(ActOptions.ToCharacter, "You wear {0}.", item);
            Act(ActOptions.ToRoom, "{0} wears {1}.", this, item);
            equipmentSlot.Item = item; // equip
            item.ChangeContainer(null); // remove from inventory
            return true;
        }

        private bool GetItem(IItem item) // equivalent to get_obj in act_obj.C:211
        {
            // TODO: check weight + item count
            Act(ActOptions.ToCharacter, "You get {0}.", item);
            Act(ActOptions.ToRoom, "{0} gets {1}.", this, item);
            item.ChangeContainer(this);
            return true;
        }

        private bool GetItem(IItem item, IContainer container)
        {
            // TODO: check weight + item count
            Act(ActOptions.ToCharacter, "You get {0} from {1}.", item, container);
            Act(ActOptions.ToRoom, "{0} gets {1} from {2}.", this, item, container);
            item.ChangeContainer(this);
            return true;
        }

        private bool RemoveItem(EquipmentSlot equipmentSlot)
        {
            // TODO: check weight + item count
            Act(ActOptions.ToCharacter, "You stop using {0}.", equipmentSlot.Item);
            Act(ActOptions.ToRoom, "{0} stops using {1}.", this, equipmentSlot.Item);
            equipmentSlot.Item.ChangeContainer(this); // add in inventory
            equipmentSlot.Item = null; // unequip
            return true;
        }
    }
}
