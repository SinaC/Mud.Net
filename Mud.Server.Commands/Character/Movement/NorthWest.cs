using Mud.Domain;
using Mud.Server.Common.Attributes;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.Commands.Character.Movement;

[CharacterCommand("northwest", "Movement", Priority = 1, MinPosition = Positions.Standing, NotInCombat = true)]
[Alias("nw")]
[Help("Use this command to walk in north west direction.")]
public class NorthWest : CharacterGameAction
{
    public override void Execute(IActionInput actionInput)
    {
        Actor.Move(ExitDirections.NorthWest, false, true);
    }
}
