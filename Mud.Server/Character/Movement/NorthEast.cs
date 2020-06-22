using Mud.Domain;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.Character.Movement
{
    [CharacterCommand("northeast", "Movement", Priority = 1, MinPosition = Positions.Standing)]
    [Alias("ne")]
    public class NorthEast : CharacterGameAction
    {
        public override void Execute(IActionInput actionInput)
        {
            Actor.Move(ExitDirections.NorthEast, true);
        }
    }
}
