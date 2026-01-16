using Mud.Blueprints.Character;
using Mud.Common;
using Mud.Server.Common.Attributes;
using Mud.Server.GameAction;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.GameAction;
using Mud.Random;
using System.Text;

namespace Mud.Server.Commands.Character.PlayableCharacter.Shop;

[PlayableCharacterCommand("list", "Shop")]
[Syntax(
    "[cmd]",
    "[cmd] <name>")]
[Help(
@"[cmd] lists the objects the shop keeper will sell you.
List <name> shows you only objects of that name.")]
public class List : ShopPlayableCharacterGameActionBase
{
    public List(ITimeManager timeManager, IAbilityManager abilityManager, IRandomManager randomManager)
        : base(timeManager, abilityManager, randomManager)
    {
    }

    public override string? Guards(IActionInput actionInput)
    {
        var baseGuards = base.Guards(actionInput);
        if (baseGuards != null)
            return baseGuards;

        if (Keeper.shopBlueprintBase is CharacterShopBlueprint shopBlueprint)
        {
            if (!Keeper.shopKeeper.Inventory.Any(Actor.CanSee))
                return "You can't buy anything here.";
        }
        else if (Keeper.shopBlueprintBase is CharacterPetShopBlueprint petShopBlueprint)
        {
            if (petShopBlueprint.PetBlueprints.Count == 0)
                return "You can't buy anything here.";
        }

        return null;
    }

    public override void Execute(IActionInput actionInput)
    {
        StringBuilder sb = new ();
        if (Keeper.shopBlueprintBase is CharacterShopBlueprint shopBlueprint)
        {
            sb.AppendLine("[Lvl Price Qty] Item");
            foreach (var itemAndCost in Keeper.shopKeeper.Inventory
                .Where(Actor.CanSee)
                .GroupBy(x => x.Blueprint.Id)
                .Select(g => new
                {
                    item = g.First(),
                    cost = GetBuyCost(Keeper.shopKeeper, shopBlueprint, g.First(), false),
                    count = g.Count()
                }))
            {
                if (itemAndCost.item.ItemFlags.IsSet("Inventory"))
                    sb.AppendFormatLine("[{0,3} {1,5} -- ] {2}", itemAndCost.item.Level, itemAndCost.cost, itemAndCost.item.DisplayName);
                else
                    sb.AppendFormatLine("[{0,3} {1,5} {2,2} ] {3}", itemAndCost.item.Level, itemAndCost.cost, itemAndCost.count, itemAndCost.item.DisplayName);
            }
        }
        else if (Keeper.shopBlueprintBase is CharacterPetShopBlueprint petShopBlueprint)
        {
            sb.AppendLine("[Lvl Price] Pet");
            foreach (var petBlueprint in petShopBlueprint.PetBlueprints)
            {
                sb.AppendFormatLine("[{0,3} {1,5}] {2}", petBlueprint.Level, 10 * petBlueprint.Level * petBlueprint.Level, petBlueprint.ShortDescription);
            }
        }
        Actor.Send(sb);
    }
}
