using Mud.Domain;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.Character.Movement;

[CharacterCommand("southeast", "Movement", Priority = 1, MinPosition = Positions.Standing, NotInCombat = true)]
[Alias("se")]
public class SouthEast : CharacterGameAction
{
    public override void Execute(IActionInput actionInput)
    {
        Actor.Move(ExitDirections.SouthEast, false, true);
    }
}
