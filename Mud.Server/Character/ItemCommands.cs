﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Mud.Domain;
using Mud.Logger;
using Mud.Server.Aura;
using Mud.Server.Common;
using Mud.Server.Helpers;
using Mud.Server.Input;
using Mud.Server.Item;
// ReSharper disable UnusedMember.Global

namespace Mud.Server.Character
{
    public partial class CharacterBase
    {
        [CharacterCommand("wear", "Item", "Equipment", MinPosition = Positions.Resting)]
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
                IReadOnlyCollection<IItem> list = !string.IsNullOrWhiteSpace(whatParameter.Value)
                    // get all.item
                    ? new ReadOnlyCollection<IItem>(FindHelpers.FindAllByName(Inventory.Where(CanSee), whatParameter).ToList())
                    // get all
                    : new ReadOnlyCollection<IItem>(Inventory.Where(CanSee).ToList());
                bool itemEquipped = false;
                if (list.Any())
                {
                    foreach (IItem equippableItem in list)
                    {
                        if (WearItem(equippableItem, false))
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
            bool succeed = WearItem(item, true);
            if (succeed)
                Recompute();
            return CommandExecutionResults.Ok;
        }

        [CharacterCommand("wield", "Item", "Equipment", MinPosition = Positions.Resting)]
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
            if (item.WearLocation != WearLocations.Hold && item.WearLocation != WearLocations.Shield)
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
        protected virtual CommandExecutionResults DoRemove(string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length == 0)
            {
                Send("Remove what?");
                return CommandExecutionResults.SyntaxErrorNoDisplay;
            }
            //
            EquippedItem equipmentSlot = FindHelpers.FindByName(Equipments.Where(x => x.Item != null && CanSee(x.Item)), x => x.Item, parameters[0]);
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
            CommandParameter whereParameter = StringCompareHelpers.StringEquals(parameters[1].Value, "from") ? parameters[2] : parameters[1];
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

        [CharacterCommand("drop", "Item", "Inventory", MinPosition = Positions.Resting)]
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
                // list must be cloned because it'll be modified when dropping an item
                IReadOnlyCollection<IItem> list = !string.IsNullOrWhiteSpace(whatParameter.Value)
                    // drop all.item
                    ? new ReadOnlyCollection<IItem>(FindHelpers.FindAllByName(Inventory.Where(x => CanSee(x) && !(x is ItemQuest)), whatParameter).ToList()) 
                    // drop all
                    : new ReadOnlyCollection<IItem>(Inventory.Where(x => CanSee(x) && !(x is ItemQuest)).ToList());
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

        [CharacterCommand("give", "Item", "Equipment", MinPosition = Positions.Resting)]
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

            if (what.ItemFlags.HasFlag(ItemFlags.NoDrop))
            {
                Send("You can't let go of it.");
                return CommandExecutionResults.InvalidTarget;
            }

            if (whom.CarryNumber + what.CarryCount > whom.MaxCarryNumber)
            {
                Act(ActOptions.ToCharacter, "{0:N} has {0:s} hands full.", whom);
                return CommandExecutionResults.InvalidTarget;
            }

            if (whom.CarryWeight + what.TotalWeight > whom.MaxCarryWeight)
            {
                Act(ActOptions.ToCharacter, "{0:N} can't carry that much weight.", whom);
                return CommandExecutionResults.InvalidTarget;
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

        [CharacterCommand("put", "Item", "Equipment", MinPosition = Positions.Resting)]
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
            IItemContainer container = where as IItemContainer;
            if (container == null)
            {
                Send("That's not a container.");
                return CommandExecutionResults.InvalidTarget;
            }
            if (container.IsClosed)
            {
                Act(ActOptions.ToCharacter, "The {0} is closed.", container);
                return CommandExecutionResults.InvalidTarget;
            }

            if (whatParameter.IsAll) // put all [in] container, put all.item [in] container
            {
                // TODO: same code as above (***) except source collection (container.Content)
                // list must be cloned because it'll be modified when putting an item
                IReadOnlyCollection<IItem> list = !string.IsNullOrWhiteSpace(whatParameter.Value)
                    // put all.item [in] container
                    ? new ReadOnlyCollection<IItem>(FindHelpers.FindAllByName(Inventory.Where(CanSee), whatParameter).ToList())
                    // put all [in] container
                    : new ReadOnlyCollection<IItem>(Inventory.Where(CanSee).ToList());
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

        [CharacterCommand("drink", "Drink", MinPosition = Positions.Resting)]
        [Syntax(
            "[cmd]",
            "[cmd] <container>")]
        protected virtual CommandExecutionResults DoDrink(string rawParameters, params CommandParameter[] parameters)
        {
            IItemDrinkable drinkable;
            // fountain in room
            if (parameters.Length == 0)
            {
                drinkable = Room.Content.OfType<IItemDrinkable>().FirstOrDefault();
                if (drinkable == null)
                {
                    Send("Drink what?");
                    return CommandExecutionResults.TargetNotFound;
                }
            }
            // search in room/inventory/equipment
            else
            {
                drinkable = FindHelpers.FindItemHere<IItemDrinkable>(this, parameters[0]);
                if (drinkable == null)
                {
                    Send(StringHelpers.CantFindIt);
                    return CommandExecutionResults.TargetNotFound;
                }
            }
            // from here, we are sure we have a drinkable item
            IPlayableCharacter pc = this as IPlayableCharacter;
            // drunk ?
            if (pc?[Conditions.Drunk] > 10)
            {
                Send("You fail to reach your mouth.  *Hic*");
                return CommandExecutionResults.NoExecution;
            }

            // get liquid info
            (string name, string color, int proof, int full, int thirst, int food, int servingsize) liquidInfo = TableValues.LiquidInfo(drinkable.LiquidName);
            if (liquidInfo == default)
            {
                Send("You can't drink from that.");
                Log.Default.WriteLine(LogLevels.Error,"Invalid liquid name {0} item {1}", drinkable.LiquidName, drinkable.DebugName);
                return CommandExecutionResults.InvalidTarget;
            }
            // empty
            if (drinkable.IsEmpty)
            {
                Send("It is already empty.");
                return CommandExecutionResults.NoExecution;
            }
            // full ?
            if (pc?[Conditions.Full] > 45)
            {
                Send("You're too full to drink more.");
                return CommandExecutionResults.NoExecution;
            }
            // compute amount (limited to liquid left)
            int amount = Math.Min(drinkable.LiquidLeft, liquidInfo.servingsize * drinkable.LiquidAmountMultiplier);
            // drink
            drinkable.Drink(amount);
            Act(ActOptions.ToAll, "{0:N} drink{0:v} {1} from {2}.", this, liquidInfo.name, drinkable);
            // drunk/thirst/food/full
            if (pc != null)
            {
                pc.GainCondition(Conditions.Drunk, (amount * liquidInfo.proof) / 36);
                pc.GainCondition(Conditions.Full, (amount * liquidInfo.full) / 4);
                pc.GainCondition(Conditions.Thirst, (amount * liquidInfo.thirst) / 10);
                pc.GainCondition(Conditions.Hunger, (amount * liquidInfo.food) / 2);

                if (pc[Conditions.Drunk] > 10)
                    Send("You feel drunk.");
                if (pc[Conditions.Full] > 40)
                    Send("You are full.");
                if (pc[Conditions.Thirst] > 40)
                    Send("Your thirst is quenched.");
            }
            // poisoned?
            if (drinkable is IItemPoisonable poisonable && poisonable.IsPoisoned)
            {
                Act(ActOptions.ToAll, "{0:N} choke{0:v} and gag{0:v}.", this);
                // search poison affect
                IAbility poison = AbilityManager["Poison"];
                IAura poisonAura = GetAura(poison);
                int duration = amount * 3;
                int level = RandomManager.Fuzzy(amount);
                if (poisonAura != null)
                {
                    poisonAura.Update(level, TimeSpan.FromMinutes(duration));
                    poisonAura.AddOrUpdateAffect(
                        x => x.Modifier == CharacterFlags.Poison,
                        () => new CharacterFlagsAffect { Modifier = CharacterFlags.Poison, Operator = AffectOperators.Or },
                        null);
                }
                else
                    World.AddAura(this, poison, drinkable, level, TimeSpan.FromMinutes(duration), AuraFlags.None, false,
                        new CharacterFlagsAffect { Modifier = CharacterFlags.Poison, Operator = AffectOperators.Or });
                Recompute();
            }

            return CommandExecutionResults.Ok;
        }

        [CharacterCommand("pour", "Drink", MinPosition = Positions.Resting)]
        [Syntax(
            "[cmd] <container> out",
            "[cmd] <container> <container>",
            "[cmd] <container> <character>")]
        protected virtual CommandExecutionResults DoPour(string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length < 2)
            {
                Send("Pour what into what?");
                return CommandExecutionResults.SyntaxErrorNoDisplay;
            }

            // search source container
            IItem item = FindHelpers.FindByName(Inventory, parameters[0]);
            if (item == null)
            {
                Send("You don't have that item.");
                return CommandExecutionResults.TargetNotFound;
            }
            IItemDrinkContainer sourceContainer = item as IItemDrinkContainer;
            if (sourceContainer == null)
            {
                Send("That's not a drink container.");
                return CommandExecutionResults.InvalidTarget;
            }
            // pour out
            if (StringCompareHelpers.StringEquals("out", parameters[1].Value))
            {
                if (sourceContainer.IsEmpty)
                {
                    Send("It's already empty.");
                    return CommandExecutionResults.InvalidTarget;
                }
                sourceContainer.Pour();
                sourceContainer.Recompute();
                Act(ActOptions.ToAll, "{0:N} invert{0:v} {1}, spilling {2} all over the ground.", this, sourceContainer, sourceContainer.LiquidName);
                return CommandExecutionResults.Ok;
            }
            // pour into another container on someone's hand or here
            ICharacter targetCharacter = null;
            IItem targetItem = FindHelpers.FindByName(Inventory, parameters[1]);
            if (targetItem == null)
            {
                targetCharacter = FindHelpers.FindByName(Room.People, parameters[1]);
                if (targetCharacter == null)
                {
                    Send("Pour into what?");
                    return CommandExecutionResults.TargetNotFound;
                }
                targetItem = targetCharacter.GetEquipment(EquipmentSlots.OffHand);
                if (targetItem == null)
                {
                    Send("They aren't holding anything.");
                    return CommandExecutionResults.InvalidTarget;
                }
            }
            // destination item found
            IItemDrinkContainer targetContainer = targetItem as IItemDrinkContainer;
            if (targetContainer == null)
            {
                Send("You can only pour into other drink containers.");
                return CommandExecutionResults.InvalidTarget;
            }
            if (targetContainer == sourceContainer)
            {
                Send("You cannot change the laws of physics!");
                return CommandExecutionResults.InvalidTarget;
            }
            if (!targetContainer.IsEmpty && targetContainer.LiquidName != sourceContainer.LiquidName)
            {
                Send("They don't hold the same liquid.");
                return CommandExecutionResults.InvalidTarget;
            }
            if (sourceContainer.IsEmpty)
            {
                Act(ActOptions.ToCharacter, "There's nothing in {0} to pour.", sourceContainer);
                return CommandExecutionResults.InvalidTarget;
            }
            if (targetContainer.LiquidLeft >= targetContainer.MaxLiquid)
            {
                Act(ActOptions.ToCharacter, "{0} is already filled to the top.", targetContainer);
                return CommandExecutionResults.InvalidTarget;
            }
            int amount = Math.Min(sourceContainer.LiquidLeft, targetContainer.MaxLiquid - targetContainer.LiquidLeft);
            targetContainer.Fill(sourceContainer.LiquidName, amount);
            if (sourceContainer.IsPoisoned) // copy poison or not poisoned
                targetContainer.Poison();
            else
                targetContainer.Cure();
            targetContainer.Recompute();
            sourceContainer.Recompute();
            //
            if (targetCharacter == null)
                Act(ActOptions.ToAll, "{0:N} pour{0:v} {1} from {2} into {3}.", this, sourceContainer.LiquidName, sourceContainer, targetContainer);
            else
            {
                targetCharacter.Act(ActOptions.ToCharacter, "{0:N} pours you some {1}.", this, sourceContainer.LiquidName);
                targetCharacter.Act(ActOptions.ToRoom, "{0:N} pour{0:v} some {1} for {2}", this, sourceContainer.LiquidName, targetCharacter);
            }

            return CommandExecutionResults.Ok;
        }

        [CharacterCommand("fill", "Drink", MinPosition = Positions.Resting)]
        [Syntax("[cmd] <container>")]
        protected virtual CommandExecutionResults DoFill(string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length == 0)
            {
                Send("Fill what?");
                return CommandExecutionResults.SyntaxErrorNoDisplay;
            }
            // search drink container
            IItem item = FindHelpers.FindByName(Inventory, parameters[0]);
            if (item == null)
            {
                Send("You do not have that item.");
                return CommandExecutionResults.TargetNotFound;
            }
            // search fountain
            IItemFountain fountain = Room.Content.OfType<IItemFountain>().FirstOrDefault();
            if (fountain == null)
            {
                Send("There is no fountain here!");
                return CommandExecutionResults.TargetNotFound;
            }
            // drink container?
            IItemDrinkContainer drinkContainer = item as IItemDrinkContainer;
            if (drinkContainer == null)
            {
                Send("You can't fill that.");
                return CommandExecutionResults.InvalidTarget;
            }
            // same liquid ?
            if (!drinkContainer.IsEmpty && drinkContainer.LiquidName != fountain.LiquidName)
            {
                Send("There is already another liquid in it.");
                return CommandExecutionResults.InvalidTarget;
            }
            // full
            if (drinkContainer.LiquidLeft >= drinkContainer.MaxLiquid)
            {
                Send("Your container is full.");
                return CommandExecutionResults.InvalidTarget;
            }
            // let's go
            Act(ActOptions.ToAll, "{0:N} fill{0:v} {1} with {2} from {3}.", this, drinkContainer, fountain.LiquidName, fountain);
            drinkContainer.Fill(fountain.LiquidName, drinkContainer.MaxLiquid);
            drinkContainer.Recompute();

            return CommandExecutionResults.Ok;
        }

        [CharacterCommand("eat", "Food", MinPosition = Positions.Resting)]
        [Syntax("[cmd] <food|pill>")]
        protected virtual CommandExecutionResults DoEat(string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length == 0)
            {
                Send("Eat what?");
                return CommandExecutionResults.SyntaxErrorNoDisplay;
            }

            IItem item = FindHelpers.FindByName(Inventory.Where(CanSee), parameters[0]);
            if (item == null)
            {
                Send(StringHelpers.ItemInventoryNotFound);
                return CommandExecutionResults.TargetNotFound;
            }

            IItemFood food = item as IItemFood;
            IItemPill pill = item as IItemPill;
            if (food == null && pill == null)
            {
                Send("That's not edible.");
                return CommandExecutionResults.InvalidTarget;
            }

            IPlayableCharacter pc = this as IPlayableCharacter;
            if (pc?[Conditions.Full] > 40)
            {
                Send("You are too full to eat more.");
                return CommandExecutionResults.NoExecution;
            }

            Act(ActOptions.ToAll, "{0:N} eat{0:v} {1}", this, item);

            if (food != null)
            {
                if (pc != null)
                {
                    int hunger = pc[Conditions.Hunger];
                    pc.GainCondition(Conditions.Full, food.FullHours);
                    pc.GainCondition(Conditions.Hunger, food.HungerHours);
                    if (hunger == 0 && pc[Conditions.Hunger] > 0)
                        Send("You are no longer hungry.");
                    else if (pc[Conditions.Full] > 40)
                        Send("You are full.");
                }
                // poisoned ?
                if (food.IsPoisoned)
                {
                    Act(ActOptions.ToAll, "{0:N} choke{0:v} and gag{0:v}.", this);
                    // search poison affect
                    IAbility poison = AbilityManager["Poison"];
                    IAura poisonAura = GetAura(poison);
                    int level = RandomManager.Fuzzy(food.FullHours);
                    int duration = food.FullHours * 2;
                    if (poisonAura != null)
                    {
                        poisonAura.Update(level, TimeSpan.FromMinutes(duration));
                        poisonAura.AddOrUpdateAffect(
                            x => x.Modifier == CharacterFlags.Poison,
                            () => new CharacterFlagsAffect { Modifier = CharacterFlags.Poison, Operator = AffectOperators.Or },
                            null);
                    }
                    else
                        World.AddAura(this, poison, food, level, TimeSpan.FromMinutes(duration), AuraFlags.None, false,
                            new CharacterFlagsAffect { Modifier = CharacterFlags.Poison, Operator = AffectOperators.Or });
                    Recompute();
                }
                World.RemoveItem(food);
                return CommandExecutionResults.Ok;
            }
            if (pill != null)
            {
                AbilityManager.CastFromItem(pill.FirstSpell, pill.SpellLevel, this, this, null, null);
                AbilityManager.CastFromItem(pill.SecondSpell, pill.SpellLevel, this, this, null, null);
                AbilityManager.CastFromItem(pill.ThirdSpell, pill.SpellLevel, this, this, null, null);
                AbilityManager.CastFromItem(pill.FourthSpell, pill.SpellLevel, this, this, null, null);
                World.RemoveItem(pill);
                return CommandExecutionResults.Ok;
            }
            return CommandExecutionResults.Ok;
        }

        [CharacterCommand("quaff", "Drink", MinPosition = Positions.Resting)]
        [Syntax("[cmd] <potion>")]
        protected virtual CommandExecutionResults DoQuaff(string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length == 0)
            {
                Send("Quaff what?");
                return CommandExecutionResults.SyntaxErrorNoDisplay;
            }

            IItem item = FindHelpers.FindByName(Inventory.Where(CanSee), parameters[0]);
            if (item == null)
            {
                Send("You do not have that potion.");
                return CommandExecutionResults.TargetNotFound;
            }

            IItemPotion potion = item as IItemPotion;
            if (potion == null)
            {
                Send("You can quaff only potions.");
                return CommandExecutionResults.InvalidTarget;
            }

            if (Level < potion.Level)
            {
                Send("This liquid is too powerful for you to drink.");
                return CommandExecutionResults.InvalidTarget;
            }

            Act(ActOptions.ToRoom, "{0:N} quaff{0:v} {1}.", this, potion);

            AbilityManager.CastFromItem(potion.FirstSpell, potion.SpellLevel, this, this, null, null);
            AbilityManager.CastFromItem(potion.SecondSpell, potion.SpellLevel, this, this, null, null);
            AbilityManager.CastFromItem(potion.ThirdSpell, potion.SpellLevel, this, this, null, null);
            AbilityManager.CastFromItem(potion.FourthSpell, potion.SpellLevel, this, this, null, null);
            World.RemoveItem(potion);
            return CommandExecutionResults.Ok;
        }

        //********************************************************************
        // Helpers
        //********************************************************************
        private bool WearItem(IItem item, bool replace) // equivalent to wear_obj in act_obj.C:1467
        {
            // check level
            if (item.Level > Level)
            {
                Send("You must be level {0} to use this object.", item.Level);
                Act(ActOptions.ToRoom, "{0} tries to use {1}, but is too inexperienced.", this, item);
                return false;
            }
            // can be worn ?
            if (item.WearLocation == WearLocations.None)
            {
                if (replace) // replace means, only item is trying to be worn
                    Act(ActOptions.ToCharacter, "{0} cannot be worn.", item);
                return false;
            }
            // search slot
            EquippedItem equipmentSlot = SearchEquipmentSlot(item, replace);
            if (equipmentSlot == null)
            {
                if (replace) // we don't want to spam if character is trying to wear all, replace is set to true only when wearing one item
                    Act(ActOptions.ToCharacter, "You cannot wear {0}.", item);
                return false;
            }
            // too heavy to be wielded ?
            IItemWeapon weapon = item as IItemWeapon;
            if (weapon != null && this is IPlayableCharacter && weapon.TotalWeight > TableValues.WieldBonus(this) * 10)
            {
                Send("It is too heavy for you to wield.");
                return false;
            }
            // remove if needed
            if (replace && equipmentSlot.Item != null)
            {
                IItem removeItem = equipmentSlot.Item;
                Act(ActOptions.ToAll, "{0:N} remove{0:v} {1}.", this, removeItem);
                //equipmentSlot.Item = null  already done by ChangeEquippedBy
                removeItem.ChangeEquippedBy(null, false);
                removeItem.ChangeContainer(this);
            }
            // Display
            string wearPhrase = GetEquipmentSlotWearPhrase(equipmentSlot.Slot, item);
            Act(ActOptions.ToAll, wearPhrase, this, item);
            // Equip at last
            equipmentSlot.Item = item; // equip
            item.ChangeContainer(null); // remove from inventory
            item.ChangeEquippedBy(this, false); // set as equipped by this
            // Display weapon confidence if wielding weapon
            if (weapon != null)
            {
                string weaponConfidence = GetWeaponConfidence(weapon);
                Act(ActOptions.ToCharacter, weaponConfidence, weapon);
            }
            // no need to recompute, because it's being done by caller

            return true;
        }

        private string GetEquipmentSlotWearPhrase(EquipmentSlots slot, IItem item)
        {
            switch (slot)
            {
                case EquipmentSlots.Light: return "{0:N} light{0:v} {1} and holds it.";
                case EquipmentSlots.Head: return "{0:N} wear{0:v} {1} on {0:s} head.";
                case EquipmentSlots.Amulet: return "{0:N} wear{0:v} {1} around {0:s} neck.";
                case EquipmentSlots.Chest: return "{0:N} wear{0:v} {1} on {0:s} torso.";
                case EquipmentSlots.Cloak: return "{0:N} wear{0:v} {1} about {0:s} torso.";
                case EquipmentSlots.Waist: return "{0:N} wear{0:v} {1} about {0:s} waist.";
                case EquipmentSlots.Wrists: return "{0:N} wear{0:v} {1} around {0:s} wrist.";
                case EquipmentSlots.Arms: return "{0:N} wear{0:v} {1} on {0:s} arms.";
                case EquipmentSlots.Hands: return "{0:N} wear{0:v} {1} on {0:s} hands.";
                    case EquipmentSlots.Ring: return "{0:N} wear{0:v} {1} on {0:s} finger.";
                case EquipmentSlots.Legs: return "{0:N} wear{0:v} {1} on {0:s} legs.";
                case EquipmentSlots.Feet: return "{0:N} wear{0:v} {1} on {0:s} feet.";
                case EquipmentSlots.MainHand: return "{0:N} wield{0:v} {1}.";
                case EquipmentSlots.OffHand:
                    switch (item)
                    {
                        case IItemWeapon _:
                            return "{0:N} wield{0:v} {1}.";
                        case IItemShield _:
                            return "{0:N} wear{0:v} {1} as a shield.";
                        default:
                            return "{0:N} hold{0:v} {1} in {0:s} hand.";
                    }
                case EquipmentSlots.Float: return "{0:N} release{0:v} {1} to float next to {0:m}.";
                default:
                    Log.Default.WriteLine(LogLevels.Error, "Invalid EquipmentSlots {0} for item {1} character {2}", slot, item.DebugName, this.DebugName);
                    return "{0:N} wear{0:v} {1}.";
            }
        }

        private string GetWeaponConfidence(IItemWeapon weapon)
        {
            var weaponLearnInfo = GetWeaponLearnInfo(weapon);
            if (weaponLearnInfo.learned >= 100)
                return "{0:N} feels like a part of you!";
            if (weaponLearnInfo.learned > 85)
                return "You feel quite confident with {0:N}.";
            if (weaponLearnInfo.learned > 70)
                return "You are skilled with {0:N}.";
            if (weaponLearnInfo.learned > 50)
                return "Your skill with {0:N} is adequate.";
            if (weaponLearnInfo.learned > 25)
                return "{0:N} feels a little clumsy in your hands.";
            if (weaponLearnInfo.learned > 1)
                return "You fumble and almost drop {0:N}.";
            return "You don't even know which end is up on {0:N}.";
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

        private bool GetItem(IItem item, IContainer container) // equivalent to get_obj in act_obj.C:211
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
            item.ChangeContainer(this);
            // TODO: money
            return true;
        }

        private bool RemoveItem(EquippedItem equipmentSlot)
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

        private bool PutItem(IItem item, IItemContainer container)
        {
            //
            if (item.ItemFlags.HasFlag(ItemFlags.NoDrop))
            {
                Send("You can't let go of it.");
                return false;
            }
            int itemWeight = item.TotalWeight;
            if ((itemWeight + container.TotalWeight > container.MaxWeight)
                || itemWeight > container.MaxWeightPerItem)
            {
                Send("It won't fit.");
                return false;
            }

            if (item == container)
            {
                Send("You can't fold it into itself.");
                return false;
            }

            // TODO: pit
            Act(ActOptions.ToAll, "{0:N} put{0:v} {1} in {2}.", this, item, container);
            item.ChangeContainer(container);
            return true;
        }
    }
}
