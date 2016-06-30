using System;
using Mud.DataStructures.Trie;
using Mud.Logger;
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

        public override IReadOnlyTrie<CommandMethodInfo> Commands => AdminCommands;

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
            if (CurrentStateMachine != null && !CurrentStateMachine.IsFinalStateReached)
            {
                CurrentStateMachine.ProcessInput(this, commandLine);
                return true;
            }
            else
            {
                string command;
                string rawParameters;
                CommandParameter[] parameters;
                bool forceOutOfGame;

                // Extract command and parameters
                bool extractedSuccessfully = CommandHelpers.ExtractCommandAndParameters(Aliases, commandLine, out command, out rawParameters, out parameters, out forceOutOfGame);
                if (!extractedSuccessfully)
                {
                    Log.Default.WriteLine(LogLevels.Warning, "Command and parameters not extracted successfully");
                    Send("Invalid command or parameters" + Environment.NewLine);
                    return false;
                }

                // Execute command
                bool executedSuccessfully;
                if (forceOutOfGame || (Impersonating == null && Incarnating == null)) // neither incarnating nor impersonating
                {
                    Log.Default.WriteLine(LogLevels.Debug, "[{0}] executing [{1}]", Name, commandLine);
                    executedSuccessfully = ExecuteCommand(command, rawParameters, parameters);
                }
                else if (Incarnating != null) // incarnating
                {
                    Log.Default.WriteLine(LogLevels.Debug, "[{0}]|[{1}] executing [{2}]", Name, Incarnating.Name, commandLine);
                    executedSuccessfully = Incarnating.ExecuteCommand(command, rawParameters, parameters);
                }
                else if (Impersonating != null) // impersonating
                {
                    Log.Default.WriteLine(LogLevels.Debug, "[{0}]|[{1}] executing [{2}]", Name, Impersonating.Name, commandLine);
                    executedSuccessfully = Impersonating.ExecuteCommand(command, rawParameters, parameters);
                }
                else
                {
                    Log.Default.WriteLine(LogLevels.Error, "[{0}] is neither out of game, nor impersonating, nor incarnating");
                    executedSuccessfully = false;
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
    }
}
