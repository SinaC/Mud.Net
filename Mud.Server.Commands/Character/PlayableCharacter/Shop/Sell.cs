using Mud.Server.Common.Attributes;
using Mud.Server.Common.Helpers;
using Mud.Server.GameAction;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Item;
using Mud.Server.Random;

namespace Mud.Server.Commands.Character.PlayableCharacter.Shop;

[PlayableCharacterCommand("sell", "Shop")]
[Syntax("[cmd] sell <item>")]
[Help(@"[cmd] sells an object to a shop keeper.")]
public class Sell : ShopPlayableCharacterGameActionBase
{
    private IItemManager ItemManager { get; }

    public Sell(ITimeManager timeManager, IAbilityManager abilityManager, IRandomManager randomManager, IItemManager itemManager)
        : base(timeManager, abilityManager, randomManager)
    {
        ItemManager = itemManager;
    }

    protected IItem What { get; set; } = default!;
    protected long Cost { get; set; }

    public override string? Guards(IActionInput actionInput)
    {
        var baseGuards = base.Guards(actionInput);
        if (baseGuards != null)
            return baseGuards;

        if (actionInput.Parameters.Length == 0)
            return "Sell what?";

        What = FindHelpers.FindByName(Actor.Inventory.Where(x => Actor.CanSee(x)), actionInput.Parameters[0])!;
        if (What == null)
            return Actor.ActPhrase("{0:N} tells you 'You don't have that item'.", Keeper.shopKeeper);

        if (!Keeper.shopKeeper.CanSee(What))
            return Actor.ActPhrase("{0:N} doesn't see what you are offering.", Keeper.shopKeeper);

        if (What.ItemFlags.IsSet("NoDrop"))
            return "You can't let go of it.";

        Cost = GetSellCost(Keeper.shopKeeper, Keeper.shopBlueprint, What, true);
        if (Cost <= 0 || What.DecayPulseLeft > 0)
            return Actor.ActPhrase("{0:N} looks uninterested in {1}.", Keeper.shopKeeper, What);

        if (Cost > Keeper.shopKeeper.SilverCoins + Keeper.shopKeeper.GoldCoins * 100)
            return Actor.ActPhrase("{0} tells you 'I'm afraid I don't have enough wealth to buy {1}.", Keeper.shopKeeper, What);

        return null;
    }

    public override void Execute(IActionInput actionInput)
    {
        // TODO: haggle ?

        long silver = Cost % 100;
        long gold = Cost / 100;

        Actor.Act(ActOptions.ToRoom, "{0} sells {1}.", Actor, What);
        Actor.Act(ActOptions.ToCharacter, "You sell {0} for {1} silver and {2} gold piece{3}.", What, silver, gold, Cost == 1 ? string.Empty : "s");

        Actor.UpdateMoney(silver, gold);
        Keeper.shopKeeper.DeductCost(Cost);

        if (What is IItemTrash)
            ItemManager.RemoveItem(What);
        else
        {
            What.ChangeContainer(Keeper.shopKeeper);
            int duration = RandomManager.Range(50, 100);
            What.SetTimer(TimeSpan.FromMinutes(duration));
        }
    }
}
