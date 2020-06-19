using Mud.Domain;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.Character.Movement
{
    [CharacterCommand("southwest", "Movement", Priority = 2, MinPosition = Positions.Standing)]
    [CharacterCommand("sw", "Movement", Priority = 1, MinPosition = Positions.Standing)]
    public class SouthWest : CharacterGameAction
    {
        public override void Execute(IActionInput actionInput)
        {
            Actor.Move(ExitDirections.SouthWest, true);
        }
    }
}
