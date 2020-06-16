using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Mud.Container;
using Mud.DataStructures.Trie;
using Mud.Domain;
using Mud.Logger;
using Mud.Server.GameAction;
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
        protected IGameActionManager GameActionManager => DependencyContainer.Current.GetInstance<IGameActionManager>();

        #region IActor

        public abstract IReadOnlyTrie<ICommandExecutionInfo> Commands { get; }

        public abstract bool ProcessCommand(string commandLine);
        public abstract void Send(string message, bool addTrailingNewLine);
        public abstract void Page(StringBuilder text);

        public bool ExecuteCommand(string command, string rawParameters, ICommandParameter[] parameters)
        {
            // Search for command and invoke it
            if (Commands != null)
            {
                command = command.ToLowerInvariant(); // lower command
                List<TrieEntry<ICommandExecutionInfo>> methodInfos = Commands.GetByPrefix(command).ToList();
                TrieEntry<ICommandExecutionInfo> entry = methodInfos.OrderBy(x => x.Value.Priority).FirstOrDefault(); // use priority to choose between conflicting commands
                if (entry.Value?.NoShortcut == true && command != entry.Key) // if command doesn't accept shortcut, inform player
                {
                    Send("If you want to {0}, spell it out.", entry.Key.ToUpper());
                    return true;
                }
                else if (entry.Value is ICommandMethodInfo cmi && cmi.MethodInfo != null)
                {
                    MethodInfo methodInfo = cmi.MethodInfo;
                    object rawExecutionResult;
                    if (entry.Value?.AddCommandInParameters == true)
                    {
                        // Insert command as first parameter
                        ICommandParameter[] enhancedParameters = new ICommandParameter[(parameters?.Length ?? 0) + 1];
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
                        StringBuilder syntax = GameActionBase<IActor,IGameActionInfo>.BuildCommandSyntax(entry.Key, entry.Value.Syntax, false);
                        Send(syntax);
                    }
                    return true;
                }
                else if (entry.Value is GameActionInfo ci && ci.CommandExecutionType != null)
                {
                    Type executionType = ci.CommandExecutionType;
                    IGameAction gameAction = GameActionManager.CreateInstance(entry.Key);
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

        protected static IReadOnlyTrie<ICommandExecutionInfo> GetCommands<T>()
            where T : ActorBase
            => GameAction.GameActionManager.GetCommands(typeof(T));

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
