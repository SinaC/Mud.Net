using Mud.Domain;
using Mud.Server.GameAction;
using Mud.Server.Guards.Attributes;

namespace Mud.Server.Commands.Admin.Avatar;

[AdminCommand("AlwaysSafe", "Avatar", "Immortal"), MustBeImpersonated]
[Syntax("[cmd]")]
public class AlwaysSafe : ImmortalBase
{
    protected override ImmortalModeFlags Flag => ImmortalModeFlags.AlwaysSafe;
}
