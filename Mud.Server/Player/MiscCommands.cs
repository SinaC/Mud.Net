using Mud.Server.Input;

namespace Mud.Server.Player
{
    public partial class Player
    {
        [Command("who")]
        protected virtual bool DoWho(string rawParameters, params CommandParameter[] parameters)
        {
            // TODO: title, additional informations
            Send("Players:");
            foreach (IPlayer player in World.World.Instance.GetPlayers())
            {
                switch (player.PlayerState)
                {
                    case PlayerStates.Connecting:
                    case PlayerStates.Connected:
                    case PlayerStates.CreatingAvatar:
                        Send("[OOG] {0}", player.DisplayName);
                        break;
                    case PlayerStates.Playing:
                        Send("[ IG] {0}", player.DisplayName);
                        break;
                }
            }
            return true;
        }
    }
}
