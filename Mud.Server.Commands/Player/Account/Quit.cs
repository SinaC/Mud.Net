using Mud.Domain;
using Mud.Server.Common;
using Mud.Server.GameAction;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.Commands.Player.Account;

[PlayerCommand("quit", "Account", Priority = 999/*low priority*/, NoShortcut = true)]
[Help(
@"[cmd] leaves the game.  You may [cmd] anywhere.  When you re-enter the game 
you will be back in the same room.

[cmd] automatically does a SAVE, so you can safely leave the game with just one
command.  Nevertheless it's a good idea to SAVE before [cmd].  If you get into
the habit of using [cmd] without SAVE, and then you play some other mud that
doesn't save before quitting, you're going to regret it.")]
public class Quit : AccountGameActionBase
{
    private IServerPlayerCommand ServerPlayerCommand { get; }
    private IWiznet Wiznet { get; }

    public Quit(IServerPlayerCommand serverPlayerCommand, IWiznet wiznet)
    {
        ServerPlayerCommand = serverPlayerCommand;
        Wiznet = wiznet;
    }

    public override void Execute(IActionInput actionInput)
    {
        Actor.Send("Alas, all good things must come to an end.");
        Impersonating?.Act(ActOptions.ToRoom, "{0:N} has left the game.", Impersonating);
        Wiznet.Log($"{Actor.DisplayName} rejoins the real world.", WiznetFlags.Logins);

        ServerPlayerCommand.Quit(Actor);
    }
}
