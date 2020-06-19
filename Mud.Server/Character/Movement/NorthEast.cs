using Mud.Domain;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.Character.Movement
{
    [CharacterCommand("northeast", "Movement", Priority = 2, MinPosition = Positions.Standing)]
    [CharacterCommand("ne", "Movement", Priority = 1, MinPosition = Positions.Standing)]
    public class NorthEast : CharacterGameAction
    {
        public override void Execute(IActionInput actionInput)
        {
            Actor.Move(ExitDirections.NorthEast, true);
        }
    }
}
