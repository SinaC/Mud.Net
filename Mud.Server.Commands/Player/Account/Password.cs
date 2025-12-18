using Mud.Repository.Interfaces;
using Mud.Server.Common;
using Mud.Server.Common.Attributes;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.Commands.Player.Account;

[PlayerCommand("password", "Account", Priority = 999, NoShortcut = true)]
[Syntax("[cmd] <old-password> <new-password>")]
[Help(
@"[cmd] changes your character's password.  The first argument must be
your old password.  The second argument is your new password.

The [cmd] command is protected against being snooped or logged.")]
public class Password : AccountGameActionBase
{
    private ILoginRepository LoginRepository { get; }

    public Password(ILoginRepository loginRepository)
    {
        LoginRepository = loginRepository;
    }

    protected string NewPassword { get; set; } = null!;

    public override string? Guards(IActionInput actionInput)
    {
        var baseGuards = base.Guards(actionInput);
        if (baseGuards != null)
            return baseGuards;

        if (actionInput.Parameters.Length != 2)
            return BuildCommandSyntax();
        NewPassword = actionInput.Parameters[1].Value;
        if (NewPassword.Length < 5)
            return "New password must be at least five characters long.";
        if (!LoginRepository.CheckPassword(Actor.Name, actionInput.Parameters[0].Value))
        {
            Actor.SetLag(10 * Pulse.PulsePerSeconds);
            return "Wrong password. Wait 10 seconds.";
        }
        return null;
    }

    public override void Execute(IActionInput actionInput)
    {
        LoginRepository.ChangePassword(Actor.Name, NewPassword);
        NewPassword = null!;
    }
}
