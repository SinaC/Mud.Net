using System;
using System.Text;
using Mud.DataStructures.Trie;
using Mud.Logger;
using Mud.Server.Helpers;
using Mud.Server.Input;

namespace Mud.Server.Admin
{
    // TODO: cannot impersonate while already incarnated, cannot incarnate while already impersonated
    public partial class Admin : Player.Player, IAdmin
    {
        private static readonly IReadOnlyTrie<CommandMethodInfo> AdminCommands;

        static Admin()
        {
            AdminCommands = CommandHelpers.GetCommands(typeof(Admin));
        }

        public Admin(Guid id, string name) 
            : base(id, name)
        {
        }

        #region IAdmin

        #region IPlayer

        #region IActor

        public override IReadOnlyTrie<CommandMethodInfo> Commands
        {
            get { return AdminCommands; }
        }

        public override bool ProcessCommand(string commandLine) // TODO: refactoring needed: almost same code in Player
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
                if (forceOutOfGame || (Impersonating == null && Incarnating == null))
                {
                    Log.Default.WriteLine(LogLevels.Debug, "[{0}] executing [{1}]", Name, commandLine);
                    executedSuccessfully = ExecuteCommand(command, rawParameters, parameters);
                }
                else if (Incarnating != null)
                {
                    Log.Default.WriteLine(LogLevels.Debug, "[{0}]|[{1}] executing [{2}]", Name, Incarnating.Name, commandLine);
                    executedSuccessfully = Incarnating.ExecuteCommand(command, rawParameters, parameters);
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

        #endregion

        public override void OnDisconnected()
        {
            base.OnDisconnected();

            // Stop incarnation if any
            if (Incarnating != null)
            {
                Incarnating.ChangeIncarnation(null);
                Incarnating = null;
            }
        }

        #endregion

        public IEntity Incarnating { get; private set; }

        #endregion

        [Command("shutdow", Hidden = true)] // TODO: add an option in CommandAttribute to force full command to be type
        protected virtual bool DoShutdow(string rawParameters, params CommandParameter[] parameters)
        {
            Send("If you want to SHUTDOWN, spell it out." + Environment.NewLine);
            return true;
        }

        [Command("shutdown")]
        protected virtual bool DoShutdown(string rawParameters, params CommandParameter[] parameters)
        {
            int seconds;
            if (parameters.Length == 0 || !int.TryParse(parameters[0].Value, out seconds))
                Send("Syntax: shutdown xxx  where xxx is a delay in seconds." + Environment.NewLine);
            else if (seconds < 30)
                Send("You cannot shutdown that fast." + Environment.NewLine);
            else
                Server.Server.Instance.Shutdown(seconds);
            return true;
        }

        [Command("who")]
        protected override bool DoWho(string rawParameters, params CommandParameter[] parameters)
        {
            Send("Players:" + Environment.NewLine);
            foreach (IPlayer player in Server.Server.Instance.GetPlayers())
            {
                StringBuilder sb = new StringBuilder();
                switch (player.PlayerState)
                {
                    case PlayerStates.Impersonating:
                        if (player.Impersonating != null)
                            sb.AppendFormat("[ IG] {0} playing {1}", player.DisplayName, player.Impersonating.Name);
                        else
                            sb.AppendFormat("[ IG] {0} playing ???", player.DisplayName);
                        break;
                    default:
                        sb.AppendFormat("[OOG] {0} {1}", player.DisplayName, player.PlayerState);
                        break;
                }
                sb.AppendLine();
                Send(sb);
            }
            Send("Admins" + Environment.NewLine);
            foreach (IAdmin admin in Server.Server.Instance.GetAdmins())
            {
                StringBuilder sb = new StringBuilder();
                switch (admin.PlayerState)
                {
                    case PlayerStates.Impersonating:
                        if (admin.Impersonating != null)
                            sb.AppendFormat("[ IG] {0} impersonating {1}", admin.DisplayName, admin.Impersonating.Name);
                        else if (admin.Incarnating != null)
                            sb.AppendFormat("[ IG] {0} incarnating {1}", admin.DisplayName, admin.Incarnating.Name);
                        else
                            sb.AppendFormat("[ IG] {0} nor playing nor incarnating !!!", admin.DisplayName);
                        break;
                    default:
                        sb.AppendFormat("[OOG] {0} {1}", admin.DisplayName, admin.PlayerState);
                        break;
                }
                sb.AppendLine();
                Send(sb);
            }
            return true;
        }
    }
}
