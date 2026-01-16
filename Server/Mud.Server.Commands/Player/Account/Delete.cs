using Microsoft.Extensions.Options;
using Mud.Server.Common;
using Mud.Server.Common.Attributes;
using Mud.Server.GameAction;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Options;

namespace Mud.Server.Commands.Player.Account;

[PlayerCommand("delete", "Account", Priority = 999, NoShortcut = true)]
[Alias("suicide")]
[Syntax("[cmd] <password>")]
[Help(
@"Use the [cmd] command to erase unwanted characters, so the name will be
available for use.  This command must be typed twice to delete a character,
and you cannot be forced to delete.  Typing delete with an argument will
return your character to 'safe' status if you change your mind after the
first delete.")]
public class Delete : AccountGameActionBase
{
    private IServerPlayerCommand ServerPlayerCommand { get; }
    private bool CheckPassword { get; }

    public Delete(IServerPlayerCommand serverPlayerCommand, IOptions<ServerOptions> options)
    {
        ServerPlayerCommand = serverPlayerCommand;
        CheckPassword = options.Value.CheckPassword;
    }

    public override string? Guards(IActionInput actionInput)
    {
        var baseGuards = base.Guards(actionInput);
        if (baseGuards != null)
            return baseGuards;

        if (actionInput.Parameters.Length == 0)
            return BuildCommandSyntax();

        if (CheckPassword && !PasswordHelpers.Check(actionInput.Parameters[0].Value, Actor.Password))
        {
            Actor.SetLag(Pulse.FromSeconds(10));
            return "Wrong password. Wait 10 seconds.";
        }

        return null;
    }

    public override void Execute(IActionInput actionInput)
    {
        if (!Actor.DeletionConfirmationNeeded)
        {
            Actor.Send("Ask you sure you want to delete your account? Use 'delete' again to confirm.");
            Actor.SetDeletionConfirmationNeeded();
            return;
        }
        Actor.Send("Deletion confirmed! Processing...");
        // delete player
        ServerPlayerCommand.Delete(Actor);
    }
}
