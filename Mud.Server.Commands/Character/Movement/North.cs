using Mud.Domain;
using Mud.Server.Common;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.Commands.Character.Movement;

[CharacterCommand("north", "Movement", Priority = 0, MinPosition = Positions.Standing, NotInCombat = true)]
[Help("Use this command to walk in north direction.")]
public class North : CharacterGameAction
{
    public override void Execute(IActionInput actionInput)
    {
        Actor.Move(ExitDirections.North, false, true);
    }
}
