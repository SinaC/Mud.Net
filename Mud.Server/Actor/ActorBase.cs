using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Mud.Common;
using Mud.Container;
using Mud.DataStructures.Trie;
using Mud.Domain;
using Mud.Logger;
using Mud.Server.Command;
using Mud.Server.Input;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Actor;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.World;
using Mud.Settings;
// ReSharper disable UnusedMember.Global

namespace Mud.Server.Actor
{
    public abstract class ActorBase : IActor
    {
        protected ISettings Settings => DependencyContainer.Current.GetInstance<ISettings>();
        protected IWorld World => DependencyContainer.Current.GetInstance<IWorld>();
        protected IWiznet Wiznet => DependencyContainer.Current.GetInstance<IWiznet>();
        protected IAbilityManager AbilityManager => DependencyContainer.Current.GetInstance<IAbilityManager>();

        #region IActor

        public abstract IReadOnlyTrie<CommandExecutionInfo> Commands { get; }

        public abstract bool ProcessCommand(string commandLine);
        public abstract void Send(string message, bool addTrailingNewLine);
        public abstract void Page(StringBuilder text);

        public bool ExecuteCommand(string command, string rawParameters, CommandParameter[] parameters)
        {
            // Search for command and invoke it
            if (Commands != null)
            {
                command = command.ToLowerInvariant(); // lower command
                List<TrieEntry<CommandExecutionInfo>> methodInfos = Commands.GetByPrefix(command).ToList();
                TrieEntry<CommandExecutionInfo> entry = methodInfos.OrderBy(x => x.Value.CommandAttribute.Priority).FirstOrDefault(); // use priority to choose between conflicting commands
                if (entry.Value?.CommandAttribute?.NoShortcut == true && command != entry.Key) // if command doesn't accept shortcut, inform player
                {
                    Send("If you want to {0}, spell it out.", entry.Key.ToUpper());
                    return true;
                }
                else if (entry.Value is CommandMethodInfo cmi && cmi.MethodInfo != null)
                {
                    if (IsCommandAvailable(entry.Value?.CommandAttribute))
                    {
                        bool beforeExecute = ExecuteBeforeCommand(cmi, rawParameters, parameters);
                        if (!beforeExecute)
                        {
                            Log.Default.WriteLine(LogLevels.Info, $"ExecuteBeforeCommand returned false for command {cmi.MethodInfo.Name} and parameters {rawParameters}");
                            return false;
                        }
                        MethodInfo methodInfo = cmi.MethodInfo;
                        object rawExecutionResult;
                        if (entry.Value.CommandAttribute?.AddCommandInParameters == true)
                        {
                            // Insert command as first parameter
                            CommandParameter[] enhancedParameters = new CommandParameter[(parameters?.Length ?? 0) + 1];
                            if (parameters != null)
                                Array.ConstrainedCopy(parameters, 0, enhancedParameters, 1, parameters.Length);
                            enhancedParameters[0] = new CommandParameter(command, 1);
                            string enhancedRawParameters = command + " " + rawParameters;
                            //
                            rawExecutionResult = methodInfo.Invoke(this, new object[] { enhancedRawParameters, enhancedParameters });
                        }
                        else
                            rawExecutionResult = methodInfo.Invoke(this, new object[] { rawParameters, parameters });
                        CommandExecutionResults executionResult = ConvertToCommandExecutionResults(entry.Key, rawExecutionResult);
                        // !!no AfterCommand executed if Error has been returned by Command
                        if (executionResult == CommandExecutionResults.Error)
                        {
                            Log.Default.WriteLine(LogLevels.Warning, "Error while executing command");
                            return false;
                        }
                        else if (executionResult == CommandExecutionResults.SyntaxError)
                        {
                            StringBuilder syntax = CommandBase<IActor>.BuildCommandSyntax(entry.Key, entry.Value.SyntaxAttribute.Syntax, false);
                            Send(syntax);
                        }
                        bool afterExecute = ExecuteAfterCommand(cmi, rawParameters, parameters);
                        if (!afterExecute)
                        {
                            Log.Default.WriteLine(LogLevels.Info, $"ExecuteAfterCommand returned false for command {cmi.MethodInfo.Name} and parameters {rawParameters}");
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
                else if (entry.Value is CommandInfo ci && ci.CommandExecutionType != null)
                {
                    if (IsCommandAvailable(entry.Value?.CommandAttribute))
                    {
                        Type executionType = ci.CommandExecutionType;
                        IGameAction gameAction;
                        // Try to find in DependencyContainer
                        if (DependencyContainer.Current.GetRegistration(executionType) != null)
                            gameAction = DependencyContainer.Current.GetInstance(executionType) as IGameAction;
                        // If not found in DependencyContainer, use Activator to create instance
                        else
                            gameAction = Activator.CreateInstance(executionType) as IGameAction;
                        if (gameAction != null)
                        {
                            ActionInput actionInput = new ActionInput(ci, this, string.Empty/*TODO*/, command, rawParameters, parameters);
                            string guardsResult = gameAction.Guards(actionInput);
                            if (guardsResult != null)
                            {
                                Send(guardsResult);
                                return false;
                            }
                            gameAction.Execute(actionInput);
                            return true;
                        }
                        else
                        {
                            Log.Default.WriteLine(LogLevels.Error, "Command: {0} not found.", command);
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

        protected virtual bool ExecuteBeforeCommand(CommandMethodInfo methodInfo, string rawParameters, params CommandParameter[] parameters) => true;

        protected virtual bool ExecuteAfterCommand(CommandMethodInfo methodInfo, string rawParameters, params CommandParameter[] parameters) => true;

        public virtual bool IsCommandAvailable(CommandAttribute attribute) => true;

        protected static IReadOnlyTrie<CommandExecutionInfo> GetCommands<T>()
            where T : ActorBase
            => CommandManager.GetCommands(typeof(T));

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
