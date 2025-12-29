using Mud.Domain;
using Mud.Server.Common.Attributes;
using Mud.Server.GameAction;
using Mud.Server.Guards.Attributes;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.Commands.Character.Movement;

[CharacterCommand("west", "Movement", Priority = 0), MinPosition(Positions.Standing), NotInCombat]
[Help("Use this command to walk in west direction.")]
public class West : CharacterGameAction
{
    public override void Execute(IActionInput actionInput)
    {
        Actor.Move(ExitDirections.West, false, true);
    }
}
