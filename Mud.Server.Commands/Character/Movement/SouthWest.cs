using Mud.Domain;
using Mud.Server.Common.Attributes;
using Mud.Server.GameAction;
using Mud.Server.Guards.Attributes;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.Commands.Character.Movement;

[CharacterCommand("southwest", "Movement", Priority = 1), MinPosition(Positions.Standing), NotInCombat]
[Alias("sw")]
[Help("Use this command to walk in south west direction.")]
public class SouthWest : CharacterGameAction
{
    public override void Execute(IActionInput actionInput)
    {
        Actor.Move(ExitDirections.SouthWest, false, true);
    }
}
