using Mud.Domain;
using Mud.Server.Domain.Attributes;

namespace Mud.Server.Commands.Character.Movement;

[CharacterCommand("northwest", "Movement", Priority = 1)]
[Alias("nw")]
[Help("Use this command to walk in north west direction.")]
public class NorthWest : MoveBase
{
    protected override ExitDirections Direction => ExitDirections.NorthWest;
}
