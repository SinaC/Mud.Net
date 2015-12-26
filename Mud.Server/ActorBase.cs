using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Mud.DataStructures;
using Mud.Logger;

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
                TrieEntry<MethodInfo> entry = methodInfos.FirstOrDefault();
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

        #endregion
    }
}
