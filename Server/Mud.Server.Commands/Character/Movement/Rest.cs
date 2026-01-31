using Mud.Domain;
using Mud.Server.Common.Helpers;
using Mud.Server.GameAction;
using Mud.Server.Guards.CharacterGuards;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Guards;
using Mud.Server.Interfaces.Item;

namespace Mud.Server.Commands.Character.Movement;

[CharacterCommand("rest", "Movement")]
[Syntax(
        "[cmd]",
        "[cmd] <furniture>")]
public class Rest : CharacterGameAction
{
    protected override IGuard<ICharacter>[] Guards => [new RequiresMinPosition(Positions.Sleeping), new CannotBeInCombat { Message = "Maybe you should finish fighting first?" }];

    private IItemFurniture What { get; set; } = default!;

    public override string? CanExecute(IActionInput actionInput)
    {
        var baseGuards = base.CanExecute(actionInput);
        if (baseGuards != null)
            return baseGuards;

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
