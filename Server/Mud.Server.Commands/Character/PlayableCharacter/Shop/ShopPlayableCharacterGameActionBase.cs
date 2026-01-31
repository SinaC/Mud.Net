using Mud.Blueprints.Character;
using Mud.Random;
using Mud.Server.Common.Helpers;
using Mud.Server.GameAction;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Item;

namespace Mud.Server.Commands.Character.PlayableCharacter.Shop;

public abstract class ShopPlayableCharacterGameActionBase : PlayableCharacterGameAction
{
    protected ITimeManager TimeManager { get; }
    protected IAbilityManager AbilityManager { get; }
    protected IRandomManager RandomManager { get; }

    protected ShopPlayableCharacterGameActionBase(ITimeManager timeManager, IAbilityManager abilityManager, IRandomManager randomManager)
    {
        TimeManager = timeManager;
        AbilityManager = abilityManager;
        RandomManager = randomManager;
    }

    protected INonPlayableCharacter? ShopKeeper { get; set; }
    protected CharacterShopBlueprintBase? ShopBlueprintBase { get; set; }

    public override string? CanExecute(IActionInput actionInput)
    {
        var baseGuards = base.CanExecute(actionInput);
        if (baseGuards != null)
            return baseGuards;

        var (shopKeeper, shopBlueprint) = Actor.Room.GetNonPlayableCharacters<CharacterShopBlueprintBase>().FirstOrDefault();
        if (shopKeeper == null)
            return StringHelpers.CantDoThatHere;

        if (TimeManager.Hour < shopBlueprint.OpenHour)
            return Actor.ActPhrase("%g%{0:N} say{0:v} '%x%Sorry, I am closed. Come back later.%g%'%x%", shopKeeper);

        if (TimeManager.Hour > shopBlueprint.CloseHour)
            return Actor.ActPhrase("%g%{0:N} say{0:v} '%x%Sorry, I am closed. Come back tomorrow.%g%'%x%", shopKeeper);

        ShopKeeper = shopKeeper;
        ShopBlueprintBase = shopBlueprint;

        return null;
    }

    protected long GetBuyCost(INonPlayableCharacter shopKeeper, CharacterShopBlueprint shopBlueprint, IItem item, bool tryToHaggle)
    {
        if (item == null || !item.IsValid)
            return 0;
        var cost = (long)item.Cost * shopBlueprint.ProfitBuy / 100;
        if (cost <= 0)
            return 0;
        if (item.ItemFlags.IsSet("SellExtract")) // no haggling if item is SellExtract
            return cost;
        if (tryToHaggle)
        {
            // pick one change cost passive
            var changeCostPassiveDefinition = RandomManager.Random(AbilityManager.SearchAbilitiesByExecutionType<IChangeCostPassive>());
            if (changeCostPassiveDefinition != null)
            {
                var changeCostPassive = AbilityManager.CreateInstance<IChangeCostPassive>(changeCostPassiveDefinition);
                if (changeCostPassive != null)
                    cost = changeCostPassive.HaggleBuyPrice(Actor, shopKeeper, cost);
            }
        }
        return cost;
    }

    protected long GetBuyCost(INonPlayableCharacter shopKeeper, CharacterPetShopBlueprint petShopBlueprint, CharacterNormalBlueprint petBlueprint, bool tryToHaggle)
    {
        var cost = (long)petBlueprint.Level * petBlueprint.Level * 10 * petShopBlueprint.ProfitBuy / 100;
        if (cost <= 0)
            return 0;
        if (tryToHaggle)
        {
            // pick one change cost passive
            var changeCostPassiveDefinition = RandomManager.Random(AbilityManager.SearchAbilitiesByExecutionType<IChangeCostPassive>());
            if (changeCostPassiveDefinition != null)
            {
                var changeCostPassive = AbilityManager.CreateInstance<IChangeCostPassive>(changeCostPassiveDefinition);
                if (changeCostPassive != null)
                    cost = changeCostPassive.HaggleBuyPrice(Actor, shopKeeper, cost);
            }
        }
        return cost;
    }

    protected long GetSellCost(INonPlayableCharacter shopKeeper, CharacterShopBlueprint shopBlueprint, IItem item, bool tryToHaggle)
    {
        if (item == null || !item.IsValid)
            return 0;
        long cost = 0;
        // check if interested in this kind of item
        if (shopBlueprint.BuyBlueprintTypes.Contains(item.Blueprint.GetType()))
            cost = item.Cost * shopBlueprint.ProfitSell / 100;
        if (cost <= 0)
            return 0;
        if (item.ItemFlags.IsSet("SellExtract")) // no lowered price nor haggling if item is SellExtract
            return cost;
        // lower price depending on quantity/quality
        // more copy -> lower price
        foreach (var itemInventory in shopKeeper.Inventory)
            if (itemInventory.Blueprint.Id == item.Blueprint.Id)
            {
                if (itemInventory.ItemFlags.IsSet("Inventory"))
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
        // haggle ?
        if (tryToHaggle)
        {
            // pick one change cost passive
            var changeCostPassiveDefinition = RandomManager.Random(AbilityManager.SearchAbilitiesByExecutionType<IChangeCostPassive>());
            if (changeCostPassiveDefinition != null)
            {
                var changeCostPassive = AbilityManager.CreateInstance<IChangeCostPassive>(changeCostPassiveDefinition);
                if (changeCostPassive != null)
                {
                    var buyPrice = GetBuyCost(shopKeeper, shopBlueprint, item, false);
                    cost = changeCostPassive.HaggleSellPrice(Actor, shopKeeper, cost, buyPrice);
                }
            }
        }
        return cost;
    }
}
