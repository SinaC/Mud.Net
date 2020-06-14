using System;
using System.Text;
using Mud.Container;
using Mud.Repository;
using Mud.DataStructures.Trie;
using Mud.Domain;
using Mud.Logger;
using Mud.Server.Input;
using System.Collections.Generic;

namespace Mud.Server.Admin
{
    public partial class Admin : Player.Player, IAdmin
    {
        private static readonly Lazy<IReadOnlyTrie<CommandMethodInfo>> AdminCommands = new Lazy<IReadOnlyTrie<CommandMethodInfo>>(() => GetCommands<Admin>());

        protected IServerAdminCommand ServerAdminCommand => DependencyContainer.Current.GetInstance<IServerAdminCommand>();
        protected IAdminManager AdminManager => DependencyContainer.Current.GetInstance<IAdminManager>();
        protected IAdminRepository AdminRepository => DependencyContainer.Current.GetInstance<IAdminRepository>();

        public Admin(Guid id, string name) 
            : base(id, name)
        {
        }

        // used for promotion
        public Admin(Guid id, string name, AdminLevels level, IReadOnlyDictionary<string,string> aliases, IEnumerable<CharacterData> avatarList)
            : base(id, name, aliases, avatarList)
        {
            Level = level;
        }

        #region IAdmin

        #region IPlayer

        #region IActor

        public override IReadOnlyTrie<CommandMethodInfo> Commands => AdminCommands.Value;

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
            return !(attribute is AdminCommandAttribute adminCommandAttribute) || Level >= adminCommandAttribute.MinLevel;
        }

        #endregion

        public override string Prompt => Incarnating != null
            ? BuildIncarnatePrompt()
            : base.Prompt;

        public override bool Load(string name)
        {
            AdminData data = AdminRepository.Load(name);
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
            AdminRepository.Save(data);
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

        public override StringBuilder PerformSanityCheck()
        {
            StringBuilder sb = base.PerformSanityCheck();

            sb.AppendLine("--Admin--");
            sb.AppendLine($"Incarnating: {Incarnating?.DebugName ?? "none"}");
            sb.AppendLine($"Level: {Level}");
            sb.AppendLine($"WiznetFlags: {WiznetFlags}");

            return sb;
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

        protected override bool InnerExecuteCommand(string commandLine, string command, string rawParameters, CommandParameter[] parameters, bool forceOutOfGame)
        {
            // Execute command
            bool executedSuccessfully;
            if (forceOutOfGame || Impersonating == null)
            {
                Log.Default.WriteLine(LogLevels.Debug, "[{0}] executing [{1}]", DisplayName, commandLine);
                executedSuccessfully = ExecuteCommand(command, rawParameters, parameters);
            }
            else if (Impersonating != null) // impersonating
            {
                Log.Default.WriteLine(LogLevels.Debug, "[{0}]|[{1}] executing [{2}]", DisplayName, Impersonating.DebugName, commandLine);
                executedSuccessfully = Impersonating.ExecuteCommand(command, rawParameters, parameters);
            }
            else
            {
                Log.Default.WriteLine(LogLevels.Error, "[{0}] is neither out of game nor impersonating", DisplayName);
                executedSuccessfully = false;
            }
            if (!executedSuccessfully)
                Log.Default.WriteLine(LogLevels.Warning, "Error while executing command");
            return executedSuccessfully;
        }

        private string BuildIncarnatePrompt()
        {
            if (Incarnating is IPlayableCharacter playableCharacter)
                return BuildCharacterPrompt(playableCharacter);
            return $"{Incarnating.DebugName}>";
        }
    }
}
