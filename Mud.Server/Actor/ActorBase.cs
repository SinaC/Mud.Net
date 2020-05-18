using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Mud.Container;
using Mud.DataStructures.Trie;
using Mud.Domain;
using Mud.Logger;
using Mud.Server.Common;
using Mud.Server.Input;
using Mud.Settings;
// ReSharper disable UnusedMember.Global

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
                            StringBuilder syntax = BuildCommandSyntax(entry.Key, entry.Value.Syntax.Syntax, false);
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

        protected virtual bool ExecuteBeforeCommand(CommandMethodInfo methodInfo, string rawParameters, params CommandParameter[] parameters)
        {
            return true;
        }

        protected virtual bool ExecuteAfterCommand(CommandMethodInfo methodInfo, string rawParameters, params CommandParameter[] parameters)
        {
            return true;
        }

        [Command("cmd", Priority = 0)]
        [Command("commands", Priority = 0)]
        [Syntax(
            "[cmd]",
            "[cmd] all",
            "[cmd] <category>")]
        protected virtual CommandExecutionResults DoCommands(string rawParameters, params CommandParameter[] parameters)
        {
            const int columnCount = 6;

            IEnumerable<KeyValuePair<string, CommandMethodInfo>> filteredCommands = Commands.Where(x => !x.Value.Attribute.Hidden && IsCommandAvailable(x.Value.Attribute));

            // Display categories
            if (parameters.Length == 0)
            {
                StringBuilder categoriesSb = new StringBuilder();
                categoriesSb.AppendLine("Available categories:%W%");
                int index = 0;
                foreach (var category in filteredCommands
                    .SelectMany(x => x.Value.Attribute.Categories.Where(c => !string.IsNullOrWhiteSpace(c)))
                    .Distinct()
                    .OrderBy(x => x))
                {
                    if ((++index % columnCount) == 0)
                        categoriesSb.AppendFormatLine("{0,-14}", category);
                    else
                        categoriesSb.AppendFormat("{0,-14}", category);
                }
                if (index > 0 && index % columnCount != 0)
                    categoriesSb.AppendLine();
                categoriesSb.Append("%x%");
                Send(categoriesSb);
                return CommandExecutionResults.Ok;
            }

            // If a parameter is specified, filter on category unless parameter is 'all'
            Func<string, bool> filterOnCategoryFunc = _ => true;
            if (!parameters[0].IsAll)
                filterOnCategoryFunc = category => StringCompareHelpers.StringStartsWith(category, parameters[0].Value);

            // Grouped by category
            // if a command has multiple categories, it will appear in each category
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Available commands:");
            foreach(var cmdByCategory in filteredCommands
                .SelectMany(x => x.Value.Attribute.Categories.Where(filterOnCategoryFunc), (kv, category) => new {category, cmi = kv.Value})
                .GroupBy(x => x.category, (category, group) => new {category, commands = group.Select(x => x.cmi)})
                .OrderBy(g => g.category))
            {
                if (!string.IsNullOrEmpty(cmdByCategory.category))
                    sb.AppendLine("%W%" + cmdByCategory.category + ":%x%");
                int index = 0;
                foreach(CommandMethodInfo cmi in cmdByCategory.commands
                    .OrderBy(x => x.Attribute.Priority)
                    .ThenBy(x => x.Attribute.Name))
                {
                    if ((++index % columnCount) == 0)
                        sb.AppendFormatLine("{0,-14}", cmi.Attribute.Name);
                    else
                        sb.AppendFormat("{0,-14}", cmi.Attribute.Name);
                }
                if (index > 0 && index % columnCount != 0)
                    sb.AppendLine();
            }
            Page(sb);
            return CommandExecutionResults.Ok;
        }

        [Command("syntax", Priority = 999)]
        [Syntax(
            "[cmd] all",
            "[cmd] <command>")]
        protected virtual CommandExecutionResults DoSyntax(string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length == 0)
                return CommandExecutionResults.SyntaxError;

            string commandNameToFind = parameters[0].IsAll
                ? string.Empty // Trie will return whole tree when searching with empty string
                : parameters[0].Value.ToLowerInvariant();
            var commands = Commands.GetByPrefix(commandNameToFind).Where(x => !x.Value.Attribute.Hidden && IsCommandAvailable(x.Value.Attribute));

            bool found = false;
            StringBuilder sb = new StringBuilder();
            foreach (var group in commands.GroupBy(x => x.Value.MethodInfo.Name).OrderBy(x => x.Key)) // group by command
            {
                string[] namesByPriority = group.OrderBy(x => x.Value.Attribute.Priority).Select(x => x.Value.Attribute.Name).ToArray(); // order by priority
                string title = string.Join(", ", namesByPriority.Select(x => $"%C%{x}%x%"));
                sb.AppendLine($"Command{(namesByPriority.Length > 1 ? "s" : string.Empty)} {title}:");
                string commandNames = string.Join("|", namesByPriority);
                StringBuilder sbSyntax = BuildCommandSyntax(commandNames, group.SelectMany(x => x.Value.Syntax.Syntax).Distinct(), true);
                sb.Append(sbSyntax);
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

        protected static IReadOnlyTrie<CommandMethodInfo> GetCommands<T>()
            where T : ActorBase
            => CommandHelpers.GetCommands(typeof(T));

        private StringBuilder BuildCommandSyntax(string commandNames, IEnumerable<string> syntaxes, bool addSpaces)
        {
            StringBuilder sb = new StringBuilder();
            foreach (string syntax in syntaxes)
            {
                // TODO: enrich argument such as <character>, <player name>, ...
                string enrichedSyntax = syntax.Replace("[cmd]", commandNames);
                if (addSpaces)
                    sb.Append("      ");
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
            Wiznet.Wiznet($"Command {command} return type {rawResult.GetType().Name} is not convertible to CommandExecutionResults", WiznetFlags.Bugs, AdminLevels.Implementor);
            return CommandExecutionResults.Ok;
        }
    }
}
