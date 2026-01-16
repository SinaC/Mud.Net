using Mud.DataStructures.Trie;
using Mud.Server.Interfaces.Actor;

namespace Mud.Server.Interfaces.GameAction;

public interface IGameActionManager
{
    IEnumerable<IGameActionInfo> GameActions { get; }

    string? Execute<TActor>(IGameActionInfo gameActionInfo, TActor actor, string commandLine, string command, params ICommandParameter[] parameters)
        where TActor: IActor;

    string? Execute<TActor>(Type gameActionType, TActor actor, string? commandLine)
        where TActor : IActor;

    string? Execute<TGameAction, TActor>(TActor actor, string? commandLine)
        where TActor : IActor;

    IReadOnlyTrie<IGameActionInfo> GetGameActions<TActor>()
        where TActor : IActor;
}
