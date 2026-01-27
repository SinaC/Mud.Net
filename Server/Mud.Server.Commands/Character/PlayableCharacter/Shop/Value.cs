using Mud.Blueprints.Character;
using Mud.Random;
using Mud.Server.Common.Attributes;
using Mud.Server.Common.Helpers;
using Mud.Server.GameAction;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Item;

namespace Mud.Server.Commands.Character.PlayableCharacter.Shop;

[PlayableCharacterCommand("value", "Shop")]
[Syntax("[cmd] <item>")]
[Help(@"[cmd] asks the shop keeper how much he, she, or it will buy the item for.")]
public class Value : ShopPlayableCharacterGameActionBase
{
    public Value(ITimeManager timeManager, IAbilityManager abilityManager, IRandomManager randomManager, IItemManager itemManager)
        : base(timeManager, abilityManager, randomManager)
    {
    }

    protected IItem What { get; set; } = default!;
    protected long Cost { get; set; }

    public override string? Guards(IActionInput actionInput)
    {
        var baseGuards = base.Guards(actionInput);
        if (baseGuards != null)
            return baseGuards;

        if (ShopBlueprintBase is not CharacterShopBlueprint shopBlueprint)
            return "You cannot sell anything here.";

        if (actionInput.Parameters.Length == 0)
            return "Value what?";

        What = FindHelpers.FindByName(Actor.Inventory.Where(Actor.CanSee), actionInput.Parameters[0])!;
        if (What == null)
            return Actor.ActPhrase("{0:N} tells you 'You don't have that item'.", ShopKeeper!);

        if (!ShopKeeper!.CanSee(What))
            return Actor.ActPhrase("{0:N} doesn't see what you are offering.", ShopKeeper);

        if (What.ItemFlags.IsSet("NoDrop"))
            return "You can't let go of it.";

        if (What is IItemQuest)
            return "You cannot value that.";

        Cost = GetSellCost(ShopKeeper, shopBlueprint, What, false);
        if (Cost <= 0 || What.DecayPulseLeft > 0)
            return Actor.ActPhrase("{0:N} looks uninterested in {1}.", ShopKeeper, What);

        return null;
    }

    public override void Execute(IActionInput actionInput)
    {
        long silver = Cost % 100;
        long gold = Cost / 100;

        Actor.Act(ActOptions.ToCharacter, "{0:N} tells you 'I'll give you {1} silver and {2} gold coins for {3}'.", ShopKeeper!, silver, gold, What);
    }
}
