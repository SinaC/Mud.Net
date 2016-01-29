using System;
using System.Text;
using Mud.Server.Helpers;
using Mud.Server.Input;

namespace Mud.Server.Player
{
    public partial class Player
    {
        [Command("who")]
        protected virtual bool DoWho(string rawParameters, params CommandParameter[] parameters)
        {
            // TODO: title, additional informations
            StringBuilder sb = new StringBuilder();
            //
            sb.AppendFormatLine("Players:");
            foreach (IPlayer player in Repository.Server.GetPlayers())
            {
                switch (player.PlayerState)
                {
                    case PlayerStates.Impersonating:
                        if (player.Impersonating != null)
                            sb.AppendFormatLine("[ IG] {0} playing {1} [lvl: {2} Class: {3} Race: {4}]",
                                player.DisplayName,
                                player.Impersonating.DisplayName,
                                player.Impersonating.Level,
                                player.Impersonating.Class == null ? "(none)" : player.Impersonating.Class.DisplayName,
                                player.Impersonating.Race == null ? "(none)" : player.Impersonating.Race.DisplayName);
                        else
                            sb.AppendFormatLine("[ IG] {0} playing something", player.DisplayName);
                        break;
                    default:
                        sb.AppendFormatLine("[OOG] {0}", player.DisplayName);
                        break;
                }
            }
            //
            Page(sb);
            return true;
        }

        [Command("qui", Hidden = true)] // TODO: full match
        protected virtual bool DoQui(string rawParameters, params CommandParameter[] parameters)
        {
            Send("If you want to QUIT, spell it out." + Environment.NewLine);
            return true;
        }

        [Command("quit")]
        protected virtual bool DoQuit(string rawParameters, params CommandParameter[] parameters)
        {
            // TODO: in combat check, ...
            Repository.Server.Quit(this);
            return true;
        }
    }
}
