using System;
using Mud.Server.Input;

namespace Mud.Server.Player
{
    public partial class Player
    {
        [Command("who")]
        protected virtual bool DoWho(string rawParameters, params CommandParameter[] parameters)
        {
            // TODO: title, additional informations
            Send("Players:" + Environment.NewLine);
            foreach (IPlayer player in Server.Server.Instance.GetPlayers())
            {
                switch (player.PlayerState)
                {
                    case PlayerStates.Impersonating:
                        Send("[ IG] {0}" + Environment.NewLine, player.DisplayName);
                        break;
                    default:
                        Send("[OOG] {0}" + Environment.NewLine, player.DisplayName);
                        break;
                }
            }
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
            Server.Server.Instance.Quit(this);
            return true;
        }
    }
}
