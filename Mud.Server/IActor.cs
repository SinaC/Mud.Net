using System.Text;
using Mud.DataStructures.Trie;
using Mud.Server.Input;

namespace Mud.Server
{
    public interface IActor
    {
        IReadOnlyTrie<CommandMethodInfo> Commands { get; } // list of commands accessible to Actor (used by ExecuteCommand)

        bool ProcessCommand(string commandLine); // split commandLine into command and parameters, then call ExecuteCommand
        bool ExecuteCommand(string command, string rawParameters, params CommandParameter[] parameters); // Search command in Commands, then execute it
        
        void Send(string message); // Send message to Actor
        void Send(string format, params object[] parameters); // Send overload
        void Send(StringBuilder sb); // Send overload

        void Page(StringBuilder text);
    }
}
