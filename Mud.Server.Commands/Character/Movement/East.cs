using Mud.Domain;
using Mud.Server.Common.Attributes;
using Mud.Server.GameAction;
using Mud.Server.Guards.Attributes;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.Commands.Character.Movement;

[CharacterCommand("east", "Movement", Priority = 0), MinPosition(Positions.Standing), NotInCombat]
[Help("Use this command to walk in east direction.")]
public class East : CharacterGameAction
{
    public override void Execute(IActionInput actionInput)
    {
        Actor.Move(ExitDirections.East, false, true);
    }
}
