using System;
using System.Text;
using Mud.DataStructures.Trie;
using Mud.Logger;
using Mud.Server.Actor;
using Mud.Server.Helpers;
using Mud.Server.Input;

namespace Mud.Server.Player
{
    public partial class Player : ActorBase, IPlayer
    {
        private static readonly IReadOnlyTrie<CommandMethodInfo> PlayerCommands;

        private readonly IInputTrap<IPlayer> _currentStateMachine; // TODO: state machine for avatar creation

        static Player()
        {
            PlayerCommands = CommandHelpers.GetCommands(typeof(Player));
        }

        protected Player()
        {
        }

        public Player(Guid id)
            :this()
        {
            Id = id;

            _currentStateMachine = new LoginStateMachine();

            PlayerState = PlayerStates.Connecting;
        }

        // TODO: remove  method is for test purpose  (no login state machine and name directly specified)
        public Player(Guid id, string name)
            : this()
        {
            Id = id;
            Name = name;

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

            // If an input state machine is running, send commandLine to machine
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
                    Send("Invalid command or parameters" + Environment.NewLine);
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

        public override void Send(string message)
        {
            if (SendData != null)
                SendData(this, message);
        }

        public override void Page(StringBuilder text)
        {
            if (PageData != null)
                PageData(this, text);
        }

        #endregion

        public event SendDataEventHandler SendData;
        public event PageDataEventHandler PageData;

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

        #endregion

        [Command("test")]
        protected virtual bool DoTest(string rawParameters, params CommandParameter[] parameters)
        {
            Send("Player: DoTest" + Environment.NewLine);
            StringBuilder lorem = new StringBuilder("1/Lorem ipsum dolor sit amet, " + Environment.NewLine +
                                                    "2/consectetur adipiscing elit, " + Environment.NewLine +
                                                    "3/sed do eiusmod tempor incididunt " + Environment.NewLine +
                                                    "4/ut labore et dolore magna aliqua. " + Environment.NewLine +
                                                    "5/Ut enim ad minim veniam, " + Environment.NewLine +
                                                    "6/quis nostrud exercitation ullamco " + Environment.NewLine +
                                                    "7/laboris nisi ut aliquip ex " + Environment.NewLine +
                                                    "8/ea commodo consequat. " + Environment.NewLine +
                                                    "9/Duis aute irure dolor in " + Environment.NewLine +
                                                    "10/reprehenderit in voluptate velit " + Environment.NewLine +
                                                    "11/esse cillum dolore eu fugiat " + Environment.NewLine +
                                                    "12/nulla pariatur. " + Environment.NewLine +
                                                    "13/Excepteur sint occaecat " + Environment.NewLine +
                                                    "14/cupidatat non proident, " + Environment.NewLine +
                                                    "15/sunt in culpa qui officia deserunt " + Environment.NewLine +
                                                    "16/mollit anim id est laborum." + Environment.NewLine);
            Page(lorem);
            return true;
        }
    }
}
