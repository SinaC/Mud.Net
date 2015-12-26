using System;
using System.Linq;

namespace Mud.Server.Player
{
    public partial class Player
    {
        [Command("tell")]
        protected virtual bool Tell(string rawParameters, CommandParameter[] parameters)
        {
            if (parameters.Length < 2)
                Send("Tell whom what ?");
            else
            {
                IPlayer target = WorldTest.Instance.GetPlayer(parameters[0]);
                if (target == null)
                {
                    Send("They aren't here.");
                }
                else
                {
                    string what = CommandHelpers.JoinParameters(parameters.Skip(1));
                    Send("You tell {0}: '{1}\'", target.Name, what);
                    target.Send("{0} tells you '{1}'", Name, what);
                }
            }

            return true;
        }

        [Command("gossip")]
        protected virtual bool Gossip(string rawParameters, CommandParameter[] parameters)
        {
            Send("You gossip '{0}'", rawParameters);
            string other = String.Format("{0} gossips '{1}'", Name, rawParameters);
            foreach(IPlayer player in WorldTest.Instance.GetPlayers().Where(x => x != this))
                player.Send(other);

            return true;
        }
    }
}
