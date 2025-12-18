using Mud.Common;
using Mud.Server.Common.Attributes;
using Mud.Server.GameAction;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.GameAction;
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
    public List(ITimeManager timeManager)
        : base(timeManager)
    {
    }

    public override string? Guards(IActionInput actionInput)
    {
        var baseGuards = base.Guards(actionInput);
        if (baseGuards != null)
            return baseGuards;

        if (!Keeper.shopKeeper.Inventory.Any(x => Actor.CanSee(x)))
            return "You can't buy anything here.";
        return null;
    }

    public override void Execute(IActionInput actionInput)
    {
        StringBuilder sb = new ();
        sb.AppendLine("[Lvl Price Qty] Item");
        foreach (var itemAndCost in Keeper.shopKeeper.Inventory
            .Where(x => Actor.CanSee(x))
            .GroupBy(x => x.Blueprint.Id)
            .Select(g => new
            {
                item = g.First(),
                cost = GetBuyCost(Keeper.shopBlueprint, g.First()),
                count = g.Count()
            }))
        {
            if (itemAndCost.item.ItemFlags.IsSet("Inventory"))
                sb.AppendFormatLine("[{0,3} {1,5} -- ] {2}", itemAndCost.item.Level, itemAndCost.cost, itemAndCost.item.DisplayName);
            else
                sb.AppendFormatLine("[{0,3} {1,5} {2,2} ] {3}", itemAndCost.item.Level, itemAndCost.cost, itemAndCost.count, itemAndCost.item.DisplayName);
        }
        Actor.Send(sb);
    }
}
