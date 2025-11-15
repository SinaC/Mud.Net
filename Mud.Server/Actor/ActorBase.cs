using Mud.Container;
using Mud.DataStructures.Trie;
using Mud.Logger;
using Mud.Server.Interfaces.Actor;
using Mud.Server.Interfaces.GameAction;
using System.Text;

namespace Mud.Server.Actor;

public abstract class ActorBase : IActor
{
    protected IGameActionManager GameActionManager => DependencyContainer.Current.GetInstance<IGameActionManager>();

    #region IActor

    public abstract IReadOnlyTrie<IGameActionInfo> GameActions { get; }

    public abstract bool ProcessInput(string input);
    public abstract void Send(string message, bool addTrailingNewLine);
    public abstract void Page(StringBuilder text);

    public bool ExecuteCommand(string commandLine, string command, ICommandParameter[] parameters)
    {
        // Search for game action and invoke it
        if (GameActions != null)
        {
            command = command.ToLowerInvariant(); // lower command
            List<TrieEntry<IGameActionInfo>> gameActionInfos = GameActions.GetByPrefix(command).ToList();
            TrieEntry<IGameActionInfo> entry = gameActionInfos.OrderBy(x => x.Value.Priority).FirstOrDefault(); // use priority to choose between conflicting gameactions
            if (entry.Value?.NoShortcut == true && command != entry.Key) // if gameaction doesn't accept shortcut, inform player
            {
                Send("If you want to {0}, spell it out.", entry.Key.ToUpper());
                return true;
            }
            if (entry.Value is IGameActionInfo gai && gai.CommandExecutionType != null)
            {
                var executionResults = GameActionManager.Execute(gai, this, commandLine, entry.Key, parameters);
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
}
