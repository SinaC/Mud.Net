using Mud.Domain;
using Mud.Server.Domain.Attributes;

namespace Mud.Server.Commands.Character.Movement;

[CharacterCommand("southwest", "Movement", Priority = 1)]
[Alias("sw")]
[Help("Use this command to walk in south west direction.")]
public class SouthWest : MoveBase
{
    protected override ExitDirections Direction => ExitDirections.SouthWest;
}
