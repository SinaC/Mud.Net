using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Mud.DataStructures.Trie;
using Mud.Logger;

namespace Mud.Server.Input
{
    public static class CommandHelpers
    {
        public static bool ExtractCommandAndParameters(string commandLine, out string command, out string rawParameters, out CommandParameter[] parameters, out bool forceOutOfGame)
        {
            return ExtractCommandAndParameters(null, commandLine, out command, out rawParameters, out parameters, out forceOutOfGame);
        }

        public static bool ExtractCommandAndParameters(IReadOnlyDictionary<string, string> aliases, string commandLine, out string command, out string rawParameters, out CommandParameter[] parameters, out bool forceOutOfGame)
        {
            Log.Default.WriteLine(LogLevels.Debug, "Extracting command and parameters [{0}]", commandLine);

            //// Extract command
            //int spaceIndex = commandLine.IndexOf(' ');
            //command = spaceIndex == -1 ? commandLine : commandLine.Substring(0, spaceIndex);
            //// Extract raw parameters
            //rawParameters = spaceIndex == -1 ? String.Empty : commandLine.Substring(spaceIndex + 1);
            // Extract command and raw parameters
            ExtractCommand(commandLine, out command, out rawParameters);

            // Substitute by alias if found
            if (aliases != null)
            {
                string alias;
                if (aliases.TryGetValue(command, out alias))
                {
                    Log.Default.WriteLine(LogLevels.Info, "Alias found : {0} -> {1}", command, alias);
                    commandLine = alias;
                    // Extract command and raw parameters
                    ExtractCommand(commandLine, out command, out rawParameters);
                }
            }

            if (String.IsNullOrWhiteSpace(commandLine))
            {
                Log.Default.WriteLine(LogLevels.Warning, "Empty command");
                forceOutOfGame = false;
                parameters = null;
                return false;
            }

            // Check if forcing OutOfGame
            if (command.StartsWith("/"))
            {
                forceOutOfGame = true;
                command = command.Substring(1); // remove '/'
            }
            else
                forceOutOfGame = false;

            // Split parameters
            string[] splitted = SplitParameters(rawParameters).ToArray();
            // Parse parameter
            parameters = splitted.Select(ParseParameter).ToArray();

            if (parameters.Any(x => x == CommandParameter.InvalidCommand))
            {
                Log.Default.WriteLine(LogLevels.Warning, "Invalid command parameters");
                return false;
            }

            return true;
        }

        public static bool ExtractCommand(string commandLine, out string command, out string rawParameters)
        {
            Log.Default.WriteLine(LogLevels.Debug, "Extracting command [{0}]", commandLine);

            // Extract command
            int spaceIndex = commandLine.IndexOf(' ');
            command = spaceIndex == -1 ? commandLine : commandLine.Substring(0, spaceIndex);
            // Extract raw parameters
            rawParameters = spaceIndex == -1 ? String.Empty : commandLine.Substring(spaceIndex + 1);

            return true;
        }

        public static IEnumerable<string> SplitParameters(string parameters)
        {
            if (String.IsNullOrWhiteSpace(parameters))
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
                if (c != '"' && c != '\'' && !(Char.IsWhiteSpace(c) && !inQuote))
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

        public static CommandParameter ParseParameter(string parameter)
        {
            if (String.IsNullOrWhiteSpace(parameter))
                return CommandParameter.EmptyCommand;
            int dotIndex = parameter.IndexOf('.');
            if (dotIndex < 0)
            {
                bool isAll = String.Equals(parameter, "all", StringComparison.InvariantCultureIgnoreCase);
                return
                    isAll
                        ? CommandParameter.IsAllCommand
                        : new CommandParameter(parameter, 1);
            }
            if (dotIndex == 0)
                return CommandParameter.InvalidCommand; // only . is invalid
            string countAsString = parameter.Substring(0, dotIndex);
            string value = parameter.Substring(dotIndex + 1);
            bool isCountAll = String.Equals(countAsString, "all", StringComparison.InvariantCultureIgnoreCase);
            if (isCountAll)
                return new CommandParameter(value, true);
            int count;
            if (!int.TryParse(countAsString, out count)) // string.string is not splitted
                return new CommandParameter(parameter, 1);
            if (count <= 0 || String.IsNullOrWhiteSpace(value)) // negative count or empty value is invalid
                return CommandParameter.InvalidCommand;
            return new CommandParameter(parameter, count);
        }

        public static string JoinParameters(IEnumerable<CommandParameter> parameters)
        {
            CommandParameter[] commandParameters = parameters as CommandParameter[] ?? parameters.ToArray();
            if (!commandParameters.Any())
                return String.Empty;

            string joined = String.Join(" ", commandParameters.Select(x => x.Count == 1 ? x.Value : $"{x.Count}.{x.Value}"));
            return joined;
        }

        public static IReadOnlyTrie<MethodInfo> GetCommandsOld(Type type)
        {
            IEnumerable<TrieEntry<MethodInfo>> commands = type.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
                .Where(x => x.GetCustomAttributes(typeof(CommandAttribute), false).Any())
                .SelectMany(x => x.GetCustomAttributes(typeof(CommandAttribute)).OfType<CommandAttribute>().Select(attr => attr.Name).Distinct(), // when overriding a command (attribute is declared twice) -> cause 2 entries with the same method and same name
                    (methodInfo, commandName) => new TrieEntry<MethodInfo>(commandName, methodInfo));
            Trie<MethodInfo> trie = new Trie<MethodInfo>(commands);
            return trie;
        }

        public static IReadOnlyTrie<CommandMethodInfo> GetCommands(Type type)
        {
            var commands = type.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
                .Where(x => x.GetCustomAttributes(typeof(CommandAttribute), false).Any())
                .SelectMany(x => x.GetCustomAttributes(typeof(CommandAttribute)).OfType<CommandAttribute>().Distinct(new CommandAttributeEqualityComparer()),
                    (methodInfo, attribute) => new TrieEntry<CommandMethodInfo>(attribute.Name, new CommandMethodInfo(attribute, methodInfo)));
            Trie<CommandMethodInfo> trie = new Trie<CommandMethodInfo>(commands);
            return trie;
        }
    }
}
