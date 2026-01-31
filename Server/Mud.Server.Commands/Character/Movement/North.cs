using Mud.Domain;
using Mud.Server.Common.Attributes;
using Mud.Server.GameAction;

namespace Mud.Server.Commands.Character.Movement;

[CharacterCommand("north", "Movement", Priority = 0)]
[Help("Use this command to walk in north direction.")]
public class North : MoveBase
{
    protected override ExitDirections Direction => ExitDirections.North;
}
