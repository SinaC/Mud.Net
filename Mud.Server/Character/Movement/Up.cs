using Mud.Domain;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.Character.Movement
{
    [CharacterCommand("up", "Movement", Priority = 0, MinPosition = Positions.Standing, NotInCombat = true)]
    public class Up : CharacterGameAction
    {
        public override void Execute(IActionInput actionInput)
        {
            Actor.Move(ExitDirections.Up, false, true);
        }
    }
}
