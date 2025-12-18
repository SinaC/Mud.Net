using Mud.Server.Common;
using Mud.Server.GameAction;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.Commands.Player.Account;

[PlayerCommand("save", "Account", Priority = 999 /*low priority*/, NoShortcut = true)]
[Help(
@"[cmd] saves your character and object.  The game saves your character every
15 minutes regardless, and is the preferred method of saving.  Typing save
will block all other command for about 20 seconds, so use it sparingly.
(90+ players all typing save every 30 seconds just generated too much lag)")]
public class Save : AccountGameActionBase
{
    private IServerPlayerCommand ServerPlayerCommand { get; }

    public Save(IServerPlayerCommand serverPlayerCommand)
    {
        ServerPlayerCommand = serverPlayerCommand;
    }

    public override void Execute(IActionInput actionInput)
    {
        ServerPlayerCommand.Save(Actor);
        Actor.Send("Saving. Remember that ROM has automatic saving now.");
        Actor.SetLag(20);
    }
}
