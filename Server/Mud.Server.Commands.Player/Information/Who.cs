using Mud.Common;
using Mud.Server.Common.Extensions;
using Mud.Server.Domain.Attributes;
using Mud.Server.GameAction;
using Mud.Server.Guards.Interfaces;
using Mud.Server.Interfaces.Admin;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Player;
using System.Text;

namespace Mud.Server.Commands.Player.Information;

[PlayerCommand("who", "Information")]
/* TODO
Syntax: who
Syntax: who <level-range>
Syntax: who <class or race>
Syntax: who <clan name>
Syntax: who <class or race> <level-range> <clan name>
Syntax: who immortals
*/
public class Who : PlayerGameAction
{
    protected override IGuard<IPlayer>[] Guards => [];

    private IPlayerManager PlayerManager { get; }

    public Who(IPlayerManager playerManager)
    {
        PlayerManager = playerManager;
    }

    public override void Execute(IActionInput actionInput)
    {
        StringBuilder sb = new ();
        sb.AppendLine("Players:");
        foreach (IPlayer player in PlayerManager.Players.Where(x => x is not IAdmin)) // only player
        {
            switch (player.PlayerState)
            {
                case PlayerStates.Impersonating:
                    if (player.Impersonating != null)
                        sb.AppendFormatLine("[ IG] {0} playing {1} [lvl: {2} Class: {3} Race: {4}] {5}",
                            player.DisplayName,
                            player.Impersonating.DisplayName,
                            player.Impersonating.Level,
                            player.Impersonating.Classes.DisplayName(),
                            player.Impersonating.Race.DisplayName,
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
        Actor.Page(sb);
    }
}
