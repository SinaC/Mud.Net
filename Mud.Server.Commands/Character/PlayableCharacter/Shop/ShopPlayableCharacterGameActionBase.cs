using Mud.Blueprints.Character;
using Mud.Server.GameAction;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Item;
using Mud.Server.Random;

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

    protected (INonPlayableCharacter shopKeeper, CharacterShopBlueprintBase shopBlueprintBase) Keeper { get; set; } = default;

    public override string? Guards(IActionInput actionInput)
    {
        var baseGuards = base.Guards(actionInput);
        if (baseGuards != null)
            return baseGuards;

        Keeper = FindKeeper();
        if (Keeper == default)
            return ""; // message already send by FindKeeper
        return null;
    }

    //
    protected (INonPlayableCharacter shopKeeper, CharacterShopBlueprintBase shopBlueprintBase) FindKeeper()
    {
        var (shopKeeper, shopBlueprint) = Actor.Room.GetNonPlayableCharacters<CharacterShopBlueprintBase>().FirstOrDefault();
        if (shopKeeper == null)
        {
            Actor.Send("You can't do that here.");
            return default;
        }

        // TODO: undesirables: killer/thief

        if (TimeManager.Hour < shopBlueprint.OpenHour)
        {
            Actor.Act(ActOptions.ToAll, "%g%{0:N} say{0:v} '%x%Sorry, I am closed. Come back later.%g%'%x%", Actor);
            return default;
        }
        if (TimeManager.Hour > shopBlueprint.CloseHour)
        {
            Actor.Act(ActOptions.ToAll, "%g%{0:N} say{0:v} '%x%Sorry, I am closed. Come back tomorrow.%g%'%x%", Actor);
            return default;
        }
        return (shopKeeper, shopBlueprint);
    }

    protected long GetBuyCost(INonPlayableCharacter shopKeeper, CharacterShopBlueprint shopBlueprint, IItem item, bool tryToHaggle)
    {
        if (item == null || !item.IsValid)
            return 0;
        var cost = (long)item.Cost * shopBlueprint.ProfitBuy / 100;
        if (cost <= 0)
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
        // more copy -> lower price
        foreach (IItem itemInventory in shopKeeper.Inventory)
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
