using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Mud.DataStructures;
using Mud.DataStructures.Trie;
using Mud.Logger;
using Mud.Server.Input;

namespace Mud.Server
{
    public abstract class ActorBase : IActor
    {
        #region IActor

        public abstract IReadOnlyTrie<MethodInfo> Commands { get; }

        public abstract bool ProcessCommand(string commandLine);
        public abstract void Send(string format, params object[] parameters);

        public bool ExecuteCommand(string command, string rawParameters, CommandParameter[] parameters)
        {
            // Search for command and invoke it
            if (Commands != null)
            {
                List<TrieEntry<MethodInfo>> methodInfos = Commands.GetByPrefix(command).ToList();
                TrieEntry<MethodInfo> entry = methodInfos.FirstOrDefault(); // TODO: use command priority (when typing 'l' as command, 'look' has higher priority than 'list')
                if (entry.Value != null)
                {
                    MethodInfo methodInfo = entry.Value;
                    bool executedSuccessfully = (bool) methodInfo.Invoke(this, new object[] {rawParameters, parameters});
                    if (!executedSuccessfully)
                    {
                        Log.Default.WriteLine(LogLevels.Warning, "Error while executing command");
                        return false;
                    }

                    return true;
                }
                else
                {
                    Send("Command not found");
                    return false;
                }
            }
            else
            {
                Send("Command not found");
                return false;
            }
        }


        [Command("commands")]
        protected virtual bool DoCommands(string rawParameters, CommandParameter[] parameters)
        {
            // TODO: group trie by value and display set of key linked to this value

            Send("Available commands:");
            StringBuilder sb = new StringBuilder();
            foreach (string command in Commands.Keys.OrderBy(x => x))
                Send(command); // TODO: display 6 by 6

            return true;
        }

        #endregion
    }
}
