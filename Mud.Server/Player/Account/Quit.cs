﻿using Mud.Domain;
using Mud.Server.GameAction;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.Player.Account
{
    [PlayerCommand("quit", "Misc", Priority = 999/*low priority*/, NoShortcut = true)]
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
            Wiznet.Wiznet($"{Actor.DisplayName} rejoins the real world.", WiznetFlags.Logins);

            Actor.Save();
            ServerPlayerCommand.Quit(Actor);
        }
    }
}
