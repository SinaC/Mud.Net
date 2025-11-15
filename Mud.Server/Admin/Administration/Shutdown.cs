using Mud.Domain;
using Mud.Server.GameAction;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.Admin.Administration;

[AdminCommand("shutdown", "Admin", Priority = 999 /*low priority*/, NoShortcut = true, MinLevel = AdminLevels.Implementor, CannotBeImpersonated = true)]
[Syntax("[cmd] <delay>")]
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

        if (actionInput.Parameters.Length == 0 || !int.TryParse(actionInput.Parameters[0].Value, out int seconds))
            return BuildCommandSyntax();
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
