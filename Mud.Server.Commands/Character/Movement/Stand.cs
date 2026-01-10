using Mud.Domain;
using Mud.Server.Common.Helpers;
using Mud.Server.GameAction;
using Mud.Server.Guards.Attributes;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Item;
using System.Text;

namespace Mud.Server.Commands.Character.Movement;

[CharacterCommand("stand", "Movement"), MinPosition(Positions.Sleeping)]
[Syntax(
    "[cmd]",
    "[cmd] <furniture>")]
public class Stand : CharacterGameAction
{
    protected IItemFurniture What { get; set; } = default!;

    public override string? Guards(IActionInput actionInput)
    {
        var baseGuards = base.Guards(actionInput);
        if (baseGuards != null)
            return baseGuards;

        if (Actor.Daze > 0)
            return "You're too dazed to re-orient yourself right now!";

        // Stand is the only movement command that can be used while fighting
        //if (Actor.Fighting != null)
        //    return "Maybe you should finish fighting first?";
        if (Actor.Position == Positions.Standing)
            return "You are already standing.";
        if (Actor.Position == Positions.Sleeping && Actor.CharacterFlags.IsSet("Sleep"))
            return "You can't wake up!";

        // Search valid furniture if any
        if (actionInput.Parameters.Length > 0)
        {
            var item = FindHelpers.FindByName(Actor.Room.Content.Where(x => Actor.CanSee(x)), actionInput.Parameters[0]);
            if (item == null)
                return StringHelpers.ItemNotFound;

            if (item is not IItemFurniture furniture || !furniture.CanStand)
                return "You can't seem to find a place to stand.";

            if (1 + furniture.People?.Count() > furniture.MaxPeople)
                return Actor.ActPhrase("There is no more room to stand on {0}.", furniture);
            What = furniture;
            return null;
        }

        return null;
    }

    public override void Execute(IActionInput actionInput)
    {
        // Change position
        Actor.DisplayChangePositionMessage(Actor.Position, Positions.Standing, What);
        Actor.ChangePosition(Positions.Standing);
        Actor.ChangeFurniture(What);
    }
}
