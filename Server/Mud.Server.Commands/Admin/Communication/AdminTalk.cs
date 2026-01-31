using Mud.Server.Common.Attributes;
using Mud.Server.GameAction;
using Mud.Server.Guards.AdminGuards;
using Mud.Server.Interfaces.Admin;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Guards;

namespace Mud.Server.Commands.Admin.Communication;

[AdminCommand("admintalk", "Communication")]
[Alias("atalk")]
[Alias(":")]
[Syntax("[cmd] <message>")]
[Help(
@"[cmd] sends a message to all admins.
Using this command with no argument turns off the admin channel (or
turns it back on).")]
public class AdminTalk : AdminGameAction
{
    protected override IGuard<IAdmin>[] Guards => [new RequiresAtLeastOneArgument { Message = "What do you want to say on admin channel ?" }];

    private IAdminManager AdminManager { get; }

    public AdminTalk(IAdminManager adminManager)
    {
        AdminManager = adminManager;
    }

    private string What { get; set; } = default!;

    public override string? CanExecute(IActionInput actionInput)
    {
        var baseGuards = base.CanExecute(actionInput);
        if (baseGuards != null)
            return baseGuards;
        
        What = $"%c%[%y%{Actor.DisplayName}%c%]: {actionInput.Parameters[0].Value}%x%";

        return null;
    }

    public override void Execute(IActionInput actionInput)
    {
        foreach (IAdmin admin in AdminManager.Admins)
            admin.Send(What);
    }
}
