using Mud.Domain;
using Mud.Server.Common.Attributes;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.Commands.Character.Movement;

[CharacterCommand("northeast", "Movement", Priority = 1, MinPosition = Positions.Standing, NotInCombat = true)]
[Alias("ne")]
[Help("Use this command to walk in north east direction.")]
public class NorthEast : CharacterGameAction
{
    public override void Execute(IActionInput actionInput)
    {
        Actor.Move(ExitDirections.NorthEast, false, true);
    }
}
