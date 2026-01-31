using Mud.Server.GameAction;

namespace Mud.Server.Commands.Admin.Avatar;

[AdminCommand("infinite", "Avatar", "Immortal")]
[Syntax("[cmd]")]
public class Infinite : ImmortalBase
{
    protected override string Flag => "Infinite";
}
