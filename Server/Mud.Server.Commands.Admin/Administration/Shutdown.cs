using Mud.Domain;
using Mud.Server.Domain.Attributes;
using Mud.Server.GameAction;
using Mud.Server.Guards.AdminGuards;
using Mud.Server.Guards.Interfaces;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.Admin;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.Commands.Admin.Administration;

[AdminCommand("shutdown", "Admin", Priority = 999 /*low priority*/, NoShortcut = true)]
[Syntax("[cmd] <delay>")]
[Help(
@"[cmd] shuts down the server and prevents the normal 'startup' script
from restarting it.")]
public class Shutdown : AdminGameAction
{
    protected override IGuard<IAdmin>[] Guards => [new RequiresMinAdminLevel(AdminLevels.Implementor), new CannotBeImpersonated(), new RequiresAtLeastOneArgument()];

    private IServerAdminCommand ServerAdminCommand { get; }

    public Shutdown(IServerAdminCommand serverAdminCommand)
    {
        ServerAdminCommand = serverAdminCommand;
    }

    private int Seconds { get; set; }

    public override string? CanExecute(IActionInput actionInput)
    {
        var baseGuards = base.CanExecute(actionInput);
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
