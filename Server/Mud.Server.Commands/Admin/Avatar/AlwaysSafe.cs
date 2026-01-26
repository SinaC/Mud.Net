using Mud.Server.GameAction;
using Mud.Server.Guards.Attributes;

namespace Mud.Server.Commands.Admin.Avatar;

[AdminCommand("alwayssafe", "Avatar", "Immortal"), MustBeImpersonated]
[Syntax("[cmd]")]
public class AlwaysSafe : ImmortalBase
{
    protected override string Flag => "AlwaysSafe";
}
