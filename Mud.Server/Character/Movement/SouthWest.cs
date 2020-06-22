using Mud.Domain;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.Character.Movement
{
    [CharacterCommand("southwest", "Movement", Priority = 1, MinPosition = Positions.Standing)]
    [Alias("sw")]
    public class SouthWest : CharacterGameAction
    {
        public override void Execute(IActionInput actionInput)
        {
            Actor.Move(ExitDirections.SouthWest, true);
        }
    }
}
