using System.Linq;
using System.Text;
using Mud.Server.Helpers;
using Mud.Server.Input;
// ReSharper disable UnusedMember.Global

namespace Mud.Server.Player
{
    public partial class Player
    {
        [Command("tell", "Communication")]
        [Syntax("[cmd] <player name> <message>")]
        protected virtual CommandExecutionResults DoTell(string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length < 2)
            {
                Send("Tell whom what ?");
                return CommandExecutionResults.SyntaxErrorNoDisplay;
            }
            IPlayer whom = PlayerManager.GetPlayer(parameters[0], true);
            if (whom == null)
            {
                Send(StringHelpers.CharacterNotFound);
                return CommandExecutionResults.TargetNotFound;
            }

            string what = CommandHelpers.JoinParameters(parameters.Skip(1));
            InnerTell(whom, what);

            return CommandExecutionResults.Ok;
        }

        [Command("reply", "Communication")]
        [Syntax("[cmd] <message>")]
        protected virtual CommandExecutionResults DoReply(string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length == 0)
            {
                Send("Reply what?");
                return CommandExecutionResults.SyntaxErrorNoDisplay;
            }
            if (LastTeller == null)
            {
                Send(StringHelpers.CharacterNotFound);
                return CommandExecutionResults.TargetNotFound;
            }

            IPlayer whom = LastTeller;
            string what = CommandHelpers.JoinParameters(parameters);
            InnerTell(whom, what);

            return CommandExecutionResults.Ok;
        }

        [Command("gossip", "Communication")]
        [Command("ooc", "Communication")]
        [Syntax("[cmd] <message>")]
        protected virtual CommandExecutionResults DoGossip(string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length == 0)
            {
                Send("Gossip what ?");
                return CommandExecutionResults.SyntaxErrorNoDisplay;
            }

            Send("%m%You gossip '%M%{0}%m%'%x%", rawParameters);

            string other = $"%m%{DisplayName} gossips '%M%{rawParameters}%m%'%x%";
            foreach (IPlayer player in PlayerManager.Players.Where(x => x != this))
                player.Send(other);

            return CommandExecutionResults.Ok;
        }

        [Command("question", "Communication")]
        [Syntax("[cmd] <message>")]
        protected virtual CommandExecutionResults DoQuestion(string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length == 0)
            {
                Send("Ask what ?");
                return CommandExecutionResults.SyntaxErrorNoDisplay;
            }

            string what = parameters[0].Value;
            Send("%y%You question '{0}'%x%", what);

            string phrase = $"%y%{DisplayName} questions '{what}'%x%";
            foreach (IPlayer player in PlayerManager.Players.Where(x => x != this))
                player.Send(phrase);

            return CommandExecutionResults.Ok;
        }

        [Command("answer", "Communication")]
        [Syntax("[cmd] <message>")]
        protected virtual CommandExecutionResults DoAnswer(string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length == 0)
            {
                Send("Answer what ?");
                return CommandExecutionResults.SyntaxErrorNoDisplay;
            }

            string what = parameters[0].Value;
            Send("%y%You answer '{0}'%x%", what);

            string phrase = $"%y%{DisplayName} answers '{what}'%x%";
            foreach (IPlayer player in PlayerManager.Players.Where(x => x != this))
                player.Send(phrase);

            return CommandExecutionResults.Ok;
        }

        [Command("afk", "Communication")]
        protected virtual CommandExecutionResults DoAfk(string rawParameters, params CommandParameter[] parameters)
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
            return CommandExecutionResults.Ok;
        }

        [Command("replay", "Communication")]
        protected virtual CommandExecutionResults DoReplay(string rawParameters, params CommandParameter[] parameters)
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
            return CommandExecutionResults.Ok;
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
