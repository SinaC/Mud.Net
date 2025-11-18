using Mud.Server.GameAction;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.Commands.Player.Account;

[PlayerCommand("save", "Account", Priority = 999 /*low priority*/, NoShortcut = true)]
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
    }
}
