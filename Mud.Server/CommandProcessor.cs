using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Mud.Logger;

namespace Mud.Server
{
    public class CommandProcessor : ICommandProcessor
    {
        private static readonly Dictionary<string, MethodInfo> Commands;

        static CommandProcessor()
        {
            Commands = new Dictionary<string, MethodInfo>();
            Commands = typeof(CommandProcessor).GetMethods()
                .Where(m => m.GetCustomAttributes().OfType<CommandAttribute>().Any())
                .ToDictionary(x => x.Name.ToLower());
        }

        public void ProcessCommand(IClient client, string commandLine)
        {
            // TODO
            Log.Default.WriteLine(LogLevels.Debug, "Processing " + commandLine + " from client " + client.Id);

            // Split commandLine into command and arguments
            string[] tokens = commandLine.Split(' ');
            // command is first token
            string command = tokens[0];
            // parameters is the remaining
            string parameters = commandLine.Remove(0, command.Length);
            MethodInfo methodInfo;
            if (Commands.TryGetValue(command, out methodInfo))
            {
                Log.Default.WriteLine(LogLevels.Debug, "Command [" + command + "] found with parameters [" + parameters + "]. Executing ...");

                // TODO: if CommandWithTarget, search target and pass it as 2nd parameter
                methodInfo.Invoke(this, new object[] { client, parameters });
            }
        }

        [Command]
        public void Scan(IClient client, string data)
        {
            Log.Default.WriteLine(LogLevels.Debug, "Scan: " + (client == null));
        }

        [CommandWithTarget]
        public void Tell(IClient client, IClient target, string data)
        {
            Log.Default.WriteLine(LogLevels.Debug, "Tell: " + (client == null)+ " " + (target == null) + " "+(data == null));
        }
    }
}
