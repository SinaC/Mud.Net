using Mud.Domain;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.Character.Movement
{
    [CharacterCommand("west", "Movement", Priority = 0, MinPosition = Positions.Standing, NotInCombat = true)]
    public class West : CharacterGameAction
    {
        public override void Execute(IActionInput actionInput)
        {
            Actor.Move(ExitDirections.West, true);
        }
    }
}
