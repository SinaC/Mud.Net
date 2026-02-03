using Mud.Domain;
using Mud.Server.Domain.Attributes;

namespace Mud.Server.Commands.Character.Movement;

[CharacterCommand("east", "Movement", Priority = 0)]
[Help("Use this command to walk in east direction.")]
public class East : MoveBase
{
    protected override ExitDirections Direction => ExitDirections.East;
}
