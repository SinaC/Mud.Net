using Mud.Domain;
using Mud.Server.GameAction;
using Mud.Server.Guards.Attributes;

namespace Mud.Server.Commands.Admin.Avatar;

[AdminCommand("omniscient", "Avatar", "Immortal"), MustBeImpersonated]
[Syntax("[cmd]")]
public class Omniscient : ImmortalBase
{
    protected override ImmortalModeFlags Flag => ImmortalModeFlags.Omniscient;
}
