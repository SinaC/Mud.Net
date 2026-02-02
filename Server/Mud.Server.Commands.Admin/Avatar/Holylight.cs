using Mud.Server.Domain.Attributes;

namespace Mud.Server.Commands.Admin.Avatar;

[AdminCommand("holylight", "Avatar", "Immortal")]
[Syntax("[cmd]")]
public class Holylight : ImmortalBase
{
    protected override string Flag => "Holylight";
}
