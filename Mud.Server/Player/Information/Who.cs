﻿using Mud.Common;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Admin;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Player;
using System.Linq;
using System.Text;

namespace Mud.Server.Player.Information
{
    [PlayerCommand("who", "Information")]
    public class Who : PlayerGameAction
    {
        private IPlayerManager PlayerManager { get; }

        public Who(IPlayerManager playerManager)
        {
            PlayerManager = playerManager;
        }

        public override void Execute(IActionInput actionInput)
        {
            StringBuilder sb = new StringBuilder();
            //
            sb.AppendLine("Players:");
            foreach (IPlayer player in PlayerManager.Players.Where(x => !(x is IAdmin))) // only player
            {
                switch (player.PlayerState)
                {
                    case PlayerStates.Impersonating:
                        if (player.Impersonating != null)
                            sb.AppendFormatLine("[ IG] {0} playing {1} [lvl: {2} Class: {3} Race: {4}] {5}",
                                player.DisplayName,
                                player.Impersonating.DisplayName,
                                player.Impersonating.Level,
                                player.Impersonating.Class?.DisplayName ?? "(none)",
                                player.Impersonating.Race?.DisplayName ?? "(none)",
                                player.Impersonating.IsImmortal ? "{IMMORTAL}" : string.Empty);
                        else
                            sb.AppendFormatLine("[ IG] {0} playing something", player.DisplayName);
                        break;
                    default:
                        sb.AppendFormatLine("[OOG] {0}", player.DisplayName);
                        break;
                }
            }
            //
            Actor.Page(sb);
        }
    }
}
