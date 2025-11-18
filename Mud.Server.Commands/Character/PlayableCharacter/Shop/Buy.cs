using Mud.Server.Common;
using Mud.Server.GameAction;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Item;

namespace Mud.Server.Commands.Character.PlayableCharacter.Shop;

[PlayableCharacterCommand("buy", "Shop")]
[Syntax("[cmd] [number] <item>")]
[Help(
@"[cmd] buys an object from a shop keeper.
When multiple items of the same name are listed, type 'buy n.item', where n
is the position of the item in a list of that name.  So if there are two
swords, buy 2.sword will buy the second. If you want to buy multiples of
an item, use an * (buy 5*pie will buy 5 pies).  These can be combined into
(for example) buy 2*2.shield, as long as the * is first.")]
public class Buy : ShopPlayableCharacterGameActionBase
{
    private IItemManager ItemManager { get; }

    public Buy(ITimeManager timeManager, IItemManager itemManager)
        : base(timeManager)
    {
        ItemManager = itemManager;
    }

    protected IItem What { get; set; } = default!;
    protected long TotalCost { get; set; }
    protected int Count { get; set; }

    public override string? Guards(IActionInput actionInput)
    {
        var baseGuards = base.Guards(actionInput);
        if (baseGuards != null)
            return baseGuards;

        if (actionInput.Parameters.Length == 0)
            return BuildCommandSyntax();
        if (actionInput.Parameters.Length >= 2 && !actionInput.Parameters[0].IsNumber)
            return BuildCommandSyntax();
        Count = actionInput.Parameters.Length >= 2
            ? actionInput.Parameters[0].AsNumber
            : 1;
        ICommandParameter itemParameter = actionInput.Parameters.Length >= 2
            ? actionInput.Parameters[1]
            : actionInput.Parameters[0];

        What = FindHelpers.FindByName(Keeper.shopKeeper.Inventory.Where(x => Actor.CanSee(x)), itemParameter)!;
        int cost = GetBuyCost(Keeper.shopBlueprint, What);
        if (What == null || cost <= 0)
            return Actor.ActPhrase("{0:N} tells you 'I don't sell that -- try 'list''.", Keeper.shopKeeper);
        if (Count <= 0 || Count > 10)
            return "Number must be between 1 and 10";
        // can afford ?
        TotalCost = cost * Count;
        long wealth = Actor.SilverCoins + Actor.GoldCoins * 100;
        if (TotalCost > wealth)
        {
            if (Count == 1)
                return Actor.ActPhrase("{0:N} tells you 'You can't afford to buy {1}'.", Keeper.shopKeeper, What);
            else
            {
                // how many can afford?
                long affordableCount = wealth / cost;
                if (affordableCount > 0)
                    return Actor.ActPhrase("{0:N} tells you 'You can only afford {1} of these'.", Keeper.shopKeeper, affordableCount);
                else
                    return Actor.ActPhrase("{0:N} tells you '{1}? You must be kidding - you can't even afford a single one, let alone {2}!'", Keeper.shopKeeper, What, Count);
            }
        }
        // can use item ?
        if (What.Level > Actor.Level)
            return Actor.ActPhrase("{0:N} tells you 'You can't use {1} yet'.", Keeper.shopKeeper, What);
        // can carry more items ?
        if (Actor.CarryNumber + What.CarryCount * Count > Actor.MaxCarryNumber)
            return "You can't carry that many items.";
        // can carry more weight ?
        if (Actor.CarryWeight + What.TotalWeight * Count > Actor.MaxCarryWeight)
            return "You can't carry that much weight.";
        // check for object sold to the keeper
        if (Count > 1 && !What.ItemFlags.IsSet("Inventory"))
            return Actor.ActPhrase("{0:N} tells you 'Sorry - {1} is something I have only one of'.", Keeper.shopKeeper, What);
        return null;
    }

    public override void Execute(IActionInput actionInput)
    {
        var (silver, gold) = Actor.DeductCost(TotalCost);
        Keeper.shopKeeper.UpdateMoney(TotalCost % 100, TotalCost / 100);

        if (Count == 1)
            Actor.Act(ActOptions.ToAll, "{0:N} buy{0:v} {1} for {2} silver and {3} gold piece{4}.", Actor, What, silver, gold, TotalCost == 1 ? string.Empty : "s");
        else
            Actor.Act(ActOptions.ToAll, "{0:N} buy{0:v} {1} * {2} for {3} silver and {4} gold piece{5}.", Actor, Count, What, silver, gold, TotalCost == 1 ? string.Empty : "s");
        // Inventory items are created on the fly
        if (What.ItemFlags.IsSet("Inventory"))
        {
            for (int i = 0; i < Count; i++)
                ItemManager.AddItem(Guid.NewGuid(), What.Blueprint, Actor);
        }
        // Items previously sold to keeper are 'given' to buyer
        else
        {
            What.ChangeContainer(Actor);
            What.SetTimer(TimeSpan.FromMinutes(0));
        }
    }
}
