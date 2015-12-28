using System;
using System.Linq;
using Mud.Server.Input;

namespace Mud.Server.Player
{
    public partial class Player
    {
        [Command("tell")]
        protected virtual bool DoTell(string rawParameters, CommandParameter[] parameters)
        {
            if (parameters.Length < 2)
                Send("Tell whom what ?");
            else
            {
                IPlayer target = World.Instance.GetPlayer(parameters[0]);
                if (target == null)
                    Send(StringHelpers.CharacterNotFound);
                else
                {
                    string what = CommandHelpers.JoinParameters(parameters.Skip(1));
                    Send("You tell {0}: '{1}\'", target.DisplayName, what);
                    //target.Send("{0} tells you '" + StringConstants.Blue + "{1}" + StringConstants.Reset + "'", Name, what);
                    target.Send("{0} tells you '%^blue%^{1}%^reset%^'", DisplayName, what);
                }
            }

            return true;
        }

        [Command("gossip")]
        protected virtual bool DoGossip(string rawParameters, CommandParameter[] parameters)
        {
            Send("You gossip '{0}'", rawParameters);
            string other = String.Format("{0} gossips '{1}'", DisplayName, rawParameters);
            foreach(IPlayer player in World.Instance.GetPlayers().Where(x => x != this))
                player.Send(other);

            return true;
        }
    }
}
