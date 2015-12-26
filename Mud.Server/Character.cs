using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Mud.Logger;

namespace Mud.Server
{
    public class Character : ICharacter
    {
        private static readonly IReadOnlyDictionary<string, MethodInfo> Commands;

        static Character()
        {
            Commands = typeof(Player).GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
                .Where(x => x.GetCustomAttributes(typeof(CommandAttribute), false).Any())
                .ToDictionary(x => x.GetCustomAttributes(typeof(CommandAttribute)).OfType<CommandAttribute>().First().Name);
        }
        
        public bool ProcessCommand(string commandLine)
        {
            string command;
            string rawParameters;
            CommandParameter[] parameters;
            bool forceOutOfGame;

            // Extract command and parameters
            bool extractedSuccessfully = CommandHelpers.ExtractCommandAndParameters(commandLine, out command, out rawParameters, out parameters, out forceOutOfGame);
            if (!extractedSuccessfully)
            {
                Log.Default.WriteLine(LogLevels.Warning, "Command and parameters not extracted successfully");
                return false;
            }

            Log.Default.WriteLine(LogLevels.Info, "Executing [{0}] as ICharacter command", command);
            return ExecuteCommand(command, rawParameters, parameters);
        }

        public bool ExecuteCommand(string command, string rawParameters, CommandParameter[] parameters)
        {
            // Search for command
            MethodInfo methodInfo;
            if (Commands.TryGetValue(command, out methodInfo))
            {
                bool executedSuccessfully = (bool)methodInfo.Invoke(this, new object[] { rawParameters, parameters });
                if (!executedSuccessfully)
                {
                    Log.Default.WriteLine(LogLevels.Warning, "Error while executing command");
                    return false;
                }

                return true;
            }
            return false;
        }

        public void Send(string message)
        {
            throw new NotImplementedException();
        }

        public Guid Id { get; private set; }
        public string Name { get; private set; }
        public string Description { get; private set; }
        public bool Impersonable { get; private set; }

        [Command("look")]
        protected virtual bool Look(string rawParameters, CommandParameter[] parameters)
        {
            return true;
        }

        [Command("order")]
        protected virtual bool Order(string rawParameters, CommandParameter[] parameters)
        {
            return true;
        }

        [Command("kill")]
        protected virtual bool Kill(string rawParameters, CommandParameter[] parameters)
        {
            return true;
        }

        [Command("test")]
        protected virtual bool Test(string rawParameters, CommandParameter[] parameters)
        {
            return true;
        }
    }
}
