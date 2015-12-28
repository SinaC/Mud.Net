using System;
using Mud.Server.Input;

namespace Mud.Server.Player
{
    public partial class Player
    {
        [Command("who")]
        protected virtual bool DoWho(string rawParameters, CommandParameter[] parameters)
        {
            Send("Who:");
            foreach (IPlayer player in WorldTest.Instance.GetPlayers())
            {
                switch (player.PlayerState)
                {
                    case PlayerStates.Connecting:
                        Send("[OOG] {0} connecting", player.DisplayName);
                        break;
                    case PlayerStates.Connected:
                        Send("[OOG] {0}", player.DisplayName);
                        break;
                    case PlayerStates.CreatingAvatar:
                        Send("[OOG] {0} creating an avatar", player.DisplayName);
                        break;
                    case PlayerStates.Playing:
                        if (player.Impersonating != null)
                            Send("[ IG] {0} playing {1}", player.DisplayName, player.Impersonating.Name);
                        else
                            Send("[ IG] {0} playing ???", player.DisplayName);
                        break;
                }
            }
            return true;
        }
    }
}
