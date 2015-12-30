using System;
using System.Collections.Concurrent;
using System.Text;
using Mud.DataStructures.Trie;
using Mud.Logger;
using Mud.Network;
using Mud.Server.Actor;
using Mud.Server.Helpers;
using Mud.Server.Input;

namespace Mud.Server.Player
{
    public partial class Player : ActorBase, IPlayer
    {
        private static readonly IReadOnlyTrie<CommandMethodInfo> PlayerCommands;

        //private readonly ConcurrentQueue<string> _sendQueue; // when using this, Server.ProcessOutput must loop until DataToSend returns null
        private readonly StringBuilder _sendBuffer;
        private readonly IClient _client;
        private readonly IInputTrap<IPlayer> _currentStateMachine; // TODO: state machine for avatar creation

        static Player()
        {
            PlayerCommands = CommandHelpers.GetCommands(typeof(Player));
        }

        protected Player()
        {
            //_sendQueue = new ConcurrentQueue<string>();
            _sendBuffer = new StringBuilder();
        }

        public Player(IClient client, Guid id)
            :this()
        {
            _client = client;
            Id = id;

            client.DataReceived += ClientOnDataReceived;
            client.Disconnected += OnDisconnected;

            _currentStateMachine = new LoginStateMachine();

            PlayerState = PlayerStates.Connecting;
        }

        // TODO: remove  method is for test purpose  (no login state machine and name directly specified)
        public Player(IClient client, Guid id, string name)
            : this()
        {
            _client = client;
            Id = id;
            Name = name;

            client.DataReceived += ClientOnDataReceived;
            client.Disconnected += OnDisconnected;

            PlayerState = PlayerStates.Connected;
        }

        #region IPlayer

        #region IActor

        public override IReadOnlyTrie<CommandMethodInfo> Commands
        {
            get { return PlayerCommands; }
        }

        public override bool ProcessCommand(string commandLine)
        {
            // ! means repeat last command
            if (commandLine != null && commandLine.Length >= 1 && commandLine[0] == '!')
            {
                commandLine = LastCommand;
                LastCommandTimestamp = DateTime.Now;
            }
            else
            {
                LastCommand = commandLine;
                LastCommandTimestamp = DateTime.Now;
            }

            if (_currentStateMachine != null && !_currentStateMachine.IsFinalStateReached)
            {
                _currentStateMachine.ProcessInput(this, commandLine);
                return true;
            }
            else
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
        }

        public override void Send(string format, params object[] parameters)
        {
            if (ServerOptions.AsynchronousSend)
            {
                string message = String.Format(format + Environment.NewLine, parameters);
                _client.WriteData(message);
            }
            else
            {
                string message = String.Format(format, parameters);
                //_sendQueue.Enqueue(message);
                lock (_sendBuffer)
                    _sendBuffer.AppendLine(message);
            }
        }

        #endregion

        public Guid Id { get; private set; }
        public string Name { get; private set; }

        public string DisplayName
        {
            get { return StringHelpers.UpperFirstLetter(Name); } // TODO: store another string or perform transformation on-the-fly ???
        }

        public PlayerStates PlayerState { get; protected set; }

        public ICharacter Impersonating { get; private set; }

        public DateTime LastCommandTimestamp { get; private set; }
        public string LastCommand { get; private set; }

        public bool Load(string name)
        {
            Name = name;
            // TODO: load player file

            PlayerState = PlayerStates.Connected;
            return true;
        }

        public virtual void OnDisconnected()
        {
            // Stop impersonation if any
            if (Impersonating != null)
            {
                Impersonating.ChangeImpersonation(null);
                Impersonating = null;
            }
        }

        public string DataToSend()
        {
            //string data;
            //bool taken = _sendQueue.TryDequeue(out data);
            //return taken ? data : null;
            lock (_sendBuffer)
            {
                string data = _sendBuffer.ToString();
                _sendBuffer.Clear();
                return data;
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
        protected virtual bool DoTest(string rawParameters, params CommandParameter[] parameters)
        {
            Send("Player test");
            return true;
        }
    }
}
