using System.Collections.Generic;
using System.Reflection;
using Mud.Logger;

namespace Mud.Server
{
    public abstract class ActorBase : IActor
    {
        #region IActor

        public abstract IReadOnlyDictionary<string, MethodInfo> Commands { get; }

        public abstract bool ProcessCommand(string commandLine);
        public abstract void Send(string format, params object[] parameters);

        public bool ExecuteCommand(string command, string rawParameters, CommandParameter[] parameters)
        {
            // Search for command and invoke it
            MethodInfo methodInfo;
            if (Commands != null && Commands.TryGetValue(command, out methodInfo))
            {
                bool executedSuccessfully = (bool)methodInfo.Invoke(this, new object[] { rawParameters, parameters });
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

        #endregion
    }
}
