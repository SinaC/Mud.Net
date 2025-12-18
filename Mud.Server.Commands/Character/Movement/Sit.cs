using Mud.Domain;
using Mud.Server.Common.Helpers;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Item;

namespace Mud.Server.Commands.Character.Movement;

[CharacterCommand("sit", "Movement", MinPosition = Positions.Sleeping)]
[Syntax(
        "[cmd]",
        "[cmd] <furniture>")]
public class Sit : CharacterGameAction
{
    protected IItemFurniture What { get; set; } = default!;

    public override string? Guards(IActionInput actionInput)
    {
        var baseGuards = base.Guards(actionInput);
        if (baseGuards != null)
            return baseGuards;

        if (Actor.Fighting != null)
            return "Maybe you should finish fighting first?";
        if (Actor.Position == Positions.Sitting)
            return "You are already sitting down.";
        if (Actor.Position == Positions.Sleeping && Actor.CharacterFlags.IsSet("Sleep"))
            return "You can't wake up!";

        // Search valid furniture if any
        if (actionInput.Parameters.Length > 0)
        {
            var item = FindHelpers.FindByName(Actor.Room.Content.Where(x => Actor.CanSee(x)), actionInput.Parameters[0]);
            if (item == null)
                return StringHelpers.ItemNotFound;

            if (item is not IItemFurniture furniture || !furniture.CanSit)
                return "You can't sit on that.";

            if (1 + furniture.People?.Count() > furniture.MaxPeople)
                return Actor.ActPhrase("There is no more room on {0}.", furniture);
            What = furniture;
            return null;
        }

        return null;
    }

    public override void Execute(IActionInput actionInput)
    {
        // Change position
        Actor.DisplayChangePositionMessage(Actor.Position, Positions.Sitting, What);
        Actor.ChangePosition(Positions.Sitting);
        Actor.ChangeFurniture(What);
    }
}
