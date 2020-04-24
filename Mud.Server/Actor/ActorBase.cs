using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Mud.Container;
using Mud.DataStructures.Trie;
using Mud.Logger;
using Mud.Server.Common;
using Mud.Server.Helpers;
using Mud.Server.Input;
using Mud.Settings;

namespace Mud.Server.Actor
{
    public abstract class ActorBase : IActor
    {
        protected ISettings Settings => DependencyContainer.Current.GetInstance<ISettings>();
        protected IWorld World => DependencyContainer.Current.GetInstance<IWorld>();
        protected IWiznet Wiznet => DependencyContainer.Current.GetInstance<IWiznet>();
        protected IPlayerManager PlayerManager => DependencyContainer.Current.GetInstance<IPlayerManager>();
        protected IAbilityManager AbilityManager => DependencyContainer.Current.GetInstance<IAbilityManager>();
        protected IClassManager ClassManager => DependencyContainer.Current.GetInstance<IClassManager>();
        protected IRaceManager RaceManager => DependencyContainer.Current.GetInstance<IRaceManager>();

        #region IActor

        public abstract IReadOnlyTrie<CommandMethodInfo> Commands { get; }

        public abstract bool ProcessCommand(string commandLine);
        public abstract void Send(string message, bool addTrailingNewLine);
        public abstract void Page(StringBuilder text);

        public bool ExecuteCommand(string command, string rawParameters, CommandParameter[] parameters)
        {
            // Search for command and invoke it
            if (Commands != null)
            {
                command = command.ToLowerInvariant(); // lower command
                List<TrieEntry<CommandMethodInfo>> methodInfos = Commands.GetByPrefix(command).ToList();
                TrieEntry<CommandMethodInfo> entry = methodInfos.OrderBy(x => x.Value.Attribute.Priority).FirstOrDefault(); // use priority to choose between conflicting commands
                if (entry.Value?.Attribute?.NoShortcut == true && command != entry.Key) // if command doesn't accept shortcut, inform player
                {
                    Send("If you want to {0}, spell it out.", entry.Key.ToUpper());
                    return true;
                }
                else if (entry.Value?.MethodInfo != null)
                {
                    if (IsCommandAvailable(entry.Value?.Attribute))
                    {
                        bool beforeExecute = ExecuteBeforeCommand(entry.Value, rawParameters, parameters);
                        if (!beforeExecute)
                        {
                            Log.Default.WriteLine(LogLevels.Info, $"ExecuteBeforeCommand returned false for command {entry.Value.MethodInfo.Name} and parameters {rawParameters}");
                            return false;
                        }
                        MethodInfo methodInfo = entry.Value.MethodInfo;
                        object rawExecutionResult;
                        if (entry.Value.Attribute?.AddCommandInParameters == true)
                        {
                            // Insert command as first parameter
                            CommandParameter[] enhancedParameters = new CommandParameter[(parameters?.Length ?? 0) + 1];
                            if (parameters != null)
                                Array.ConstrainedCopy(parameters, 0, enhancedParameters, 1, parameters.Length);
                            enhancedParameters[0] = new CommandParameter(command, 1);
                            string enhancedRawParameters = command + " " + rawParameters;
                            //
                            rawExecutionResult = methodInfo.Invoke(this, new object[] {enhancedRawParameters, enhancedParameters});
                        }
                        else
                            rawExecutionResult = methodInfo.Invoke(this, new object[] {rawParameters, parameters});
                        CommandExecutionResults executionResult = ConvertToCommandExecutionResults(entry.Key, rawExecutionResult);
                        // !!no AfterCommand executed if Error has been returned by Command
                        if (executionResult == CommandExecutionResults.Error)
                        {
                            Log.Default.WriteLine(LogLevels.Warning, "Error while executing command");
                            return false;
                        }
                        else if (executionResult == CommandExecutionResults.SyntaxError)
                        {
                            StringBuilder syntax = BuildCommandSyntax(entry.Value);
                            Send(syntax);
                        }
                        bool afterExecute = ExecuteAfterCommand(entry.Value, rawParameters, parameters);
                        if (!afterExecute)
                        {
                            Log.Default.WriteLine(LogLevels.Info, $"ExecuteAfterCommand returned false for command {entry.Value.MethodInfo.Name} and parameters {rawParameters}");
                            return false;
                        }
                        return true;
                    }
                    else
                    {
                        Log.Default.WriteLine(LogLevels.Warning, $"Command {command} not found");
                        Send("Command not found.");
                        return false;
                    }
                }
                else
                {
                    Log.Default.WriteLine(LogLevels.Warning, $"Command {command} not found");
                    Send("Command not found.");
                    return false;
                }
            }
            else
            {
                Log.Default.WriteLine(LogLevels.Warning, $"No command found for {GetType().FullName}");
                Send("Command not found.");
                return false;
            }
        }

        public virtual bool ExecuteBeforeCommand(CommandMethodInfo methodInfo, string rawParameters, params CommandParameter[] parameters)
        {
            return true;
        }

        public virtual bool ExecuteAfterCommand(CommandMethodInfo methodInfo, string rawParameters, params CommandParameter[] parameters)
        {
            return true;
        }

        public void Send(string format, params object[] parameters)
        {
            string message = parameters.Length == 0 
                ? format 
                : string.Format(format, parameters);
            Send(message, true); // add trailing newline
        }

        public void Send(StringBuilder text)
        {
            Send(text.ToString(), false); // don't add trailing newline
        }

        #endregion

        [Command("cmd", Priority = 0)]
        [Command("commands", Priority = 0)]
        [Syntax(
            "[cmd]",
            "[cmd] <category>")]
        protected virtual CommandExecutionResults DoCommands(string rawParameters, params CommandParameter[] parameters)
        {
            const int columnCount = 5;
            // TODO: group trie by value (group by DoXXX) and display set of key linked to this value

            IEnumerable<KeyValuePair<string, CommandMethodInfo>> filteredCommands = Commands.Where(x => !x.Value.Attribute.Hidden && IsCommandAvailable(x.Value.Attribute));

            // If a parameter is specified, filter on category
            if (parameters.Length > 0)
                filteredCommands = filteredCommands.Where(x => x.Value.Attribute.Categories.Any(category => FindHelpers.StringStartsWith(category, parameters[0].Value)));

            StringBuilder sb = new StringBuilder("Available commands:" + Environment.NewLine);
            foreach (IGrouping<string, KeyValuePair<string, CommandMethodInfo>> group in filteredCommands
                .GroupBy(x => string.Join(",", x.Value.Attribute.Categories)) // TODO: group by any category
                .OrderBy(g => g.Key))
            {
                if (!string.IsNullOrEmpty(group.Key))
                    sb.AppendLine("%W%" + group.Key + ":%x%");
                int index = 0;
                foreach (KeyValuePair<string, CommandMethodInfo> kv in group
                    .OrderBy(x => x.Value.Attribute.Priority)
                    .ThenBy(x => x.Key))
                {
                    if ((++index % columnCount) == 0)
                        sb.AppendFormatLine("{0,-14}", kv.Key);
                    else
                        sb.AppendFormat("{0,-14}", kv.Key);
                }
                if (index > 0 && index % columnCount != 0)
                    sb.AppendLine();
            }
            Page(sb);
            return CommandExecutionResults.Ok;
        }

        [Command("syntax", Priority = 999)]
        [Syntax("[cmd] <command>")]
        protected virtual CommandExecutionResults DoSyntax(string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length == 0)
                return CommandExecutionResults.SyntaxError;

            string commandNameToFind = parameters[0].Value.ToLowerInvariant();
            var commands = Commands.GetByPrefix(commandNameToFind).Where(x => !x.Value.Attribute.Hidden && IsCommandAvailable(x.Value.Attribute));

            bool found = false;
            StringBuilder sb = new StringBuilder();
            foreach (var group in commands.GroupBy(x => x.Value.MethodInfo.Name)) // group by command
            {
                string[] namesByPriority = group.OrderBy(x => x.Value.Attribute.Priority).Select(x => x.Value.Attribute.Name).ToArray(); // order by priority
                string title = string.Join(", ", namesByPriority.Select(x => $"%C%{x}%x%"));
                sb.AppendLine($"Command{(namesByPriority.Length > 1 ? "s" : string.Empty)} {title}:");
                string commandNames = string.Join("|", namesByPriority);
                foreach (string syntax in group.SelectMany(x => x.Value.Syntax.Syntax))
                {
                    // TODO: enrich argument such as <character>, <player name>, ...
                    string enrichedSyntax = syntax.Replace("[cmd]", commandNames);
                    sb.AppendLine("     Syntax: " + enrichedSyntax);
                }
                found = true;
            }
            if (found)
            {
                Page(sb);
                return CommandExecutionResults.Ok;
            }
            Send("No command found.");
            return CommandExecutionResults.TargetNotFound;
        }

        protected virtual bool IsCommandAvailable(CommandAttribute attribute)
        {
            return true;
        }

        protected static IReadOnlyTrie<CommandMethodInfo> GetCommands<T>() => CommandHelpers.GetCommands(typeof(T));

        private StringBuilder BuildCommandSyntax(CommandMethodInfo commandMethodInfo)
        {
            StringBuilder sb = new StringBuilder();
            foreach (string syntax in commandMethodInfo.Syntax.Syntax)
            {
                // TODO: enrich argument such as <character>, <player name>, ...
                string enrichedSyntax = syntax.Replace("[cmd]", commandMethodInfo.Attribute.Name);
                sb.AppendLine("Syntax: " + enrichedSyntax);
            }
            return sb;
        }

        private CommandExecutionResults ConvertToCommandExecutionResults(string command, object rawResult)
        {
            if (rawResult == null)
                return CommandExecutionResults.Ok;
            if (rawResult is bool boolResult)
                return boolResult
                    ? CommandExecutionResults.Ok
                    : CommandExecutionResults.Error;
            if (rawResult is CommandExecutionResults commandExecutionResult)
                return commandExecutionResult;
            Log.Default.WriteLine(LogLevels.Error, "Command {0} return type {1} is not convertible to CommandExecutionResults", command, rawResult.GetType().Name);
            return CommandExecutionResults.Ok;
        }
    }
}
