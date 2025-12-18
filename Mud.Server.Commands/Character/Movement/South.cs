using Mud.Domain;
using Mud.Server.Common.Attributes;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.Commands.Character.Movement;

[CharacterCommand("south", "Movement", Priority = 0, MinPosition = Positions.Standing, NotInCombat = true)]
[Help("Use this command to walk in south direction.")]
public class South : CharacterGameAction
{
    public override void Execute(IActionInput actionInput)
    {
        Actor.Move(ExitDirections.South, false, true);
    }
}
