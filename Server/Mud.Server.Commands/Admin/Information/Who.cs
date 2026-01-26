using Mud.Common;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Admin;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Player;
using System.Text;

namespace Mud.Server.Commands.Admin.Information;

[AdminCommand("who", "Information")]
/* TODO
Syntax: who
Syntax: who <level-range>
Syntax: who <class or race>
Syntax: who <clan name>
Syntax: who <class or race> <level-range> <clan name>
Syntax: who immortals
*/
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
        StringBuilder sb = new();
        //
        sb.AppendLine("Players:");
        foreach (var player in PlayerManager.Players.Where(x => x is not IAdmin)) // only player
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
                            player.Impersonating.ImmortalMode.IsNone ? string.Empty : player.Impersonating.ImmortalMode);
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
        foreach (var admin in AdminManager.Admins)
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
                            admin.Impersonating.ImmortalMode.IsNone ? string.Empty : admin.Impersonating.ImmortalMode);
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