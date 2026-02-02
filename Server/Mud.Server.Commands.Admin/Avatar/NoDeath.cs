using Mud.Server.Domain.Attributes;

namespace Mud.Server.Commands.Admin.Avatar;

[AdminCommand("nodeath", "Avatar", "Immortal")]
[Syntax("[cmd]")]
public class NoDeath : ImmortalBase
{
    protected override string Flag => "NoDeath";
}
