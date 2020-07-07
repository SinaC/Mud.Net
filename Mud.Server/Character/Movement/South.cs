using Mud.Domain;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.Character.Movement
{
    [CharacterCommand("south", "Movement", Priority = 0, MinPosition = Positions.Standing, NotInCombat = true)]
    public class South : CharacterGameAction
    {
        public override void Execute(IActionInput actionInput)
        {
            Actor.Move(ExitDirections.South, false, true);
        }
    }
}
