using Mud.Domain;
using Mud.Server.Common.Attributes;
using Mud.Server.Common.Helpers;
using Mud.Server.GameAction;
using Mud.Server.Guards.Attributes;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Item;

namespace Mud.Server.Commands.Character.Movement;

[CharacterCommand("enter", "Movement"), MinPosition(Positions.Standing), NotInCombat]
[Syntax("[cmd] <portal>")]
[Help(
@"This command will allow you to step into (walk through) a portal.")]
public class Enter : CharacterGameAction
{
    protected IItemPortal What { get; set; } = default!;

    public override string? Guards(IActionInput actionInput)
    {
        var baseGuards = base.Guards(actionInput);
        if (baseGuards != null)
            return baseGuards;

        if (actionInput.Parameters.Length == 0)
            return "Nope, can't do it.";
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
