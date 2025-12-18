using Mud.Server.Blueprints.Character;
using Mud.Server.GameAction;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Item;

namespace Mud.Server.Commands.Character.PlayableCharacter.Shop;

public abstract class ShopPlayableCharacterGameActionBase : PlayableCharacterGameAction
{
    private ITimeManager TimeManager { get; }

    protected ShopPlayableCharacterGameActionBase(ITimeManager timeManager)
    {
        TimeManager = timeManager;
    }

    protected (INonPlayableCharacter shopKeeper, CharacterShopBlueprint shopBlueprint) Keeper { get; set; } = default;

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
    protected (INonPlayableCharacter shopKeeper, CharacterShopBlueprint shopBlueprint) FindKeeper()
    {
        var shopKeeper = Actor.Room.NonPlayableCharacters.FirstOrDefault(x => x.Blueprint is CharacterShopBlueprint);
        if (shopKeeper == null)
        {
            Actor.Send("You can't do that here.");
            return default;
        }

        // TODO: undesirables: killer/thief

        if (shopKeeper.Blueprint is not CharacterShopBlueprint shopBlueprint)
        {
            Actor.Send("You can't do that here."); // should never happen
            return default;
        }

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

    protected static int GetBuyCost(CharacterShopBlueprint shopBlueprint, IItem item)
    {
        if (item == null || !item.IsValid)
            return 0;
        return item.Cost * shopBlueprint.ProfitBuy / 100;
    }

    protected static int GetSellCost(INonPlayableCharacter shopKeeper, CharacterShopBlueprint shopBlueprint, IItem item)
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
        return cost;
    }
}
