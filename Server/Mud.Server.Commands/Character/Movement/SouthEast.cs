using Mud.Domain;
using Mud.Server.Common.Attributes;
using Mud.Server.GameAction;
using Mud.Server.Guards.Attributes;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.Commands.Character.Movement;

[CharacterCommand("southeast", "Movement", Priority = 1), MinPosition(Positions.Standing), NotInCombat]
[Alias("se")]
[Help("Use this command to walk in south east direction.")]
public class SouthEast : CharacterGameAction
{
    public override void Execute(IActionInput actionInput)
    {
        Actor.Move(ExitDirections.SouthEast, false, true);
    }
}
