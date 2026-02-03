using Mud.Domain;
using Mud.Server.Domain.Attributes;

namespace Mud.Server.Commands.Character.Movement;

[CharacterCommand("down", "Movement", Priority = 0)]
[Help("Use this command to walk in down direction.")]
public class Down : MoveBase
{
    protected override ExitDirections Direction => ExitDirections.Down;
}
