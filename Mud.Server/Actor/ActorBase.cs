using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Mud.DataStructures.Trie;
using Mud.Logger;
using Mud.Server.Helpers;
using Mud.Server.Input;

namespace Mud.Server.Actor
{
    public abstract class ActorBase : IActor
    {
        #region IActor

        public abstract IReadOnlyTrie<CommandMethodInfo> Commands { get; }

        public abstract bool ProcessCommand(string commandLine);
        public abstract void Send(string message);
        public abstract void Page(StringBuilder text);

        public bool ExecuteCommand(string command, string rawParameters, CommandParameter[] parameters)
        {
            // Search for command and invoke it
            if (Commands != null)
            {
                List<TrieEntry<CommandMethodInfo>> methodInfos = Commands.GetByPrefix(command).ToList();
                TrieEntry<CommandMethodInfo> entry = methodInfos.OrderBy(x => x.Value.Attribute.Priority).FirstOrDefault(); // use priority to choose between conflicting commands
                if (entry.Value?.Attribute?.NoShortcut == true && command != entry.Key) // if command doesn't accept shortcut, inform player
                {
                    Send("If you want to {0}, spell it out."+Environment.NewLine, entry.Key.ToUpper());
                    return true;
                }
                else if (entry.Value?.MethodInfo != null)
                {
                    MethodInfo methodInfo = entry.Value.MethodInfo;
                    bool executedSuccessfully;
                    if (entry.Value.Attribute.AddCommandInParameters)
                    {
                        CommandParameter[] enhancedParameters = new CommandParameter[parameters?.Length + 1 ?? 1];
                        if (parameters != null)
                            Array.ConstrainedCopy(parameters, 0, enhancedParameters, 1, parameters.Length);
                        enhancedParameters[0] = new CommandParameter(command, 1);
                        string enhancedRawParameters = command + " " + rawParameters;
                        executedSuccessfully = (bool)methodInfo.Invoke(this, new object[] { enhancedRawParameters, enhancedParameters });
                    }
                    else
                        executedSuccessfully = (bool) methodInfo.Invoke(this, new object[] {rawParameters, parameters});
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
                    Send("Command not found"+Environment.NewLine);
                    return false;
                }
            }
            else
            {
                Log.Default.WriteLine(LogLevels.Warning, "Command not found");
                Send("Command not found" + Environment.NewLine);
                return false;
            }
        }

        public void Send(string format, params object[] parameters)
        {
            Send(String.Format(format, parameters));
        }

        public void Send(StringBuilder text)
        {
            Send(text.ToString());
        }

        [Command("cmd", Priority = 0)]
        [Command("commands", Priority = 0)]
        protected virtual bool DoCommands(string rawParameters, params CommandParameter[] parameters)
        {
            // TODO: group trie by value and display set of key linked to this value

            IEnumerable<KeyValuePair<string, CommandMethodInfo>> filteredCommands = Commands.Where(x => !x.Value.Attribute.Hidden);
            if (parameters.Length > 0)
                filteredCommands = filteredCommands.Where(x => FindHelpers.StringStartsWith(x.Value.Attribute.Category, parameters[0].Value));

            StringBuilder sb = new StringBuilder("Available commands:"+Environment.NewLine);
            foreach (IGrouping<string, KeyValuePair<string, CommandMethodInfo>> group in filteredCommands
                .GroupBy(x => x.Value.Attribute.Category)
                .OrderBy(g => g.Key))
            {
                if (!String.IsNullOrEmpty(group.Key))
                    sb.AppendLine("%W%" + group.Key + ":%x%");
                int index = 0;
                foreach (KeyValuePair<string, CommandMethodInfo> kv in group
                    .OrderBy(x => x.Value.Attribute.Priority)
                    .ThenBy(x => x.Key))
                {
                    if ((++index%6) == 0)
                        sb.AppendFormatLine("{0,-13}", kv.Key);
                    else
                        sb.AppendFormat("{0,-13}", kv.Key);
                }
                if (index > 0 && index%6 != 0)
                    sb.AppendLine();
            }
            Page(sb);
            return true;
        }

        #endregion
    }
}
