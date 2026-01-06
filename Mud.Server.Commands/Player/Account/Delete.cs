using Mud.Server.Common;
using Mud.Server.Common.Attributes;
using Mud.Server.GameAction;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.GameAction;

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

    public Delete(IServerPlayerCommand serverPlayerCommand)
    {
        ServerPlayerCommand = serverPlayerCommand;
    }

    public override string? Guards(IActionInput actionInput)
    {
        var baseGuards = base.Guards(actionInput);
        if (baseGuards != null)
            return baseGuards;

        if (actionInput.Parameters.Length == 0)
            return BuildCommandSyntax();

        if (Actor.Password != actionInput.Parameters[0].Value) // TODO: encode password
        {
            Actor.SetLag(10 * Pulse.PulsePerSeconds);
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
