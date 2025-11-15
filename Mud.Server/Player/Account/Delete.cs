using Mud.Repository;
using Mud.Server.Common;
using Mud.Server.GameAction;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.Player.Account;

[PlayerCommand("delete", "Account", Priority = 999, NoShortcut = true)]
[Syntax("[cmd] <password>")]
public class Delete : AccountGameActionBase
{
    private ILoginRepository LoginRepository { get; }
    private IServerPlayerCommand ServerPlayerCommand { get; }

    public Delete(ILoginRepository loginRepository, IServerPlayerCommand serverPlayerCommand)
    {
        LoginRepository = loginRepository;
        ServerPlayerCommand = serverPlayerCommand;
    }

    public override string? Guards(IActionInput actionInput)
    {
        var baseGuards = base.Guards(actionInput);
        if (baseGuards != null)
            return baseGuards;

        if (actionInput.Parameters.Length == 0)
            return BuildCommandSyntax();

        if (!LoginRepository.CheckPassword(Actor.Name, actionInput.Parameters[0].Value))
        {
            Actor.SetGlobalCooldown(10 * Pulse.PulsePerSeconds);
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
        ServerPlayerCommand.Delete(Actor);
    }
}
