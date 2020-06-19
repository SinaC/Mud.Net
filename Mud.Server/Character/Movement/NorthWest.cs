using Mud.Domain;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.Character.Movement
{
    [CharacterCommand("northwest", "Movement", Priority = 2, MinPosition = Positions.Standing)]
    [CharacterCommand("nw", "Movement", Priority = 1, MinPosition = Positions.Standing)]
    public class NorthWest : CharacterGameAction
    {
        public override void Execute(IActionInput actionInput)
        {
            Actor.Move(ExitDirections.NorthWest, true);
        }
    }
}
