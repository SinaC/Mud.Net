using Mud.Server.GameAction;
using Mud.Server.Guards.Attributes;

namespace Mud.Server.Commands.Admin.Avatar;

[AdminCommand("passthru", "Avatar", "Immortal"), MustBeImpersonated]
[Syntax("[cmd]")]
public class PassThru : ImmortalBase
{
    protected override string Flag => "PassThru";
}
