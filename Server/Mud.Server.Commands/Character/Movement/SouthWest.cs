using Mud.Domain;
using Mud.Server.Common.Attributes;
using Mud.Server.GameAction;

namespace Mud.Server.Commands.Character.Movement;

[CharacterCommand("southwest", "Movement", Priority = 1)]
[Alias("sw")]
[Help("Use this command to walk in south west direction.")]
public class SouthWest : MoveBase
{
    protected override ExitDirections Direction => ExitDirections.SouthWest;
}
