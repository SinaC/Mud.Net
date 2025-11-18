using Mud.Domain;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.Commands.Character.Movement;

[CharacterCommand("down", "Movement", Priority = 0, MinPosition = Positions.Standing, NotInCombat = true)]
public class Down : CharacterGameAction
{
    public override void Execute(IActionInput actionInput)
    {
        Actor.Move(ExitDirections.Down, false, true);
    }
}
