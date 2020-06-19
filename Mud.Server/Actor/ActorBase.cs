using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mud.Container;
using Mud.DataStructures.Trie;
using Mud.Logger;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Actor;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.World;
using Mud.Settings;

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

        public abstract IReadOnlyTrie<IGameActionInfo> Commands { get; }

        public abstract bool ProcessCommand(string commandLine);
        public abstract void Send(string message, bool addTrailingNewLine);
        public abstract void Page(StringBuilder text);

        public bool ExecuteCommand(string command, string rawParameters, ICommandParameter[] parameters)
        {
            // Search for command and invoke it
            if (Commands != null)
            {
                command = command.ToLowerInvariant(); // lower command
                List<TrieEntry<IGameActionInfo>> gameActionInfos = Commands.GetByPrefix(command).ToList();
                TrieEntry<IGameActionInfo> entry = gameActionInfos.OrderBy(x => x.Value.Priority).FirstOrDefault(); // use priority to choose between conflicting commands
                if (entry.Value?.NoShortcut == true && command != entry.Key) // if command doesn't accept shortcut, inform player
                {
                    Send("If you want to {0}, spell it out.", entry.Key.ToUpper());
                    return true;
                }
                if (entry.Value is IGameActionInfo gai && gai.CommandExecutionType != null)
                {
                    string executionResults = GameActionManager.Execute(gai, this, command, rawParameters, parameters);
                    if (executionResults != null)
                    {
                        Send(executionResults);
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

        protected static IReadOnlyTrie<IGameActionInfo> GetCommands<T>()
            where T : ActorBase
            => GameAction.GameActionManager.GetCommands(typeof(T));
    }
}
