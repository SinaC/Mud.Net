using System;
using System.Reflection;
using Mud.DataStructures;
using Mud.Logger;

namespace Mud.Server.Player
{
    public partial class Player : ActorBase, IPlayer
    {
        private static readonly IReadOnlyTrie<MethodInfo> PlayerCommands;

        static Player()
        {
            PlayerCommands = CommandHelpers.GetCommands(typeof(Player));
        }

        public Player(Guid id, string name)
        {
            Id = id;
            Name = name;
        }

        #region IPlayer

        #region IActor
        
        public override IReadOnlyTrie<MethodInfo> Commands
        {
            get { return PlayerCommands; }
        }

        public override bool ProcessCommand(string commandLine)
        {
            string command;
            string rawParameters;
            CommandParameter[] parameters;
            bool forceOutOfGame;

            LastCommand = commandLine;
            LastCommandTimestamp = DateTime.Now;

            // Extract command and parameters
            bool extractedSuccessfully = CommandHelpers.ExtractCommandAndParameters(commandLine, out command, out rawParameters, out parameters, out forceOutOfGame);
            if (!extractedSuccessfully)
            {
                Log.Default.WriteLine(LogLevels.Warning, "Command and parameters not extracted successfully");
                Send("Invalid command or parameters");
                return false;
            }

            bool executedSuccessfully;
            if (forceOutOfGame || Impersonating == null)
            {
                Log.Default.WriteLine(LogLevels.Debug, "[{0}] executing [{1}]", Name, commandLine);
                executedSuccessfully = ExecuteCommand(command, rawParameters, parameters);
            }
            else
            {
                Log.Default.WriteLine(LogLevels.Debug, "[{0}]|[{1}] executing [{2}]", Name, Impersonating.Name, commandLine);
                executedSuccessfully = Impersonating.ExecuteCommand(command, rawParameters, parameters);
            }
            if (!executedSuccessfully)
                Log.Default.WriteLine(LogLevels.Warning, "Error while executing command");
            return executedSuccessfully;
        }

        public override void Send(string format, params object[] parameters)
        {
            // TODO: get Socket, push message
            string message = String.Format(format, parameters);
            Log.Default.WriteLine(LogLevels.Info, "SENDING TO [{0}]:[[["+message+"]]]", Name);
        }

        #endregion

        public Guid Id { get; private set; }
        public string Name { get; private set; }

        public ICharacter Impersonating { get; private set; }

        public DateTime LastCommandTimestamp { get; private set; }
        public string LastCommand { get; private set; }

        public virtual void OnDisconnected()
        {
            // Stop impersonation if any
            if (Impersonating != null)
            {
                Impersonating.ChangeImpersonation(null);
                Impersonating = null;
            }
        }

        #endregion

        [Command("test")]
        protected virtual bool Test(string rawParameters, CommandParameter[] parameters)
        {
            return true;
        }
    }
}
