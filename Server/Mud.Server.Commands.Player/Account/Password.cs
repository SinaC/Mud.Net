using Microsoft.Extensions.Options;
using Mud.Server.Common;
using Mud.Server.Domain.Attributes;
using Mud.Server.GameAction;
using Mud.Server.Guards.Interfaces;
using Mud.Server.Guards.PlayerGuards;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Player;
using Mud.Server.Options;

namespace Mud.Server.Commands.Player.Account;

[PlayerCommand("password", "Account", Priority = 999, NoShortcut = true)]
[Syntax("[cmd] <old-password> <new-password>")]
[Help(
@"[cmd] changes your character's password.  The first argument must be
your old password.  The second argument is your new password.

The [cmd] command is protected against being snooped or logged.")]
public class Password : PlayerGameAction
{
    protected override IGuard<IPlayer>[] Guards => [new CannotBeInCombat(), new RequiresAtLeastTwoArguments()];

    private IServerPlayerCommand ServerPlayerCommand { get; }
    private bool CheckPassword { get; }

    public Password(IServerPlayerCommand serverPlayerCommand, IOptions<ServerOptions> options)
    {
        ServerPlayerCommand = serverPlayerCommand;
        CheckPassword = options.Value.CheckPassword;
    }

    private string NewPassword { get; set; } = null!;

    public override string? CanExecute(IActionInput actionInput)
    {
        var baseGuards = base.CanExecute(actionInput);
        if (baseGuards != null)
            return baseGuards;

        if (actionInput.Parameters[1].Value.Length < 5)
            return "New password must be at least five characters long.";

        if (CheckPassword && !PasswordHelpers.Check(actionInput.Parameters[0].Value, Actor.Password))
        {
            Actor.SetLag(Pulse.FromSeconds(10));
            return "Wrong password. Wait 10 seconds.";
        }
        NewPassword = PasswordHelpers.Crypt(actionInput.Parameters[1].Value);
        return null;
    }

    public override void Execute(IActionInput actionInput)
    {
        Actor.ChangePassword(NewPassword);
        ServerPlayerCommand.Save(Actor);
        NewPassword = null!;
    }
}
