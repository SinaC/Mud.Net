using Mud.Domain;
using Mud.Server.Common.Attributes;
using Mud.Server.GameAction;

namespace Mud.Server.Commands.Character.Movement;

[CharacterCommand("east", "Movement", Priority = 0)]
[Help("Use this command to walk in east direction.")]
public class East : MoveBase
{
    protected override ExitDirections Direction => ExitDirections.East;
}
