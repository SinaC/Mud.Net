using Mud.Domain;
using Mud.Server.Common.Attributes;
using Mud.Server.GameAction;

namespace Mud.Server.Commands.Character.Movement;

[CharacterCommand("northeast", "Movement", Priority = 1)]
[Alias("ne")]
[Help("Use this command to walk in north east direction.")]
public class NorthEast : MoveBase
{
    protected override ExitDirections Direction => ExitDirections.NorthEast;
}
