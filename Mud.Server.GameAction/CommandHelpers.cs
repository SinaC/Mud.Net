using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mud.Logger;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.GameAction
{
    public static class CommandHelpers
    {
        public static readonly ICommandParameter[] NoParameters = Enumerable.Empty<ICommandParameter>().ToArray();

        private static readonly CommandParameter EmptyCommandParameter = new CommandParameter(string.Empty, string.Empty, false);

        public static bool ExtractCommandAndParameters(string input, out string command, out ICommandParameter[] parameters)
        {
            return ExtractCommandAndParameters(null, input, out command, out parameters, out _);
        }

        public static bool ExtractCommandAndParameters(Func<bool, IReadOnlyDictionary<string,string>> aliasesFunc, string input, out string command, out ICommandParameter[] parameters, out bool forceOutOfGame)
        {
            Log.Default.WriteLine(LogLevels.Trace, "Extracting command and parameters [{0}]", input);

            // No command ?
            if (string.IsNullOrWhiteSpace(input))
            {
                Log.Default.WriteLine(LogLevels.Warning, "Empty command");
                command = null;
                parameters = null;
                forceOutOfGame = false;
                return false;
            }

            // Split into command and remaining tokens
            var extractedCommandInfo = ExtractCommandAndTokens(input);

            command = extractedCommandInfo.command;
            IEnumerable<string> tokens = extractedCommandInfo.tokens;
            // Check if forcing OutOfGame
            if (command.StartsWith("/"))
            {
                forceOutOfGame = true;
                command = command.Substring(1); // remove '/'
            }
            else
                forceOutOfGame = false;

            // Substitute by alias if found
            IReadOnlyDictionary<string, string> aliases = aliasesFunc?.Invoke(forceOutOfGame);
            if (aliases != null)
            {
                string alias;
                if (aliases.TryGetValue(command, out alias))
                {
                    Log.Default.WriteLine(LogLevels.Debug, "Alias found : {0} -> {1}", command, alias);
                    // Extract command and raw parameters
                    var aliasExtractedCommandInfo = ExtractCommandAndTokens(alias);
                    tokens = aliasExtractedCommandInfo.tokens;
                }
            }

            // Parse parameter
            parameters = tokens.Select(ParseParameter).ToArray();

            if (parameters.Any(x => x == CommandParameter.InvalidCommandParameter))
            {
                Log.Default.WriteLine(LogLevels.Warning, "Invalid command parameters");
                return false;
            }

            return true;
        }

        private static (string command, IEnumerable<string> tokens) ExtractCommandAndTokens(string commandLine)
        {
            Log.Default.WriteLine(LogLevels.Trace, "Extracting command [{0}]", commandLine);

            // Split
            string[] tokens = SplitParameters(commandLine).ToArray();

            // First token is the command
            string command = tokens[0];

            return (command, tokens.Skip(1)); // return command and remaining tokens
        }

        public static IEnumerable<string> SplitParameters(string parameters)
        {
            if (string.IsNullOrWhiteSpace(parameters))
                yield break;
            var sb = new StringBuilder();
            bool inQuote = false;
            foreach (char c in parameters)
            {
                if ((c == '"' || c == '\'') && !inQuote)
                {
                    inQuote = true;
                    continue;
                }
                if (c != '"' && c != '\'' && !(char.IsWhiteSpace(c) && !inQuote))
                {
                    sb.Append(c);
                    continue;
                }
                if (sb.Length > 0)
                {
                    var result = sb.ToString();
                    sb.Clear();
                    inQuote = false;
                    yield return result;
                }
            }
            if (sb.Length > 0)
                yield return sb.ToString();
        }

        public static ICommandParameter ParseParameter(string parameter)
        {
            if (string.IsNullOrWhiteSpace(parameter))
                return EmptyCommandParameter;
            int dotIndex = parameter.IndexOf('.');
            if (dotIndex < 0)
            {
                bool isAll = string.Equals(parameter, "all", StringComparison.InvariantCultureIgnoreCase);
                return isAll
                        ? new CommandParameter(parameter, string.Empty, true)
                        : new CommandParameter(parameter, parameter, 1);
            }
            if (dotIndex == 0)
                return CommandParameter.InvalidCommandParameter; // only . is invalid
            string countAsString = parameter.Substring(0, dotIndex);
            string value = parameter.Substring(dotIndex + 1);
            bool isCountAll = string.Equals(countAsString, "all", StringComparison.InvariantCultureIgnoreCase);
            if (isCountAll)
                return new CommandParameter(parameter, value, true);
            int count;
            if (!int.TryParse(countAsString, out count)) // string.string is not splitted
                return new CommandParameter(parameter, value, 1);
            if (count <= 0 || string.IsNullOrWhiteSpace(value)) // negative count or empty value is invalid
                return CommandParameter.InvalidCommandParameter;
            return new CommandParameter(parameter, value, count);
        }

        public static string JoinParameters(IEnumerable<ICommandParameter> parameters)
        {
            ICommandParameter[] commandParameters = parameters as ICommandParameter[] ?? parameters.ToArray();
            if (!commandParameters.Any())
                return string.Empty;

            string joined = string.Join(" ", commandParameters.Select(x => x.RawValue));
            return joined;
        }
    }
}
