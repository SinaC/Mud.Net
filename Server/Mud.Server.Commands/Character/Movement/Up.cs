using Mud.Domain;
using Mud.Server.Common.Attributes;
using Mud.Server.GameAction;

namespace Mud.Server.Commands.Character.Movement;

[CharacterCommand("up", "Movement", Priority = 0)]
[Help("Use this command to walk in up direction.")]
public class Up : MoveBase
{
    protected override ExitDirections Direction => ExitDirections.Up;
}
