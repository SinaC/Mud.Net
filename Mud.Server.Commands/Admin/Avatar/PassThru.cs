using Mud.Domain;
using Mud.Server.GameAction;
using Mud.Server.Guards.Attributes;

namespace Mud.Server.Commands.Admin.Avatar;

[AdminCommand("PassThru", "Avatar", "Immortal"), MustBeImpersonated]
[Syntax("[cmd]")]
public class PassThru : ImmortalBase
{
    protected override ImmortalModeFlags Flag => ImmortalModeFlags.PassThru;
}
