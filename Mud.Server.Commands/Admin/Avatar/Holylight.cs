using Mud.Domain;
using Mud.Server.GameAction;
using Mud.Server.Guards.Attributes;

namespace Mud.Server.Commands.Admin.Avatar;

[AdminCommand("Holylight", "Avatar", "Immortal"), MustBeImpersonated]
[Syntax("[cmd]")]
public class Holylight : ImmortalBase
{
    protected override ImmortalModeFlags Flag => ImmortalModeFlags.Holylight;
}
