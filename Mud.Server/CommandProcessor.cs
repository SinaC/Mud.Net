using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Mud.Logger;

namespace Mud.Server
{
    public class CommandProcessor : ICommandProcessor
    {
        private static readonly Dictionary<string, ICommand> Commands;

        static CommandProcessor()
        {
            Type iCommandType = typeof (ICommand);
            Commands = Assembly.GetExecutingAssembly().GetTypes()
                .Where(x => iCommandType.IsAssignableFrom(x) && x.IsClass && !x.IsAbstract)
                .Select(x => (ICommand) Activator.CreateInstance(x))
                .ToDictionary(x => x.Name);
        }

        public bool ProcessCommand(IClient client, string commandLine)
        {
            Log.Default.WriteLine(LogLevels.Debug, "Processing [" + commandLine + "] from client " + client.Id);

            int spaceIndex = commandLine.IndexOf(' ');
            string commandName = spaceIndex == -1 ? commandLine : commandLine.Substring(0, spaceIndex);

            bool oog = !client.Impersonating;
            if (commandName.StartsWith("/"))
            {
                oog = true;
                commandName = commandName.Substring(1);
            }

            // Execute command if exists
            // TODO: partial match
            //  l should match look
            ICommand command;
            if (Commands.TryGetValue(commandName, out command))
            {
                // Check IG/OOG
                if (!oog && (command.Flags & CommandFlags.InGame) != CommandFlags.InGame)
                {
                    Log.Default.WriteLine(LogLevels.Warning, "Cannot execute OutOfGame while impersonating (or add / before command)");
                    return false;
                }
                else if (oog && (command.Flags & CommandFlags.OutOfGame) != CommandFlags.OutOfGame)
                {
                    Log.Default.WriteLine(LogLevels.Warning, "Cannot execute InGame command while not impersonating");
                    return false;
                }

                // Extract parameters
                string parameters = spaceIndex == -1 ? String.Empty : commandLine.Substring(spaceIndex + 1);
                // Split parameters
                string[] splitted = SplitParameters(parameters).ToArray();
                // Parse parameter
                CommandParameter[] commandParameters = splitted.Select(ParseParameter).ToArray();

                if (commandParameters.Any(x => x == CommandParameter.Invalid))
                {
                    Log.Default.WriteLine(LogLevels.Warning, "Invalid command parameters");
                    return false;
                }

                Log.Default.WriteLine(LogLevels.Debug, "Command [" + command.Name + "] found with parameters [" + parameters + "] or [" + String.Join(" ", splitted.Select(x => "[" + x + "]").ToArray()) + "]. Executing ...");

                bool executed = command.Execute(client, parameters, commandParameters);

                if (!executed)
                {
                    Log.Default.WriteLine(LogLevels.Warning, "Error while executing command");
                    return false;
                }
            }
            else
            {
                Log.Default.WriteLine(LogLevels.Warning, "Command not found");
                return false;
            }
            return true;
        }

        public List<string> CommandList(CommandFlags flags)
        {
            return Commands
                .Where(x => (x.Value.Flags & flags) == flags)
                .Select(x => x.Key)
                .ToList();
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
    }
}
