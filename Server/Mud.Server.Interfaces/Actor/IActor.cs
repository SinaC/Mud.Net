using Mud.DataStructures.Trie;
using Mud.Server.Parser.Interfaces;
using Mud.Server.Interfaces.GameAction;
using System.Text;

namespace Mud.Server.Interfaces.Actor;

public interface IActor
{
    IReadOnlyTrie<IGameActionInfo> GameActions { get; } // list of commands accessible to Actor (used by ExecuteCommand)

    bool ProcessInput(string input); // split commandLine into command and parameters, then call ExecuteCommand
    bool ExecuteCommand(string commandLine, IParseResult parseResult); // search command in Commands, then execute it

    void Send(string message, bool addTrailingNewLine); // send message to Actor
    void Send(string format, params object[] parameters); // send overload (this function will automatically add a trailing newline)
    void Send(StringBuilder text); // send overload (This function will not add trailing newline)

    void Page(StringBuilder text); // send a lot of pageable string (this function will not add trailing newline)
}
