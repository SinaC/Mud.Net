using Mud.Domain;
using Mud.Server.Common.Attributes;
using Mud.Server.GameAction;
using Mud.Server.Guards.Attributes;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.Commands.Admin.Administration;

[AdminCommand("shutdown", "Admin", Priority = 999 /*low priority*/, NoShortcut = true), MinAdminLevel(AdminLevels.Implementor), CannotBeImpersonated, NoArgumentGuard]
[Syntax("[cmd] <delay>")]
[Help(
@"[cmd] shuts down the server and prevents the normal 'startup' script
from restarting it.")]
public class Shutdown : AdminGameAction
{
    private IServerAdminCommand ServerAdminCommand { get; }

    public Shutdown(IServerAdminCommand serverAdminCommand)
    {
        ServerAdminCommand = serverAdminCommand;
    }

    protected int Seconds { get; set; }

    public override string? Guards(IActionInput actionInput)
    {
        var baseGuards = base.Guards(actionInput);
        if (baseGuards != null)
            return baseGuards;

        if (!actionInput.Parameters[0].IsNumber)
            return BuildCommandSyntax();
        var seconds = actionInput.Parameters[0].AsNumber;
        if (seconds < 30)
            return "You cannot shutdown that fast.";

        Seconds = seconds;
        return null;
    }

    public override void Execute(IActionInput actionInput)
    {
        ServerAdminCommand.Shutdown(Seconds);
    }
}
