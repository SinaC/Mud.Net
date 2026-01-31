using Mud.Domain;
using Mud.Server.Common.Attributes;
using Mud.Server.GameAction;

namespace Mud.Server.Commands.Character.Movement;

[CharacterCommand("south", "Movement", Priority = 0)]
[Help("Use this command to walk in south direction.")]
public class South : MoveBase
{
    protected override ExitDirections Direction => ExitDirections.South;
}
