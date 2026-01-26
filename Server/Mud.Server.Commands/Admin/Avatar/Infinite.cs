using Mud.Server.GameAction;
using Mud.Server.Guards.Attributes;

namespace Mud.Server.Commands.Admin.Avatar;

[AdminCommand("infinite", "Avatar", "Immortal"), MustBeImpersonated]
[Syntax("[cmd]")]
public class Infinite : ImmortalBase
{
    protected override string Flag => "Infinite";
}
