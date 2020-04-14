using System.Linq;
using System.Text;
using Mud.Server.Helpers;
using Mud.Server.Input;

namespace Mud.Server.Player
{
    public partial class Player
    {
        [Command("tell", Category = "Communication")]
        protected virtual bool DoTell(string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length < 2)
            {
                Send("Tell whom what ?");
                return true;
            }
            IPlayer whom = Server.GetPlayer(parameters[0], true);
            if (whom == null)
            {
                Send(StringHelpers.CharacterNotFound);
                return true;
            }

            string what = CommandHelpers.JoinParameters(parameters.Skip(1));
            InnerTell(whom, what);

            return true;
        }

        [Command("reply", Category = "Communication")]
        protected virtual bool DoReply(string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length == 0)
            {
                Send("Reply what?");
                return true;
            }
            if (LastTeller == null)
            {
                Send(StringHelpers.CharacterNotFound);
                return true;
            }

            IPlayer whom = LastTeller;
            string what = CommandHelpers.JoinParameters(parameters);
            InnerTell(whom, what);

            return true;
        }

        [Command("gossip", Category = "Communication")]
        protected virtual bool DoGossip(string rawParameters, params CommandParameter[] parameters)
        {
            Send("%m%You gossip '%M%{0}%m%'%x%", rawParameters);
            string other = $"%m%{DisplayName} gossips '%M%{rawParameters}%m%'%x%";
            foreach (IPlayer player in Server.Players.Where(x => x != this))
                player.Send(other);

            return true;
        }

        [Command("question", Category = "Communication")]
        protected virtual bool DoQuestion(string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length == 0)
            {
                Send("Ask what ?");
                return true;
            }

            string what = parameters[0].Value;
            Send("%y%You question '{0}'%x%", what);

            string phrase = $"%y%{DisplayName} questions '{what}'%x%";
            foreach (IPlayer player in Server.Players.Where(x => x != this))
                player.Send(phrase);

            return true;
        }

        [Command("answer", Category = "Communication")]
        protected virtual bool DoAnswer(string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length == 0)
            {
                Send("Answer what ?");
                return true;
            }

            string what = parameters[0].Value;
            Send("%y%You answer '{0}'%x%", what);

            string phrase = $"%y%{DisplayName} answers '{what}'%x%";
            foreach (IPlayer player in Server.Players.Where(x => x != this))
                player.Send(phrase);

            return true;
        }

        [Command("afk", Category = "Communication")]
        protected virtual bool DoAfk(string rawParameters, params CommandParameter[] parameters)
        {
            if (IsAfk)
            {
                Send("%G%AFK%x% removed.");
                if (DelayedTells.Any())
                    Send("%r%You have received tells: Type %Y%'replay'%r% to see them.%x%");
            }
            else
                Send("You are now in %G%AFK%x% mode.");
            IsAfk = !IsAfk;
            return true;
        }

        [Command("replay", Category = "Communication")]
        protected virtual bool DoReplay(string rawParameters, params CommandParameter[] parameters)
        {
            if (DelayedTells.Any())
            {
                StringBuilder sb = new StringBuilder();
                foreach (string sentence in DelayedTells)
                    sb.AppendLine(sentence);
                Page(sb);
                ClearDelayedTells();
            }
            else
                Send("You have no tells to replay.");
            return true;
        }

        // Helpers
        private void InnerTell(IPlayer whom, string what) // used by DoTell and DoReply
        {
            string sentence = $"%g%{DisplayName} tells you '%G%{what}%g%'%x%";
            if (whom.IsAfk)
            {
                Send($"{whom.DisplayName} is AFK, but your tell will go through when {whom.DisplayName} returns.");
                whom.AddDelayedTell(sentence);
            }
            else
            {
                Send("%g%You tell {0}: '%G%{1}%g%'%x%", whom.DisplayName, what);
                whom.Send(sentence);
                whom.SetLastTeller(this);
            }
        }
    }
}
