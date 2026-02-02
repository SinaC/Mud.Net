using Mud.Domain;
using Mud.Server.Domain.Attributes;

namespace Mud.Server.Commands.Character.Movement;

[CharacterCommand("southeast", "Movement", Priority = 1)]
[Alias("se")]
[Help("Use this command to walk in south east direction.")]
public class SouthEast : MoveBase
{
    protected override ExitDirections Direction => ExitDirections.SouthEast;
}
