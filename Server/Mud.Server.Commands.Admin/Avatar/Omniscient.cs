using Mud.Server.Domain.Attributes;

namespace Mud.Server.Commands.Admin.Avatar;

[AdminCommand("omniscient", "Avatar", "Immortal")]
[Syntax("[cmd]")]
public class Omniscient : ImmortalBase
{
    protected override string Flag => "Omniscient";
}
