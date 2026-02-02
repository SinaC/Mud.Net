using Mud.Blueprints.Character;
using Mud.Domain;
using Mud.Random;
using Mud.Server.Ability.Interfaces;
using Mud.Server.Domain.Attributes;
using Mud.Server.Common.Helpers;
using Mud.Server.Guards.Interfaces;
using Mud.Server.Guards.PlayableCharacterGuards;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Item;

namespace Mud.Server.Commands.Character.PlayableCharacter.Shop;

[PlayableCharacterCommand("sell", "Shop")]
[Syntax("[cmd] sell <item>")]
[Help(@"[cmd] sells an object to a shop keeper.")]
public class Sell : ShopPlayableCharacterGameActionBase
{
    protected override IGuard<IPlayableCharacter>[] Guards => [new RequiresMinPosition(Positions.Resting), new RequiresAtLeastOneArgument { Message = "Sell what ?" }];

    private IItemManager ItemManager { get; }

    public Sell(ITimeManager timeManager, IAbilityManager abilityManager, IRandomManager randomManager, IItemManager itemManager)
        : base(timeManager, abilityManager, randomManager)
    {
        ItemManager = itemManager;
    }

    protected IItem What { get; set; } = default!;
    protected long Cost { get; set; }

    public override string? CanExecute(IActionInput actionInput)
    {
        var baseGuards = base.CanExecute(actionInput);
        if (baseGuards != null)
            return baseGuards;

        if (ShopBlueprintBase is not CharacterShopBlueprint shopBlueprint)
            return "You cannot sell anything here.";

        What = FindHelpers.FindByName(Actor.Inventory.Where(Actor.CanSee), actionInput.Parameters[0])!;
        if (What == null)
            return Actor.ActPhrase("{0:N} tells you 'You don't have that item'.", ShopKeeper!);

        if (!ShopKeeper!.CanSee(What))
            return Actor.ActPhrase("{0:N} doesn't see what you are offering.", ShopKeeper!);

        if (What.ItemFlags.IsSet("NoDrop"))
            return "You can't let go of it.";

        if (What is IItemQuest)
            return "You cannot sell that.";

        Cost = GetSellCost(ShopKeeper, shopBlueprint, What, true);
        if (Cost <= 0 || What.DecayPulseLeft > 0)
            return Actor.ActPhrase("{0:N} looks uninterested in {1}.", ShopKeeper!, What);

        if (Cost > ShopKeeper.SilverCoins + ShopKeeper.GoldCoins * 100)
            return Actor.ActPhrase("{0} tells you 'I'm afraid I don't have enough wealth to buy {1}.", ShopKeeper!, What);

        return null;
    }

    public override void Execute(IActionInput actionInput)
    {
        var silver = Cost % 100;
        var gold = Cost / 100;

        Actor.Act(ActOptions.ToRoom, "{0} sells {1}.", Actor, What);
        Actor.Act(ActOptions.ToCharacter, "You sell {0} for {1} silver and {2} gold piece{3}.", What, silver, gold, Cost == 1 ? string.Empty : "s");

        Actor.UpdateMoney(silver, gold);
        ShopKeeper!.DeductCost(Cost);

        if (What is IItemTrash || What.ItemFlags.IsSet("SellExtract"))
            ItemManager.RemoveItem(What);
        else
        {
            What.ChangeContainer(ShopKeeper!);
            int duration = RandomManager.Range(50, 100);
            What.SetTimer(TimeSpan.FromMinutes(duration));
        }
    }
}
