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
    }
}
