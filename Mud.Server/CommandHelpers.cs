using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Mud.DataStructures;
using Mud.Logger;

namespace Mud.Server
{
    public static class CommandHelpers
    {
        public static bool ExtractCommandAndParameters(string commandLine, out string command, out string rawParameters, out CommandParameter[] parameters, out bool forceOutOfGame)
        {
            Log.Default.WriteLine(LogLevels.Debug, "Extracting command and parameters [{0}]", commandLine);

            // Extract command
            int spaceIndex = commandLine.IndexOf(' ');
            command = spaceIndex == -1 ? commandLine : commandLine.Substring(0, spaceIndex);
            // Extract raw parameters
            rawParameters = spaceIndex == -1 ? String.Empty : commandLine.Substring(spaceIndex + 1);

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

            if (parameters.Any(x => x == CommandParameter.Invalid))
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
                return CommandParameter.Empty;
            int dotIndex = parameter.IndexOf('.');
            if (dotIndex < 0)
                return new CommandParameter
                {
                    Count = 1,
                    Value = parameter
                };
            if (dotIndex == 0)
                return CommandParameter.Invalid; // only a . is invalid
            string countAsString = parameter.Substring(0, dotIndex);
            string value = parameter.Substring(dotIndex + 1);
            int count;
            if (!int.TryParse(countAsString, out count))
                count = 1;
            if (count <= 0 || String.IsNullOrWhiteSpace(value)) // negative count or empty value is invalid
                return CommandParameter.Invalid;
            return new CommandParameter
            {
                Count = count,
                Value = value
            };
        }

        public static string JoinParameters(IEnumerable<CommandParameter> parameters)
        {
            CommandParameter[] commandParameters = parameters as CommandParameter[] ?? parameters.ToArray();
            if (!commandParameters.Any())
                return String.Empty;

            string joined = String.Join(" ", commandParameters.Select(x => x.Count == 1 ? x.Value : String.Format("{0}.{1}", x.Count, x.Value)));
            return joined;
        }

        public static Trie<MethodInfo> GetCommands(Type type)
        {
            var commands = type.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
                .Where(x => x.GetCustomAttributes(typeof (CommandAttribute), false).Any())
                //.Select(x => new TrieEntry<MethodInfo>(x.GetCustomAttributes(typeof(CommandAttribute)).OfType<CommandAttribute>().First().Name, x));
                .SelectMany(x => x.GetCustomAttributes(typeof (CommandAttribute)).OfType<CommandAttribute>(),
                    (methodInfo, attribute) => new
                    {
                        methodInfo,
                        name = attribute.Name
                    })
                .Select(x => new TrieEntry<MethodInfo>(x.name, x.methodInfo));
            Trie<MethodInfo> trie = new Trie<MethodInfo>();
            trie.AddRange(commands);
            return trie;
        }
    }
}
