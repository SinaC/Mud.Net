using Mud.Domain;
using Mud.Server.Common.Attributes;
using Mud.Server.GameAction;
using Mud.Server.Guards.Attributes;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.Commands.Character.Movement;

[CharacterCommand("up", "Movement", Priority = 0), MinPosition(Positions.Standing), NotInCombat]
[Help("Use this command to walk in up direction.")]
public class Up : CharacterGameAction
{
    public override void Execute(IActionInput actionInput)
    {
        Actor.Move(ExitDirections.Up, false, true);
    }
}
