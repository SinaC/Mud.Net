﻿using Mud.Server.Common;
using Mud.Server.GameAction;
using Mud.Server.Helpers;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.Admin;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Player;

namespace Mud.Server.Admin.Punish
{
    [AdminCommand("snoop", "Punish")]
    [Syntax("[cmd] <player name>")]
    public class Snoop : AdminGameAction
    {
        private IPlayerManager PlayerManager { get; }
        private IWiznet Wiznet { get; }

        public IPlayer Whom { get; protected set; }

        public Snoop(IPlayerManager playerManager, IWiznet wiznet)
        {
            PlayerManager = playerManager;
            Wiznet = wiznet;
        }

        public override string Guards(IActionInput actionInput)
        {
            string baseGuards = base.Guards(actionInput);
            if (baseGuards != null)
                return baseGuards;

            if (actionInput.Parameters.Length == 0)
                return BuildCommandSyntax();

            Whom = FindHelpers.FindByName(PlayerManager.Players, actionInput.Parameters[0]);
            if (Whom == null)
                return StringHelpers.CharacterNotFound;

            if (Whom.SnoopBy != null)
                return "Busy already.";

            // Check snoop loop
            for (IAdmin snooper = Actor.SnoopBy; snooper != null; snooper = snooper.SnoopBy)
                if (snooper == Whom)
                    return "No snoop loops.";

            return null;
        }

        public override void Execute(IActionInput actionInput)
        {
            if (Whom == Actor)
            {
                Actor.Send("Cancelling all snoops.");
                Wiznet.Wiznet($"{Actor.DisplayName} stops being such as snoop.", Domain.WiznetFlags.Punish | Domain.WiznetFlags.Snoops);
                foreach (IPlayer player in PlayerManager.Players)
                {
                    if (player.SnoopBy == Actor)
                        player.SetSnoopBy(null);
                }
                return;
            }

            Wiznet.Wiznet($"{Actor.DisplayName} starts snooping {Whom.DisplayName}.", Domain.WiznetFlags.Snoops | Domain.WiznetFlags.Punish);

            Whom.SetSnoopBy(Actor);
            Actor.Send("Ok.");
        }
    }
}
