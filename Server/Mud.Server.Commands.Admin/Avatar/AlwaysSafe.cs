using Mud.Server.Domain.Attributes;

namespace Mud.Server.Commands.Admin.Avatar;

[AdminCommand("alwayssafe", "Avatar", "Immortal")]
[Syntax("[cmd]")]
public class AlwaysSafe : ImmortalBase
{
    protected override string Flag => "AlwaysSafe";
}
