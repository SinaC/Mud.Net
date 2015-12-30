using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Mud.DataStructures.Trie;
using Mud.Logger;
using Mud.Server.Input;

namespace Mud.Server.Actor
{
    public abstract class ActorBase : IActor
    {
        #region IActor

        public abstract IReadOnlyTrie<CommandMethodInfo> Commands { get; }

        public abstract bool ProcessCommand(string commandLine);
        public abstract void Send(string format, params object[] parameters);

        public bool ExecuteCommand(string command, string rawParameters, CommandParameter[] parameters)
        {
            // Search for command and invoke it
            if (Commands != null)
            {
                List<TrieEntry<CommandMethodInfo>> methodInfos = Commands.GetByPrefix(command).ToList();
                TrieEntry<CommandMethodInfo> entry = methodInfos.OrderBy(x => x.Value.Attribute.Priority).FirstOrDefault(); // use priority to choose between conflicting commands
                if (entry.Value != null && entry.Value.MethodInfo != null)
                {
                    MethodInfo methodInfo = entry.Value.MethodInfo;
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
                    Log.Default.WriteLine(LogLevels.Warning, "Command not found");
                    Send("Command not found");
                    return false;
                }
            }
            else
            {
                Log.Default.WriteLine(LogLevels.Warning, "Command not found");
                Send("Command not found");
                return false;
            }
        }

        [Command("commands")]
        protected virtual bool DoCommands(string rawParameters, params CommandParameter[] parameters)
        {
            // TODO: group trie by value and display set of key linked to this value

            Send("Available commands:");
            StringBuilder sb = new StringBuilder();
            int index = 0;
            foreach (string command in Commands.Keys.OrderBy(x => x))
            {
                //Send(command); // TODO: display 6 by 6
                if ((++index%6) == 0)
                {
                    sb.AppendFormat("{0,-13}", command);
                    Send(sb.ToString());
                    sb = new StringBuilder();
                }
                else
                    sb.AppendFormat("{0,-13}", command);
            }
            if (sb.Length > 0)
                Send(sb.ToString());

            return true;
        }

        #endregion
    }
}
