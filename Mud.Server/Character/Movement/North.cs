using Mud.Domain;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.Character.Movement;

[CharacterCommand("north", "Movement", Priority = 0, MinPosition = Positions.Standing, NotInCombat = true)]
public class North : CharacterGameAction
{
    public override void Execute(IActionInput actionInput)
    {
        Actor.Move(ExitDirections.North, false, true);
    }
}
