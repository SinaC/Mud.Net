using Mud.Server.GameAction;

namespace Mud.Server.Commands.Admin.Avatar;

[AdminCommand("passthru", "Avatar", "Immortal")]
[Syntax("[cmd]")]
public class PassThru : ImmortalBase
{
    protected override string Flag => "PassThru";
}
