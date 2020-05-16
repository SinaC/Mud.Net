using System.Linq;
using Mud.Domain;
using Mud.Server.Helpers;
using Mud.Server.Input;
// ReSharper disable UnusedMember.Global

namespace Mud.Server.Character
{
    public partial class CharacterBase
    {
        [CharacterCommand("say", "Communication", MinPosition = Positions.Resting)]
        [Syntax("[cmd] <message>")]
        protected virtual CommandExecutionResults DoSay(string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length == 0)
            {
                Send("Say what?");
                return CommandExecutionResults.SyntaxErrorNoDisplay;
            }

            Act(ActOptions.ToAll, "%g%{0:N} say{0:v} '%x%{1}%g%'%x%", this, rawParameters);
            return CommandExecutionResults.Ok;
        }

        [CharacterCommand("yell", "Communication", MinPosition = Positions.Resting)]
        [Syntax("[cmd] <message>")]
        protected virtual CommandExecutionResults DoYell(string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length == 0)
            {
                Send("Yell what?");
                return CommandExecutionResults.SyntaxErrorNoDisplay;
            }

            Act(Room.Area.Players.Where(x => x.Impersonating != null).Select(x => x.Impersonating), "%G%{0:n} yell{0:v} '%x%{1}%G%'%x%", this, rawParameters);
            return CommandExecutionResults.Ok;
        }

        [CharacterCommand("emote", "Communication", MinPosition = Positions.Resting)]
        [Syntax("[cmd] <message>")]
        protected virtual CommandExecutionResults DoEmote(string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length == 0)
            {
                Send("Emote what?");
                return CommandExecutionResults.SyntaxErrorNoDisplay;
            }

            Act(ActOptions.ToAll, "{0:n} {1}", this, rawParameters);
            return CommandExecutionResults.Ok;
        }

        [CharacterCommand("whisper", "Communication", MinPosition = Positions.Standing)]
        [Syntax("[cmd] <character> <message>")]
        protected virtual CommandExecutionResults DoWhisper(string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length <= 1)
            {
                Send("Whisper whom what?");
                return CommandExecutionResults.SyntaxErrorNoDisplay;
            }

            ICharacter whom = FindHelpers.FindByName(Room.People, parameters[0]);
            if (whom == null)
            {
                Send(StringHelpers.CharacterNotFound);
                return CommandExecutionResults.TargetNotFound;
            }

            string what = CommandHelpers.JoinParameters(parameters.Skip(1));

            Act(ActOptions.ToCharacter, "You whisper '{0}' to {1:n}.", what, whom);
            if (this != whom)
                whom.Act(ActOptions.ToCharacter, "{0:n} whispers you '{1}'.", this, what); // TODO: when used on ourself (player is pouet), pouet whispers you 'blabla'
            ActToNotVictim(whom, "{0:n} whispers something to {1:n}.", this, whom);
            // ActOptions.ToAll cannot be used because 'something' is sent except for 'this' and 'whom'

            return CommandExecutionResults.Ok;
        }

        [CharacterCommand("shout", "Communication", MinPosition = Positions.Resting)]
        [Syntax("[cmd] <message>")]
        protected virtual CommandExecutionResults DoShout(string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length == 0)
            {
                Send("Shout what?");
                return CommandExecutionResults.SyntaxErrorNoDisplay;
            }

            Act(PlayerManager.Players.Where(x => x.Impersonating != null).Select(x => x.Impersonating), "{0:N} shout{0:v} '{1}'", this, rawParameters);
            return CommandExecutionResults.Ok;
        }
    }
}
