using Mud.Domain;
using Mud.Server.Common.Helpers;
using Mud.Server.GameAction;
using Mud.Server.Guards.Attributes;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Item;

namespace Mud.Server.Commands.Character.Movement;

[CharacterCommand("rest", "Movement"), MinPosition(Positions.Sleeping)]
[Syntax(
        "[cmd]",
        "[cmd] <furniture>")]
public class Rest : CharacterGameAction
{
    protected IItemFurniture What { get; set; } = default!;

    public override string? Guards(IActionInput actionInput)
    {
        var baseGuards = base.Guards(actionInput);
        if (baseGuards != null)
            return baseGuards;

        if (Actor.Fighting != null)
            return "Maybe you should finish fighting first?";
        if (Actor.Position == Positions.Resting)
            return "You are already resting.";
        if (Actor.Position == Positions.Sleeping && Actor.CharacterFlags.IsSet("Sleep"))
            return "You can't wake up!";

        // Search valid furniture if any
        if (actionInput.Parameters.Length > 0)
        {
            var item = FindHelpers.FindByName(Actor.Room.Content.Where(Actor.CanSee), actionInput.Parameters[0]);
            if (item == null)
                return StringHelpers.ItemNotFound;

            if (item is not IItemFurniture furniture || !furniture.CanRest)
                return "You can't rest on that.";

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
        Actor.DisplayChangePositionMessage(Actor.Position, Positions.Resting, What);
        Actor.ChangePosition(Positions.Resting);
        Actor.ChangeFurniture(What);
    }
}
