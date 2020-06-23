using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mud.Common;
using Mud.Logger;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.GameAction
{
    public static class CommandHelpers
    {
        public static readonly ICommandParameter[] NoParameters = Enumerable.Empty<ICommandParameter>().ToArray();

        private static readonly CommandParameter EmptyCommandParameter = new CommandParameter(string.Empty, false);
        private static readonly CommandParameter IsAllCommandParameter = new CommandParameter(string.Empty, true);

        public static bool ExtractCommandAndParameters(string commandLine, out string command, out string rawParameters, out ICommandParameter[] parameters)
        {
            return ExtractCommandAndParameters(null, commandLine, out command, out rawParameters, out parameters, out _);
        }

        public static bool ExtractCommandAndParameters(Func<bool, IReadOnlyDictionary<string,string>> aliasesFunc, string commandLine, out string command, out string rawParameters, out ICommandParameter[] parameters, out bool forceOutOfGame)
        {
            Log.Default.WriteLine(LogLevels.Trace, "Extracting command and parameters [{0}]", commandLine);

            // No command ?
            if (string.IsNullOrWhiteSpace(commandLine))
            {
                Log.Default.WriteLine(LogLevels.Warning, "Empty command");
                command = null;
                rawParameters = null;
                parameters = null;
                forceOutOfGame = false;
                return false;
            }

            // Split into command and remaining tokens
            var extractedCommandInfo = ExtractCommand(commandLine);

            command = extractedCommandInfo.command;
            rawParameters = extractedCommandInfo.rawParameters;
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
                    var aliasExtractedCommandInfo = ExtractCommand(alias);
                    rawParameters = aliasExtractedCommandInfo.rawParameters;
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

        private static (string command, string rawParameters, IEnumerable<string> tokens) ExtractCommand(string commandLine)
        {
            Log.Default.WriteLine(LogLevels.Trace, "Extracting command [{0}]", commandLine);

            // Split
            var tokens = SplitParameters(commandLine).ToArray();

            // First token is the command
            string command = tokens[0];

            // Group remaining tokens
            string rawParameters = string.Join(" ", tokens.Skip(1).Select(x => x.Quoted()));

            return (command, rawParameters, tokens.Skip(1));
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
                        ? IsAllCommandParameter
                        : new CommandParameter(parameter, 1);
            }
            if (dotIndex == 0)
                return CommandParameter.InvalidCommandParameter; // only . is invalid
            string countAsString = parameter.Substring(0, dotIndex);
            string value = parameter.Substring(dotIndex + 1);
            bool isCountAll = string.Equals(countAsString, "all", StringComparison.InvariantCultureIgnoreCase);
            if (isCountAll)
                return new CommandParameter(value, true);
            int count;
            if (!int.TryParse(countAsString, out count)) // string.string is not splitted
                return new CommandParameter(value, 1);
            if (count <= 0 || string.IsNullOrWhiteSpace(value)) // negative count or empty value is invalid
                return CommandParameter.InvalidCommandParameter;
            return new CommandParameter(value, count);
        }

        public static string JoinParameters(IEnumerable<ICommandParameter> parameters)
        {
            ICommandParameter[] commandParameters = parameters as ICommandParameter[] ?? parameters.ToArray();
            if (!commandParameters.Any())
                return string.Empty;

            string joined = string.Join(" ", commandParameters.Select(x => ToString(x)));
            return joined;
        }

        public static (string rawParameters, ICommandParameter[] parameters) SkipParameters(IEnumerable<ICommandParameter> inputParameters, int count)
        {
            ICommandParameter[] parameters = inputParameters.Skip(count).ToArray();
            string rawParameter = JoinParameters(parameters);
            return (rawParameter, parameters);
        }

        private static string ToString(ICommandParameter commandParameter)
        {
            if (commandParameter.IsAll)
            {
                if (string.IsNullOrWhiteSpace(commandParameter.Value))
                    return "all";
                else
                    return "all." + commandParameter.Value;
            }
            return commandParameter.Count == 1
                ? commandParameter.Value.Quoted()
                : $"{commandParameter.Count}.{commandParameter.Value.Quoted()}";
        }
    }
}
