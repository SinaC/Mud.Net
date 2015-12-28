using System;
using System.Reflection;
using System.Runtime.Remoting.Channels;
using Mud.DataStructures;
using Mud.Logger;
using Mud.Network;

namespace Mud.Server.Player
{
    public partial class Player : ActorBase, IPlayer
    {
        private static readonly Trie<MethodInfo> PlayerCommands;

        private readonly IClient _client;

        static Player()
        {
            PlayerCommands = CommandHelpers.GetCommands(typeof(Player));
        }

        public Player(IClient client, Guid id, string name)
        {
            _client = client;
            Id = id;
            Name = name;

            client.DataReceived += ClientOnDataReceived;
            client.Disconnected += OnDisconnected;
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
            string message = String.Format(format+Environment.NewLine, parameters);
            _client.WriteData(message);
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

        #region IClient event handlers

        private void ClientOnDataReceived(string data)
        {
            ProcessCommand(data);
        }

        #endregion

        [Command("test")]
        protected virtual bool Test(string rawParameters, CommandParameter[] parameters)
        {
            return true;
        }
    }
}
