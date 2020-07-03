using Mud.Domain;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.Character.Movement
{
    [CharacterCommand("northwest", "Movement", Priority = 1, MinPosition = Positions.Standing, NotInCombat = true)]
    [Alias("nw")]
    public class NorthWest : CharacterGameAction
    {
        public override void Execute(IActionInput actionInput)
        {
            Actor.Move(ExitDirections.NorthWest, true);
        }
    }
}
