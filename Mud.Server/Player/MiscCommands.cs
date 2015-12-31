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
            foreach (IPlayer player in Server.Instance.GetPlayers())
            {
                switch (player.PlayerState)
                {
                    case PlayerStates.Connecting:
                    case PlayerStates.Connected:
                    case PlayerStates.CreatingAvatar:
                        Send("[OOG] {0}" + Environment.NewLine, player.DisplayName);
                        break;
                    case PlayerStates.Playing:
                        Send("[ IG] {0}" + Environment.NewLine, player.DisplayName);
                        break;
                }
            }
            return true;
        }
    }
}
