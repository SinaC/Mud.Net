using Mud.Domain;
using Mud.Server.Common.Helpers;
using Mud.Server.GameAction;
using Mud.Server.Guards.CharacterGuards;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Guards;
using Mud.Server.Interfaces.Item;

namespace Mud.Server.Commands.Character.Movement;

[CharacterCommand("sleep", "Movement")]
[Syntax(
      "[cmd]",
      "[cmd] <furniture>")]
public class Sleep : CharacterGameAction
{
    protected override IGuard<ICharacter>[] Guards => [new RequiresMinPosition(Positions.Sleeping), new CannotBeInCombat { Message = "Maybe you should finish fighting first?" }];

    private IItemFurniture? What { get; set; } = default!;

    public override string? CanExecute(IActionInput actionInput)
    {
        var baseGuards = base.CanExecute(actionInput);
        if (baseGuards != null)
            return baseGuards;

        if (Actor.Position == Positions.Sleeping)
            return "You are already sleeping.";

        // If already on a furniture and no parameter specified, use that furniture
        // Search valid furniture if any
        if (actionInput.Parameters.Length != 0)
        {
            var item = FindHelpers.FindByName(Actor.Room.Content.Where(Actor.CanSee), actionInput.Parameters[0]);
            if (item == null)
                return StringHelpers.ItemNotFound;

            if (item is not IItemFurniture furniture)
                return "You can't sleep on that.";
            What = furniture;
        }
        else
            What = Actor.Furniture;

        if (What == null)
            return null;

        // Check furniture validity
        if (!What.CanSleep)
            return "You can't sleep on that.";

        // If already on furniture, don't count
        if (What != Actor.Furniture && 1 + What.People?.Count() > What.MaxPeople)
            return Actor.ActPhrase("There is no more room on {0} for you.", What);

        return null;
    }

    public override void Execute(IActionInput actionInput)
    {
        // Change position
        Actor.DisplayChangePositionMessage(Actor.Position, Positions.Sleeping, What);
        Actor.ChangePosition(Positions.Sleeping);
        Actor.ChangeFurniture(What);
    }
}
