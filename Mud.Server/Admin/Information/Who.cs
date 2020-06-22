using Mud.Common;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Admin;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Player;
using System.Linq;
using System.Text;

namespace Mud.Server.Admin.Information
{
    [AdminCommand("who", "Information")]
    public class Who : AdminGameAction
    {
        private IPlayerManager PlayerManager { get; }
        private IAdminManager AdminManager { get; }

        public Who(IPlayerManager playerManager, IAdminManager adminManager)
        {
            PlayerManager = playerManager;
            AdminManager = adminManager;
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
            sb.AppendFormatLine("Admins:");
            foreach (IAdmin admin in AdminManager.Admins)
            {
                switch (admin.PlayerState)
                {
                    case PlayerStates.Impersonating:
                        if (admin.Impersonating != null)
                            sb.AppendFormatLine("[ IG] {0} playing {1} [lvl: {2} Class: {3} Race: {4}] {5}",
                                admin.DisplayName,
                                admin.Impersonating.DisplayName,
                                admin.Impersonating.Level,
                                admin.Impersonating.Class?.DisplayName ?? "(none)",
                                admin.Impersonating.Race?.DisplayName ?? "(none)",
                                admin.Impersonating.IsImmortal ? "{IMMORTAL}" : string.Empty);
                        else if (admin.Incarnating != null)
                            sb.AppendFormatLine("[ IG] [{0}] {1} incarnating {2}", admin.Level, admin.DisplayName, admin.Incarnating.DisplayName);
                        else
                            sb.AppendFormatLine("[ IG] [{0}] {1} neither playing nor incarnating !!!", admin.Level, admin.DisplayName);
                        break;
                    default:
                        sb.AppendFormatLine("[OOG] [{0}] {1} {2}", admin.Level, admin.DisplayName, admin.PlayerState);
                        break;
                }
            }
            //
            Actor.Page(sb);
        }
    }
}