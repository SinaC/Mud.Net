using Mud.Domain;
using Mud.Server.Common;
using Mud.Server.Common.Helpers;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Item;

namespace Mud.Server.Commands.Character.Item;

[CharacterCommand("fill", "Drink", MinPosition = Positions.Resting)]
[Syntax("[cmd] <container>")]
[Help(@"[cmd] fills a drink container with water.")]
public class Fill : CharacterGameAction
{
    protected IItemFountain Fountain { get; set; } = default!;
    protected IItemDrinkContainer DrinkContainer { get; set; } = default!;

    public override string? Guards(IActionInput actionInput)
    {
        var baseGuards = base.Guards(actionInput);
        if (baseGuards != null)
            return baseGuards;

        if (actionInput.Parameters.Length == 0)
            return "Fill what?";

        // search drink container
        var item = FindHelpers.FindByName(Actor.Inventory, actionInput.Parameters[0]);
        if (item == null)
            return "You do not have that item.";

        // search fountain
        var fountain = Actor.Room.Content.OfType<IItemFountain>().FirstOrDefault();
        if (fountain == null)
            return "There is no fountain here!";

        // drink container?
        if (item is not IItemDrinkContainer drinkContainer)
            return "You can't fill that.";

        // same liquid ?
        if (!drinkContainer.IsEmpty && drinkContainer.LiquidName != fountain.LiquidName)
            return "There is already another liquid in it.";

        // full
        if (drinkContainer.LiquidLeft >= drinkContainer.MaxLiquid)
            return "Your container is full.";

        return null;
    }

    public override void Execute(IActionInput actionInput)
    {
        Actor.Act(ActOptions.ToAll, "{0:N} fill{0:v} {1} with {2} from {3}.", Actor, DrinkContainer, Fountain.LiquidName ?? "mysterious liquid", Fountain);
        DrinkContainer.Fill(Fountain.LiquidName, DrinkContainer.MaxLiquid);
        DrinkContainer.Recompute();
    }
}
