using Mud.Domain;
using Mud.Server.GameAction;
using Mud.Server.Guards.Attributes;

namespace Mud.Server.Commands.Admin.Avatar;

[AdminCommand("nodeath", "Avatar", "Immortal"), MustBeImpersonated]
[Syntax("[cmd]")]
public class NoDeath : ImmortalBase
{
    protected override ImmortalModeFlags Flag => ImmortalModeFlags.NoDeath;
}
