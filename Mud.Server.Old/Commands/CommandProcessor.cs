using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Mud.Logger;
using Mud.Server.Commands;

namespace Mud.Server.Old.Commands
{
    public class CommandProcessor : ICommandProcessor
    {
        private static readonly Dictionary<string, IInGameCommand> InGameCommands;
        private static readonly Dictionary<string, IOutOfGameCommand> OutOfGameCommands;

        static CommandProcessor()
        {
            // IG
            Type iInGameCommandType = typeof(IInGameCommand);
            InGameCommands = Assembly.GetExecutingAssembly().GetTypes()
                .Where(x => iInGameCommandType.IsAssignableFrom(x) && x.IsClass && !x.IsAbstract)
                .Select(x => (IInGameCommand)Activator.CreateInstance(x))
                .ToDictionary(x => x.Name);
            // OOG
            Type iOutOfGameCommandType = typeof(IOutOfGameCommand);
            OutOfGameCommands = Assembly.GetExecutingAssembly().GetTypes()
                .Where(x => iOutOfGameCommandType.IsAssignableFrom(x) && x.IsClass && !x.IsAbstract)
                .Select(x => (IOutOfGameCommand)Activator.CreateInstance(x))
                .ToDictionary(x => x.Name);
        }

        public bool ProcessCommand(IEntity entity, string commandLine) // IG
        {
            Log.Default.WriteLine(LogLevels.Debug, "Processing [" + commandLine + "]");

            int spaceIndex = commandLine.IndexOf(' ');
            string commandName = spaceIndex == -1 ? commandLine : commandLine.Substring(0, spaceIndex);
            string commandParameters = spaceIndex == -1 ? String.Empty : commandLine.Substring(spaceIndex + 1);

            return ExecuteInGameCommand(entity, commandName, commandParameters);
        }

        public bool ProcessCommand(IPlayer player, string commandLine) // IG/OOG
        {
            Log.Default.WriteLine(LogLevels.Debug, "Processing [" + commandLine + "]");

            int spaceIndex = commandLine.IndexOf(' ');
            string commandName = spaceIndex == -1 ? commandLine : commandLine.Substring(0, spaceIndex);
            string commandParameters = spaceIndex == -1 ? String.Empty : commandLine.Substring(spaceIndex + 1);


            // If not impersonating -> OOG
            // If impersonating
            //      if command starts with a / -> OOG
            //      else, IG

            bool oog = player.Impersonating == null;
            if (commandName.StartsWith("/"))
            {
                oog = true;
                commandName = commandName.Substring(1);
            }

            if (oog)
            {
                return ExecuteOutOfGameCommand(player, commandName, commandParameters);
            }
            else
            {
                return ExecuteInGameCommand(player.Impersonating, commandName, commandParameters);
            }
        }

        private bool ExecuteOutOfGameCommand(IPlayer player, string commandName, string commandParameters)
        {
            IOutOfGameCommand command;
            if (OutOfGameCommands.TryGetValue(commandName, out command))
            {
                // Split parameters
                string[] splitted = SplitParameters(commandParameters).ToArray();
                // Parse parameter
                CommandParameter[] parameters = splitted.Select(ParseParameter).ToArray();

                if (parameters.Any(x => x == CommandParameter.Invalid))
                {
                    player.Send("Invalid command parameters");
                    return false;
                }

                Log.Default.WriteLine(LogLevels.Debug, "Command [" + command.Name + "] found with parameters [" + commandParameters + "] or [" + String.Join(" ", splitted.Select(x => "[" + x + "]").ToArray()) + "]. Executing ...");

                bool executed = command.Execute(player, commandParameters, parameters);

                if (!executed)
                {
                    Log.Default.WriteLine(LogLevels.Error, "Error while executing command");
                    return false;
                }
            }
            else
            {
                player.Send("Command not found");
                return false;
            }
            return true;
        }

        private bool ExecuteInGameCommand(IEntity entity, string commandName, string commandParameters)
        {
            IInGameCommand command;
            if (InGameCommands.TryGetValue(commandName, out command))
            {
                // Split parameters
                string[] splitted = SplitParameters(commandParameters).ToArray();
                // Parse parameter
                CommandParameter[] parameters = splitted.Select(ParseParameter).ToArray();

                if (parameters.Any(x => x == CommandParameter.Invalid))
                {
                    entity.Send("Invalid command parameters");
                    return false;
                }

                Log.Default.WriteLine(LogLevels.Debug, "Command [" + command.Name + "] found with parameters [" + commandParameters + "] or [" + String.Join(" ", splitted.Select(x => "[" + x + "]").ToArray()) + "]. Executing ...");

                bool executed = command.Execute(entity, commandParameters, parameters);

                if (!executed)
                {
                    Log.Default.WriteLine(LogLevels.Error, "Error while executing command");
                    return false;
                }
            }
            else
            {
                entity.Send("Command not found");
                return false;
            }
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
    }
}
