using System;
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
        [Command("wear", "Item", "Equipment")]
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

        [Command("wield", "Item", "Equipment")]
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

        [Command("hold", "Item", "Equipment")]
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
            if (equipable == null || (equipable.WearLocation != WearLocations.Hold && equipable.WearLocation != WearLocations.Shield))
            {
                Send("It cannot be hold.");
                return CommandExecutionResults.InvalidTarget;
            }
            //
            WearItem(equipable, true);
            Recompute();
            return CommandExecutionResults.Ok;
        }

        [Command("remove", "Item", "Equipment")]
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

        [Command("get", "Item", "Inventory")]
        [Command("take", "Item", "Inventory")]
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
            if (containerItem is ICloseable closeable && closeable.IsClosed)
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

        [Command("drop", "Item", "Inventory")]
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

        [Command("give", "Item", "Equipment")]
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

        [Command("put", "Item", "Equipment")]
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
            if (where is ICloseable closeable && closeable.IsClosed)
            {
                Act(ActOptions.ToCharacter, "The {0} is closed.", container);
                return CommandExecutionResults.InvalidTarget;
            }

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

        [Command("drink", "Drink")]
        [Syntax(
            "[cmd]",
            "[cmd] <container>")]
        protected virtual CommandExecutionResults DoDrink(string rawParameters, params CommandParameter[] parameters)
        {
            IItemDrinkable drinkable = null;
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

        [Command("pour", "Drink")]
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
            if (item == null)
            {
                targetCharacter = FindHelpers.FindByName(Room.People, parameters[1]);
                if (targetCharacter == null)
                {
                    Send("Pour into what?");
                    return CommandExecutionResults.TargetNotFound;
                }
                targetItem = targetCharacter.Equipments.FirstOrDefault(x => x.Slot == EquipmentSlots.OffHand && x.Item != null)?.Item;
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

        [Command("fill", "Drink")]
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

        [Command("eat", "Food")]
        [Syntax("[cmd] <item>")]
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

            // TODO pill
            IItemFood food = item as IItemFood;
            if (food == null)
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
                return CommandExecutionResults.Ok;
            }
            // TODO: pill
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
