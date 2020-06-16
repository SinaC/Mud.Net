using Mud.Server.GameAction;
using Mud.Server.Interfaces.GameAction;
using System.Collections.Generic;
using System.Linq;

namespace Mud.Server.Character.PlayableCharacter
{
    public partial class PlayableCharacter
    {
        [Command("macro", "Misc")]
        [Command("alias", "Misc")]
        [Syntax(
            "[cmd]",
            "[cmd] <word>",
            "[cmd] <word> <substitution>")]
        protected virtual CommandExecutionResults DoAlias(string rawParameters, params ICommandParameter[] parameters)
        {
            if (parameters.Length == 0)
            {
                if (Aliases.Count == 0)
                {
                    Send("You have no aliases defined.");
                    return CommandExecutionResults.Ok;
                }
                Send("Your current aliases are:");
                foreach (KeyValuePair<string, string> aliasToDisplay in Aliases.OrderBy(x => x.Key))
                    Send("     {0}: {1}", aliasToDisplay.Key, aliasToDisplay.Value);
                return CommandExecutionResults.Ok;
            }
            if (parameters.Length == 1)
            {
                string aliasToRemove = parameters[0].Value.ToLowerInvariant();
                string cmd;
                if (Aliases.TryGetValue(aliasToRemove, out cmd))
                {
                    Send($"{aliasToRemove} is aliases to {cmd}.");
                    return CommandExecutionResults.Ok;
                }
                else
                {
                    Send("That alias is not defined.");
                    return CommandExecutionResults.InvalidParameter;
                }
            }

            string alias = parameters[0].Value.ToLowerInvariant().Trim();
            if (alias.StartsWith("alias") || alias.StartsWith("unalias"))
            {
                Send("Sorry, that word is reserved.");
                return CommandExecutionResults.InvalidParameter;
            }
            if (alias.Any(c => c == '\'' || c == '"' || c == ' '))
            {
                Send("Aliases with that kind of characters are not allowed!");
                return CommandExecutionResults.InvalidParameter;
            }
            if (alias.StartsWith("delete"))
            {
                Send("That shall not be done.");
                return CommandExecutionResults.InvalidParameter;
            }
            string newCmd = CommandHelpers.JoinParameters(parameters.Skip(1)); // merge parameters except first one
            if (_aliases.TryGetValue(alias, out _))
            {
                _aliases[alias] = newCmd;
                Send($"{alias} is now realiased to '{newCmd}'.");
            }
            else
            {
                _aliases.Add(alias, newCmd);
                Send($"{alias} is now aliased to '{newCmd}'.");
            }
            return CommandExecutionResults.Ok;

        }

        [Command("unmacro", "Misc")]
        [Command("unalias", "Misc")]
        [Syntax("[cmd] <word>")]
        protected virtual CommandExecutionResults DoUnAlias(string rawParameters, params ICommandParameter[] parameters)
        {
            if (parameters.Length == 0)
            {
                Send("Unalias what?");
                return CommandExecutionResults.SyntaxErrorNoDisplay;
            }
            string alias = parameters[0].Value.ToLowerInvariant().Trim();
            if (Aliases.TryGetValue(alias, out _))
            {
                _aliases.Remove(alias);
                Send("Alias removed.");
                return CommandExecutionResults.Ok;
            }
            Send("No alias of that name to remove.");
            return CommandExecutionResults.InvalidParameter;
        }
    }
}
