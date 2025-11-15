using Mud.Domain;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.Character.Combat;

[CharacterCommand("flee", "Combat", MinPosition = Positions.Standing)]
public class Flee : CharacterGameAction
{
    public override string? Guards(IActionInput actionInput)
    {
        var baseGuards = base.Guards(actionInput);
        if (baseGuards != null)
            return baseGuards;

        if (Actor.Fighting == null)
            return "You aren't fighting anyone.";
        return null;
    }

    public override void Execute(IActionInput actionInput)
    {
        Actor.Flee();
    }
}
