using Mud.Common;
using Mud.Domain;
using Mud.Server.Blueprints.Character;
using Mud.Server.Common;
using Mud.Server.Helpers;
using Mud.Server.Input;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Item;
using System;
using System.Linq;
using System.Text;

namespace Mud.Server.Character.PlayableCharacter
{
    public partial class PlayableCharacter
    {
        [PlayableCharacterCommand("list", "Shop")]
        protected virtual CommandExecutionResults DoList(string rawParameters, params CommandParameter[] parameters)
        {
            var keeperInfo = FindKeeper();
            if (keeperInfo == default)
                return CommandExecutionResults.TargetNotFound;
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("[Lvl Price Qty] Item");
            bool found = false;
            foreach (var itemAndCost in keeperInfo.shopKeeper.Inventory
                .Where(CanSee)
                .GroupBy(x => x.Blueprint.Id)
                .Select(g => new 
                { 
                    item = g.First(), 
                    cost = GetBuyCost(keeperInfo.shopBlueprint, g.First()), 
                    count = g.Count()
                }))
            {
                found = true;
                if (itemAndCost.item.ItemFlags.HasFlag(ItemFlags.Inventory))
                    sb.AppendFormatLine("[{0,3} {1,5} -- ] {2}", itemAndCost.item.Level, itemAndCost.cost, itemAndCost.item.DisplayName);
                else
                    sb.AppendFormatLine("[{0,3} {1,5} {2,2} ] {3}", itemAndCost.item.Level, itemAndCost.cost, itemAndCost.count,  itemAndCost.item.DisplayName);
            }
            if (!found)
                Send("You can't buy anything here.");
            else
                Send(sb);
            return CommandExecutionResults.Ok;
        }

        [PlayableCharacterCommand("buy", "Shop")]
        [Syntax("[cmd] [number] <item>")]
        protected virtual CommandExecutionResults DoBuy(string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length == 0)
                return CommandExecutionResults.SyntaxError;
            if (parameters.Length >= 2 && !parameters[0].IsNumber)
                return CommandExecutionResults.SyntaxError;
            int count = parameters.Length >= 2
                ? parameters[0].AsNumber
                : 1;
            CommandParameter itemParameter = parameters.Length >= 2
                ? parameters[1]
                : parameters[0];

            (INonPlayableCharacter shopKeeper, CharacterShopBlueprint shopBlueprint) keeperInfo = FindKeeper();
            if (keeperInfo == default)
                return CommandExecutionResults.TargetNotFound;
            IItem item = FindHelpers.FindByName(keeperInfo.shopKeeper.Inventory.Where(CanSee), itemParameter);
            int cost = GetBuyCost(keeperInfo.shopBlueprint, item);
            if (item == null || cost <= 0)
            {
                Act(ActOptions.ToCharacter, "{0:N} tells you 'I don't sell that -- try 'list''.", keeperInfo.shopKeeper);
                return CommandExecutionResults.TargetNotFound;
            }
            if (count <= 0 || count > 10)
            {
                Send("Number must be between 1 and 10");
                return CommandExecutionResults.InvalidParameter;
            }
            // can afford ?
            long totalCost = cost * count;
            long wealth = SilverCoins + GoldCoins * 100;
            if (totalCost > wealth)
            {
                if (count == 1)
                    Act(ActOptions.ToCharacter, "{0:N} tells you 'You can't afford to buy {1}'.", keeperInfo.shopKeeper, item);
                else
                {
                    // how many can afford?
                    long affordableCount = wealth / cost;
                    if (affordableCount > 0)
                        Act(ActOptions.ToCharacter, "{0:N} tells you 'You can only afford {1} of these'.", keeperInfo.shopKeeper, affordableCount);
                    else
                        Act(ActOptions.ToCharacter, "{0:N} tells you '{1}? You must be kidding - you can't even afford a single one, let alone {2}!'", keeperInfo.shopKeeper, item, count);
                }
                return CommandExecutionResults.NoExecution;
            }
            // can use item ?
            if (item.Level > Level)
            {
                Act(ActOptions.ToCharacter, "{0:N} tells you 'You can't use {1} yet'.", keeperInfo.shopKeeper, item);
                return CommandExecutionResults.InvalidTarget;
            }
            // can carry more items ?
            if (CarryNumber + item.CarryCount * count > MaxCarryNumber)
            {
                Send("You can't carry that many items.");
                return CommandExecutionResults.InvalidTarget;
            }
            // can carry more weight ?
            if (CarryWeight + item.TotalWeight * count > MaxCarryWeight)
            {
                Send("You can't carry that much weight.");
                return CommandExecutionResults.InvalidTarget;
            }
            // check for object sold to the keeper
            if (count > 1 && !item.ItemFlags.HasFlag(ItemFlags.Inventory))
            { 
                Act(ActOptions.ToCharacter, "{0:N} tells you 'Sorry - {1} is something I have only one of'.", keeperInfo.shopKeeper, item);
                return CommandExecutionResults.InvalidParameter;
            }
            // TODO: haggle ?

            var deductedCost = DeductCost(totalCost);
            keeperInfo.shopKeeper.UpdateMoney(totalCost % 100, totalCost / 100);

            if (count == 1)
                Act(ActOptions.ToAll, "{0:N} buy{0:v} {1} for {2} silver and {3} gold piece{4}.", this, item, deductedCost.silver, deductedCost.gold, cost == 1 ? string.Empty : "s");
            else
                Act(ActOptions.ToAll, "{0:N} buy{0:v} {1} * {2} for {3} silver and {4} gold piece{5}.", this, count, item, deductedCost.silver, deductedCost.gold, cost == 1 ? string.Empty : "s");
            // Inventory items are created on the fly
            if (item.ItemFlags.HasFlag(ItemFlags.Inventory))
            {
                for (int i = 0; i < count; i++)
                    ItemManager.AddItem(Guid.NewGuid(), item.Blueprint, this);
            }
            // Items previously sold to keeper are 'given' to buyer
            else
            {
                item.ChangeContainer(this);
                item.SetTimer(TimeSpan.FromMinutes(0));
            }
            return CommandExecutionResults.Ok;
        }

        [PlayableCharacterCommand("sell", "Shop")]
        protected virtual CommandExecutionResults DoSell(string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length == 0)
            {
                Send("Sell what?");
                return CommandExecutionResults.SyntaxErrorNoDisplay;
            }

            var keeperInfo = FindKeeper();
            if (keeperInfo == default)
                return CommandExecutionResults.TargetNotFound;

            IItem item = FindHelpers.FindByName(Inventory.Where(CanSee), parameters[0]);
            if (item == null)
            {
                Act(ActOptions.ToCharacter, "{0:N} tells you 'You don't have that item'.", keeperInfo.shopKeeper);
                return CommandExecutionResults.TargetNotFound;
            }

            if (!keeperInfo.shopKeeper.CanSee(item))
            {
                Act(ActOptions.ToCharacter, "{0:N} doesn't see what you are offering.", keeperInfo.shopKeeper);
                return CommandExecutionResults.InvalidTarget;
            }

            if (item.ItemFlags.HasFlag(ItemFlags.NoDrop))
            {
                Send("You can't let go of it.");
                return CommandExecutionResults.InvalidTarget;
            }

            long cost = GetSellCost(keeperInfo.shopKeeper, keeperInfo.shopBlueprint, item);
            if (cost <= 0 || item.DecayPulseLeft > 0)
            {
                Act(ActOptions.ToCharacter, "{0:N} looks uninterested in {1}.", keeperInfo.shopKeeper, item);
                return CommandExecutionResults.InvalidTarget;
            }

            if (cost > keeperInfo.shopKeeper.SilverCoins + keeperInfo.shopKeeper.GoldCoins * 100)
            {
                Act(ActOptions.ToCharacter, "{0} tells you 'I'm afraid I don't have enough wealth to buy {1}.", keeperInfo.shopKeeper, item);
                return CommandExecutionResults.InvalidTarget;
            }

            // TODO: haggle ?

            long silver = cost % 100;
            long gold = cost / 100;

            Act(ActOptions.ToRoom, "{0} sells {1}.", this, item);
            Act(ActOptions.ToCharacter, "You sell {0} for {1} silver and {2} gold piece{3}.", item, silver, gold, cost == 1 ? string.Empty : "s");

            UpdateMoney(silver, gold);
            keeperInfo.shopKeeper.DeductCost(cost);

            if (item is IItemTrash)
                ItemManager.RemoveItem(item);
            else
            {
                item.ChangeContainer(keeperInfo.shopKeeper);
                int duration = RandomManager.Range(50, 100);
                item.SetTimer(TimeSpan.FromMinutes(duration));
            }

            return CommandExecutionResults.Ok;
        }

        [PlayableCharacterCommand("appraise", "Shop", "Identify")]
        protected virtual CommandExecutionResults DoAppraise(string rawParameters, params CommandParameter[] parameters)
        {
            // TODO: identify
            return CommandExecutionResults.Error;
        }

        [PlayableCharacterCommand("value", "Shop")]
        protected virtual CommandExecutionResults DoValue(string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length == 0)
            {
                Send("Value what?");
                return CommandExecutionResults.SyntaxErrorNoDisplay;
            }

            var keeperInfo = FindKeeper();
            if (keeperInfo == default)
                return CommandExecutionResults.TargetNotFound;

            IItem item = FindHelpers.FindByName(Inventory.Where(CanSee), parameters[0]);
            if (item == null)
            {
                Act(ActOptions.ToCharacter, "{0:N} tells you 'You don't have that item'.", keeperInfo.shopKeeper);
                return CommandExecutionResults.TargetNotFound;
            }

            if (!keeperInfo.shopKeeper.CanSee(item))
            {
                Act(ActOptions.ToCharacter, "{0:N} doesn't see what you are offering.", keeperInfo.shopKeeper);
                return CommandExecutionResults.InvalidTarget;
            }

            if (item.ItemFlags.HasFlag(ItemFlags.NoDrop))
            {
                Send("You can't let go of it.");
                return CommandExecutionResults.InvalidTarget;
            }

            long cost = GetSellCost(keeperInfo.shopKeeper, keeperInfo.shopBlueprint, item);
            if (cost <= 0 || item.DecayPulseLeft > 0)
            {
                Act(ActOptions.ToCharacter, "{0:N} looks uninterested in {1}.", keeperInfo.shopKeeper, item);
                return CommandExecutionResults.InvalidTarget;
            }

            long silver = cost % 100;
            long gold = cost / 100;

            Act(ActOptions.ToCharacter, "{0:N} tells you 'I'll give you {1} silver and {2} gold coins for {3}'.", keeperInfo.shopKeeper, silver, gold, item);

            return CommandExecutionResults.Ok;
        }

        //
        protected (INonPlayableCharacter shopKeeper, CharacterShopBlueprint shopBlueprint) FindKeeper()
        {
            INonPlayableCharacter shopKeeper = Room.NonPlayableCharacters.FirstOrDefault(x => x.Blueprint is CharacterShopBlueprint);
            if (shopKeeper == null)
            {
                Send("You can't do that here.");
                return default;
            }

            // TODO: undesirables: killer/thief

            CharacterShopBlueprint shopBlueprint = shopKeeper.Blueprint as CharacterShopBlueprint;

            if (TimeManager.Hour < shopBlueprint.OpenHour)
            {
                Act(ActOptions.ToAll, "%g%{0:N} say{0:v} '%x%Sorry, I am closed. Come back later.%g%'%x%", this);
                return default;
            }
            if (TimeManager.Hour > shopBlueprint.CloseHour)
            {
                Act(ActOptions.ToAll, "%g%{0:N} say{0:v} '%x%Sorry, I am closed. Come back tomorrow.%g%'%x%", this);
                return default;
            }
            return (shopKeeper, shopBlueprint);
        }

        protected int GetBuyCost(CharacterShopBlueprint shopBlueprint, IItem item)
        {
            if (item == null || !item.IsValid)
                return 0;
            return item.Cost * shopBlueprint.ProfitBuy / 100;
        }

        protected int GetSellCost(INonPlayableCharacter shopKeeper, CharacterShopBlueprint shopBlueprint, IItem item)
        {
            if (item == null || !item.IsValid)
                return 0;
            int cost = 0;
            // check if interested in this kind of item
            if (shopBlueprint.BuyBlueprintTypes.Contains(item.Blueprint.GetType()))
                cost = item.Cost * shopBlueprint.ProfitSell / 100;
            if (cost <= 0)
                return 0;
            // more copy -> lower price
            foreach (IItem itemInventory in shopKeeper.Inventory)
                if (itemInventory.Blueprint.Id == item.Blueprint.Id)
                {
                    if (itemInventory.ItemFlags.HasFlag(ItemFlags.Inventory))
                        cost /= 2;
                    else
                        cost = 3 * cost / 4;
                }
            // item with charges are sold at lower price if it has been used
            if (item is IItemCastSpellsCharge itemCharge)
            {
                if (itemCharge.CurrentChargeCount == 0)
                    cost /= 4;
                else
                    cost = cost * itemCharge.CurrentChargeCount / itemCharge.MaxChargeCount;
            }
            return cost;
        }
    }
}
