using Mud.Server.GameAction;
using Mud.Server.Interfaces.Admin;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.Commands.Admin.Communication;

[AdminCommand("admintalk", "Communication")]
[Alias("atalk")]
[Syntax("[cmd] <message>")]
public class AdminTalk : AdminGameAction
{
    private IAdminManager AdminManager { get; }

    public AdminTalk(IAdminManager adminManager)
    {
        AdminManager = adminManager;
    }

    protected string What { get; set; } = default!;

    public override string? Guards(IActionInput actionInput)
    {
        var baseGuards = base.Guards(actionInput);
        if (baseGuards != null)
            return baseGuards;
        
        if (actionInput.Parameters.Length == 0)
            return "What do you want to say on admin channel ?";

        What = $"%c%[%y%{Actor.DisplayName}%c%]: {actionInput.Parameters[0].Value}%x%";

        return null;
    }

    public override void Execute(IActionInput actionInput)
    {
        foreach (IAdmin admin in AdminManager.Admins)
            admin.Send(What);
    }
}
