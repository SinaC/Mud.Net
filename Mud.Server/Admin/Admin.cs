using System;
using Mud.Datas.DataContracts;
using Mud.DataStructures.Trie;
using Mud.Logger;
using Mud.Server.Constants;
using Mud.Server.Input;

namespace Mud.Server.Admin
{
    public partial class Admin : Player.Player, IAdmin
    {
        private static readonly Lazy<IReadOnlyTrie<CommandMethodInfo>> AdminCommands = new Lazy<IReadOnlyTrie<CommandMethodInfo>>(() => CommandHelpers.GetCommands(typeof(Admin)));

        public Admin(Guid id, string name) 
            : base(id, name)
        {
        }

        #region IAdmin

        #region IPlayer

        #region IActor

        public override IReadOnlyTrie<CommandMethodInfo> Commands => AdminCommands.Value;

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
                // Extract command and parameters
                bool extractedSuccessfully = CommandHelpers.ExtractCommandAndParameters(Aliases, commandLine, out var command, out var rawParameters, out var parameters, out var forceOutOfGame);
                if (!extractedSuccessfully)
                {
                    Log.Default.WriteLine(LogLevels.Warning, "Command and parameters not extracted successfully");
                    Send("Invalid command or parameters");
                    return false;
                }

                // Execute command
                bool executedSuccessfully;
                if (forceOutOfGame || (Impersonating == null && Incarnating == null)) // neither incarnating nor impersonating
                {
                    Log.Default.WriteLine(LogLevels.Debug, "[{0}] executing [{1}]", DisplayName, commandLine);
                    executedSuccessfully = ExecuteCommand(command, rawParameters, parameters);
                }
                else if (Incarnating != null) // incarnating
                {
                    Log.Default.WriteLine(LogLevels.Debug, "[{0}]|[{1}] executing [{2}]", DisplayName, Incarnating.DebugName, commandLine);
                    executedSuccessfully = Incarnating.ExecuteCommand(command, rawParameters, parameters);
                }
                else if (Impersonating != null) // impersonating
                {
                    Log.Default.WriteLine(LogLevels.Debug, "[{0}]|[{1}] executing [{2}]", DisplayName, Impersonating.DebugName, commandLine);
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

        //public override bool ExecuteBeforeCommand(CommandMethodInfo methodInfo, string rawParameters, params CommandParameter[] parameters)
        //{
        //    AdminCommandAttribute adminCommandAttribute = methodInfo.Attribute as AdminCommandAttribute;
        //    if (adminCommandAttribute?.MinLevel > Level)
        //    {
        //        Send($"You're a not allowed to use '{adminCommandAttribute.Name}'.");
        //        Log.Default.WriteLine(LogLevels.Info, $"{DisplayName} [Level:{Level}] tried to use {adminCommandAttribute.Name} [Level:{adminCommandAttribute.MinLevel}]");
        //        return false;
        //    }
        //    return base.ExecuteBeforeCommand(methodInfo, rawParameters, parameters);
        //}

        protected override bool IsCommandAvailable(CommandAttribute attribute)
        {
            AdminCommandAttribute adminCommandAttribute = attribute as AdminCommandAttribute;
            return adminCommandAttribute == null || Level >= adminCommandAttribute?.MinLevel;
        }

        #endregion

        public override string Prompt => Incarnating != null 
            ? BuildIncarnatePrompt()
            : base.Prompt;

        public override bool Load(string name)
        {
            AdminData data = Repository.AdminManager.Load(name);
            // Load player data
            LoadPlayerData(data);
            // Load admin datas
            Level = data?.Level ?? AdminLevels.Angel;
            WiznetFlags = data?.WiznetFlags ?? 0;
            //
            PlayerState = PlayerStates.Playing;
            return true;
        }

        public override bool Save()
        {
            if (Impersonating != null)
                UpdateCharacterDataFromImpersonated();
            //
            AdminData data = new AdminData();
            // Fill player data
            FillPlayerData(data);
            // Fill admin data
            data.Level = Level;
            data.WiznetFlags = WiznetFlags;
            //
            Repository.AdminManager.Save(data);
            //
            Log.Default.WriteLine(LogLevels.Info, $"Admin {DisplayName} saved");
            return true;
        }

        public override void OnDisconnected()
        {
            base.OnDisconnected();

            // Stop incarnation if any
            if (Incarnating != null)
            {
                StopIncarnating();
            }
        }

        #endregion

        public AdminLevels Level { get; private set; }

        public WiznetFlags WiznetFlags { get; private set; }

        public IEntity Incarnating { get; private set; }

        public void StopIncarnating()
        {
            Incarnating?.ChangeIncarnation(null);
            Incarnating = null;
        }

        #endregion

        private string BuildIncarnatePrompt()
        {
            if (Incarnating is ICharacter character)
                return BuildCharacterPrompt(character);
            return $"{Incarnating.DisplayName}>";
        }
    }
}
