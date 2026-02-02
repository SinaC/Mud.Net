using Mud.Domain;
using Mud.Server.Domain.Attributes;
using Mud.Server.Common.Helpers;
using Mud.Server.GameAction;
using Mud.Server.Guards.CharacterGuards;
using Mud.Server.Guards.Interfaces;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Item;

namespace Mud.Server.Commands.Character.Movement;

[CharacterCommand("enter", "Movement")]
[Syntax("[cmd] <portal>")]
[Help(
@"This command will allow you to step into (walk through) a portal.")]
public class Enter : CharacterGameAction
{
    protected override IGuard<ICharacter>[] Guards => [new RequiresMinPosition(Positions.Resting), new CannotBeInCombat(), new RequiresAtLeastOneArgument { Message = "Nope, can't do it." }];

    private IItemPortal What { get; set; } = default!;

    public override string? CanExecute(IActionInput actionInput)
    {
        var baseGuards = base.CanExecute(actionInput);
        if (baseGuards != null)
            return baseGuards;

        var item = FindHelpers.FindItemHere(Actor, actionInput.Parameters[0]);
        if (item == null)
            return StringHelpers.ItemNotFound;
        if (item is not IItemPortal what)
            return "You can't seem to find a way in.";
        What = what;
        return null;
    }

    public override void Execute(IActionInput actionInput)
    {
        Actor.Enter(What, false, true);
    }
}
