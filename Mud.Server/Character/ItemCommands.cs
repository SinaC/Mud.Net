﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Mud.Domain;
using Mud.Logger;
using Mud.Server.Helpers;
using Mud.Server.Input;
using Mud.Server.Item;

namespace Mud.Server.Character
{
    public partial class CharacterBase
    {
        [Command("wear", Category = "Item")]
        // Wear item
        // Wear all
        // Wear all.item
        protected virtual bool DoWear(string rawParameters, params CommandParameter[] parameters)
        {
            // TODO: bug with shield + second hand
            if (parameters.Length == 0)
            {
                Send("Wear, wield, or hold what?");
                return true;
            }
            // wear all, wear all.item
            if (parameters[0].IsAll)
            {
                CommandParameter whatParameter = parameters[0];
                // We have to clone list because it'll be modified when wearing an item
                IReadOnlyCollection<IEquipable> list; // list must be cloned because it'll be modified when wearing an item
                if (!string.IsNullOrWhiteSpace(whatParameter.Value)) // get all.item
                    list = new ReadOnlyCollection<IEquipable>(FindHelpers.FindAllByName(Content.Where(CanSee).OfType<IEquipable>(), whatParameter).ToList());
                else // get all
                    list = new ReadOnlyCollection<IEquipable>(Content.Where(CanSee).OfType<IEquipable>().ToList());
                bool itemEquipped = false;
                if (list.Any())
                {
                    foreach (IEquipable equipableItem in list)
                    {
                        if (WearItem(equipableItem, false))
                            itemEquipped = true;
                    }

                    if (itemEquipped)
                        RecomputeAttributes();
                }
                else
                    Send(StringHelpers.ItemInventoryNotFound);
                return true;
            }
            // wear item
            IItem item = FindHelpers.FindByName(Content.Where(CanSee), parameters[0]);
            if (item == null)
            {
                Send(StringHelpers.ItemInventoryNotFound);
                return true;
            }
            IEquipable equipable = item as IEquipable;
            if (equipable == null)
            {
                Send("It cannot be equiped.");
                return true;
            }
            WearItem(equipable, true);
            RecomputeAttributes();
            return true;
        }

        [Command("wield", Category = "Item")]
        // Wield item
        protected virtual bool DoWield(string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length == 0)
            {
                Send("Wield what?");
                return true;
            }
            IItem item = FindHelpers.FindByName(Content.Where(CanSee), parameters[0]);
            if (item == null)
            {
                Send(StringHelpers.ItemInventoryNotFound);
                return true;
            }
            IEquipable equipable = item as IEquipable;
            if (equipable == null)
            {
                Send("It cannot be wielded.");
                return true;
            }
            if (!(item is IItemWeapon))
            {
                Send("Only weapons can be wielded.");
                return true;
            }
            //
            WearItem(equipable, true);
            RecomputeAttributes();
            return true;
        }

        [Command("hold", Category = "Item")]
        // Hold item
        protected virtual bool DoHold(string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length == 0)
            {
                Send("Hold what?");
                return true;
            }
            IItem item = FindHelpers.FindByName(Content.Where(CanSee), parameters[0]);
            if (item == null)
            {
                Send(StringHelpers.ItemInventoryNotFound);
                return true;
            }
            IEquipable equipable = item as IEquipable;
            if (equipable == null || equipable.WearLocation != WearLocations.Hold || equipable.WearLocation != WearLocations.Shield)
            {
                Send("It cannot be hold.");
                return true;
            }
            //
            WearItem(equipable, true);
            RecomputeAttributes();
            return true;
        }

        [Command("remove", Category = "Item")]
        // Remove item
        protected virtual bool DoRemove(string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length == 0)
            {
                Send("Remove what?");
                return true;
            }
            //
            EquipedItem equipmentSlot = FindHelpers.FindByName(Equipments.Where(x => x.Item != null && CanSee(x.Item)), x => x.Item, parameters[0]);
            if (equipmentSlot?.Item == null)
            {
                Send(StringHelpers.ItemInventoryNotFound);
                return true;
            }
            //
            RemoveItem(equipmentSlot);
            RecomputeAttributes();
            return true;
        }

        [Command("get", Category = "Item")]
        // Get item
        // Get item [from] container
        // Get all
        // Get all.item
        // Get all [from] container
        // Get all.item [from] container
        protected virtual bool DoGet(string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length == 0)
            {
                Send("Get what?");
                return true;
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
                        return true;
                    }
                    if (allDot)
                    {
                        Send("I see nothing like that here.");
                        return true;
                    }
                    Send("I see nothing here.");
                    return true;
                }
                // get item
                IItem itemInRoom = FindHelpers.FindByName(Room.Content.Where(CanSee), parameters[0]);
                if (itemInRoom == null)
                {
                    Send("I see no {0} here.", parameters[0]);
                    return true;
                }
                GetItem(itemInRoom);
                return true;
            }
            // get item [from] container, get all [from] container, get all.item [from] container
            CommandParameter whereParameter = FindHelpers.StringEquals(parameters[1].Value, "from") ? parameters[2] : parameters[1];
            if (whereParameter.IsAll)
            {
                Send("You can't do that");
                return true;
            }
            // search container
            IItem containerItem = FindHelpers.FindItemHere(this, whereParameter);
            if (containerItem == null)
            {
                Send("I see no {0} here.", whereParameter);
                return true;
            }
            IContainer container = containerItem as IContainer;
            if (container == null)
            {
                Send("That's not a container.");
                return true;
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
                    return true;
                }
                if (allDot)
                {
                    Send("I see nothing like that in the {0}.", whereParameter);
                    return true;
                }
                Send("I see nothing in the {0}.", whereParameter);
                return true;
            }
            // get item [from] container
            IItem item = FindHelpers.FindByName(container.Content.Where(CanSee), whatParameter);
            if (item == null)
            {
                Send("I see nothing like that in the {0}.", whereParameter);
                return true;
            }
            GetItem(item, container);
            return true;
        }

        [Command("drop", Category = "Item")]
        // Drop item
        // Drop all
        // Drop all.item
        protected virtual bool DoDrop(string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length == 0)
            {
                Send("Drop what?");
                return true;
            }
            // drop all, drop all.item
            if (parameters[0].IsAll)
            {
                CommandParameter whatParameter = parameters[0];
                IReadOnlyCollection<IItem> list; // list must be cloned because it'll be modified when dropping an item
                if (!string.IsNullOrWhiteSpace(whatParameter.Value)) // drop all.item
                    list = new ReadOnlyCollection<IItem>(FindHelpers.FindAllByName(Content.Where(x => CanSee(x) && !(x is ItemQuest)), whatParameter).ToList());
                else // drop all
                    list = new ReadOnlyCollection<IItem>(Content.Where(x => CanSee(x) && !(x is ItemQuest)).ToList());
                if (list.Any())
                {
                    foreach (IItem itemInList in list)
                        DropItem(itemInList);
                }
                else
                    Send(StringHelpers.ItemInventoryNotFound);
                return true;
            }
            // drop item
            IItem item = FindHelpers.FindByName(Content.Where(CanSee), parameters[0]);
            if (item == null)
            {
                Send(StringHelpers.ItemInventoryNotFound);
                return true;
            }
            if (item is ItemQuest)
            {
                Act(ActOptions.ToCharacter, "You cannot drop quest items.");
                return true;
            }
            DropItem(item);
            return true;
        }

        [Command("give", Category = "Item")]
        // Give item victim
        protected virtual bool DoGive(string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length < 2)
            {
                Send("Give what to whom?");
                return true;
            }

            IItem what = FindHelpers.FindByName(Content.Where(CanSee), parameters[0]);
            if (what == null)
            {
                Send(StringHelpers.ItemInventoryNotFound);
                return true;
            }

            ICharacter whom = FindHelpers.FindByName(Room.People.Where(CanSee), parameters[1]);
            if (whom == null)
            {
                Send(StringHelpers.CharacterNotFound);
                return true;
            }

            if (!whom.CanSee(what))
            {
                Act(ActOptions.ToCharacter, "{0:n} can't see it.", whom);
                return true;
            }

            if (what is ItemQuest)
            {
                Act(ActOptions.ToCharacter, "You cannot give quest items.");
                return true;
            }

            // Give item to victim
            what.ChangeContainer(whom);
            whom.RecomputeAttributes();
            RecomputeAttributes();

            ActToNotVictim(whom, "{0} gives {1} to {2}.", this, what, whom);
            whom.Act(ActOptions.ToCharacter, "{0} gives you {1}.", this, what);
            Act(ActOptions.ToCharacter, "You give {0} to {1}.", what, whom);

            return true;
        }

        [Command("put", Category = "Item")]
        // Put item container
        // Put item [in] container
        // Put all.item container
        // Put all.item [in] container
        protected virtual bool DoPut(string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length < 2)
            {
                Send("Put what in what?");
                return true;
            }

            // Extract parameters
            CommandParameter whatParameter = parameters[0];
            CommandParameter whereParameter = FindHelpers.StringEquals(parameters[1].Value, "in") ? parameters[2] : parameters[1];

            // search container
            if (whereParameter.IsAll)
            {
                Send("You can't do that");
                return true;
            }
            IItem where = FindHelpers.FindItemHere(this, whereParameter);
            if (where == null)
            {
                Send(StringHelpers.ItemNotFound);
                return true;
            }
            IContainer container = where as IContainer;
            if (container == null)
            {
                Send("That's not a container.");
                return true;
            }

            // TODO: check if container is closed
            if (whatParameter.IsAll) // put all [in] container, put all.item [in] container
            {
                // TODO: same code as above (***) except source collection (container.Content)
                IReadOnlyCollection<IItem> list; // list must be cloned because it'll be modified when putting an item
                if (!string.IsNullOrWhiteSpace(whatParameter.Value)) // put all.item [in] container
                    list = new ReadOnlyCollection<IItem>(FindHelpers.FindAllByName(Content.Where(CanSee), whatParameter).ToList());
                else // put all [in] container
                    list = new ReadOnlyCollection<IItem>(Content.Where(CanSee).ToList());
                if (list.Any())
                {
                    foreach (IItem itemInList in list)
                        PutItem(itemInList, container);
                }
                else
                    Send(StringHelpers.ItemInventoryNotFound);
                return true;
            }
            // put item [in] container
            IItem item = FindHelpers.FindByName(Content.Where(CanSee), whatParameter);
            if (item == null)
            {
                Send(StringHelpers.ItemInventoryNotFound);
                return true;
            }
            PutItem(item, container);
            return true;
        }

        //********************************************************************
        // Helpers
        //********************************************************************
        private bool WearItem(IEquipable item, bool replace) // equivalent to wear_obj in act_obj.C:1467
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
                IEquipable removeItem = equipmentSlot.Item;
                //Act(ActOptions.ToCharacter, "You remove {0}.", removeItem);
                //Act(ActOptions.ToRoom, "{0} removes {1}.", this, removeItem);
                Act(ActOptions.ToAll, "{0:N} remove{0:v} {1}.", this, removeItem);
                //equipmentSlot.Item = null  already done by ChangeEquipedBy
                removeItem.ChangeEquipedBy(null);
                removeItem.ChangeContainer(this);
            }
            // TODO: different phrase depending on wear location
            //Act(ActOptions.ToCharacter, "You wear {0}.", item);
            //Act(ActOptions.ToRoom, "{0} wears {1}.", this, item);
            Act(ActOptions.ToAll, "{0:N} wear{0:v} {1}.", this, item);
            equipmentSlot.Item = item; // equip
            item.ChangeContainer(null); // remove from inventory
            item.ChangeEquipedBy(this); // set as equiped by this
            return true;
        }

        private bool DropItem(IItem item)
        {
            //Act(ActOptions.ToCharacter, "You drop {0}.", item);
            //Act(ActOptions.ToRoom, "{0} drops {1}.", this, item);
            Act(ActOptions.ToAll, "{0:N} drop{0:v} {1}.", this, item);
            item.ChangeContainer(Room);
            return true;
        }

        private bool GetItem(IItem item) // equivalent to get_obj in act_obj.C:211
        {
            // TODO: check if someone is using it as Furniture
            // TODO: check weight + item count
            //Act(ActOptions.ToCharacter, "You get {0}.", item);
            //Act(ActOptions.ToRoom, "{0} gets {1}.", this, item);
            Act(ActOptions.ToAll, "{0:N} get{0:v} {1}.", this, item);
            item.ChangeContainer(this);
            return true;
        }

        private bool GetItem(IItem item, IContainer container)
        {
            // TODO: check weight + item count
            //Act(ActOptions.ToCharacter, "You get {0} from {1}.", item, container);
            //Act(ActOptions.ToRoom, "{0} gets {1} from {2}.", this, item, container);
            Act(ActOptions.ToAll, "{0:N} get{0:v} {1} from {2}.", this, item, container);
            item.ChangeContainer(this);
            return true;
        }

        private bool RemoveItem(EquipedItem equipmentSlot)
        {
            // TODO: check weight + item count
            //Act(ActOptions.ToCharacter, "You stop using {0}.", equipmentSlot.Item);
            //Act(ActOptions.ToRoom, "{0} stops using {1}.", this, equipmentSlot.Item);
            Act(ActOptions.ToAll, "{0:N} stop{0:v} using {1}.", this, equipmentSlot.Item);
            equipmentSlot.Item.ChangeContainer(this); // add in inventory
            equipmentSlot.Item.ChangeEquipedBy(null); // clear equiped by
            equipmentSlot.Item = null; // unequip
            return true;
        }

        private bool PutItem(IItem item, IContainer container)
        {
            // TODO: check weight + item count
            Act(ActOptions.ToAll, "{0:N} put{0:v} {1} in {2}.", this, item, container);
            item.ChangeContainer(container);
            return true;
        }
    }
}
