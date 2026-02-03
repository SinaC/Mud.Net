using Mud.Domain;
using Mud.Server.Domain.Attributes;

namespace Mud.Server.Commands.Character.Movement;

[CharacterCommand("west", "Movement", Priority = 0)]
[Help("Use this command to walk in west direction.")]
public class West : MoveBase
{
    protected override ExitDirections Direction => ExitDirections.West;
}
