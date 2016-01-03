using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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
        // Get all [from] container
        // Get all.item [from] container
        protected virtual bool DoGet(string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length == 0)
                Send("Get what?" + Environment.NewLine);
            else if (parameters.Length == 1) // Get item
            {
                IItem item = FindHelpers.FindByName(Room.Content, parameters[0]);
                if (item == null)
                    Send("I see no {0} here", parameters[0]);
                else
                    GetItem(item);
            }
            else
                // TODO: other cases
                ;
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
                IItem item = FindHelpers.FindByName(Content, parameters[0]);
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
                EquipmentSlot equipmentSlot = FindHelpers.FindByName(Equipments.Where(x => x.Item != null), x => x.Item, parameters[0]);
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
            Act(ActOptions.ToRoom, "$n gets $p.", this, item);
            item.ChangeContainer(this);
            return true;
        }

        private bool GetItem(IItem item, ItemContainer container)
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
