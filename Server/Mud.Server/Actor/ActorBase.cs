using Microsoft.Extensions.Logging;
using Mud.DataStructures.Trie;
using Mud.Server.Parser.Interfaces;
using Mud.Server.Interfaces.Actor;
using Mud.Server.Interfaces.GameAction;
using System.Text;

namespace Mud.Server.Actor;

public abstract class ActorBase : IActor
{
    protected ILogger<ActorBase> Logger { get; }
    protected IGameActionManager GameActionManager { get; }

    protected ActorBase(ILogger<ActorBase> logger, IGameActionManager gameActionManager)
    {
        Logger = logger;
        GameActionManager = gameActionManager;
    }

    #region IActor

    public IReadOnlyTrie<IGameActionInfo> GameActions
        => GameActionManager.GetGameActions(GetType());

    public abstract bool ProcessInput(string input);
    public abstract void Send(string message, bool addTrailingNewLine);
    public abstract void Page(StringBuilder text);

    public bool ExecuteCommand(string commandLine, string command, ICommandParameter[] parameters)
    {
        var actorType = GetType();
        var gameActions = GameActionManager.GetGameActions(actorType);
        // Search for game action and invoke it
        if (gameActions != null)
        {
            command = command.ToLowerInvariant(); // lower command
            var entries = gameActions.GetByPrefix(command).ToList();
            var entry = entries.OrderBy(x => x.Value.Priority).FirstOrDefault(); // use priority to choose between conflicting gameactions
            var gameActionInfo = entry.Value;
            if (gameActionInfo != null)
            {
                if (gameActionInfo.NoShortcut == true && command != entry.Key) // if gameaction doesn't accept shortcut, inform player
                {
                    Send("If you want to {0}, spell it out.", entry.Key.ToUpper());
                    return true;
                }
                if (gameActionInfo.CommandExecutionType != null)
                {
                    var executionResults = GameActionManager.Execute(gameActionInfo, this, commandLine, entry.Key, parameters);
                    if (executionResults != null)
                    {
                        Send(executionResults);
                        return false;
                    }
                    return true;
                }
            }
            //
            Logger.LogWarning("Command {command} not found", command);
            Send("Command not found.");
            return false;
        }
        else
        {
            Logger.LogWarning("No command found for {actorType}", GetType().FullName);
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
