using System;
using System.Text;
using Mud.Container;
using Mud.Repository;
using Mud.DataStructures.Trie;
using Mud.Domain;
using Mud.Logger;
using System.Collections.Generic;
using Mud.Server.Interfaces.Admin;
using Mud.Server.Interfaces.Class;
using Mud.Server.Interfaces.Race;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.Table;
using Mud.Server.Interfaces.Player;
using Mud.Server.Interfaces.Entity;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Item;
using Mud.Server.Interfaces.Room;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.Admin
{
    public partial class Admin : Player.Player, IAdmin
    {
        protected IClassManager ClassManager => DependencyContainer.Current.GetInstance<IClassManager>();
        protected IRaceManager RaceManager => DependencyContainer.Current.GetInstance<IRaceManager>();
        protected IServerAdminCommand ServerAdminCommand => DependencyContainer.Current.GetInstance<IServerAdminCommand>();
        protected IAdminManager AdminManager => DependencyContainer.Current.GetInstance<IAdminManager>();
        protected IAdminRepository AdminRepository => DependencyContainer.Current.GetInstance<IAdminRepository>();
        protected ITableValues TableValues => DependencyContainer.Current.GetInstance<ITableValues>();
        protected IItemManager ItemManager => DependencyContainer.Current.GetInstance<IItemManager>();
        protected IRoomManager RoomManager => DependencyContainer.Current.GetInstance<IRoomManager>();

        public Admin(Guid id, string name) 
            : base(id, name)
        {
        }

        // used for promotion
        public Admin(Guid id, string name, AdminLevels level, IReadOnlyDictionary<string,string> aliases, IEnumerable<PlayableCharacterData> avatarList)
            : base(id, name, aliases, avatarList)
        {
            Level = level;
        }

        #region IAdmin

        #region IPlayer

        #region IActor

        public override IReadOnlyTrie<IGameActionInfo> Commands => GameActionManager.GetGameActions<Admin>();

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
            Level = data?.AdminLevel ?? AdminLevels.Angel;
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
            data.AdminLevel = Level;
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

        public void AddWiznet(WiznetFlags wiznetFlags)
        {
            WiznetFlags |= wiznetFlags;
        }

        public void RemoveWiznet(WiznetFlags wiznetFlags)
        {
            WiznetFlags &= ~wiznetFlags;
        }

        public IEntity Incarnating { get; private set; }

        public bool StartIncarnating(IEntity entity)
        {
            bool incarnated = entity.ChangeIncarnation(this);
            if (incarnated)
            {
                Incarnating = entity;
                PlayerState = PlayerStates.Impersonating;
            }
            return incarnated;
        }

        public void StopIncarnating()
        {
            Incarnating?.ChangeIncarnation(null);
            Incarnating = null;
        }

        #endregion

        protected override bool InnerExecuteCommand(string commandLine, string command, string rawParameters, ICommandParameter[] parameters, bool forceOutOfGame)
        {
            // Execute command
            bool executedSuccessfully;
            if (forceOutOfGame || (Impersonating == null && Incarnating == null))
            {
                Log.Default.WriteLine(LogLevels.Debug, "[{0}] executing [{1}]", DisplayName, commandLine);
                executedSuccessfully = ExecuteCommand(command, rawParameters, parameters);
            }
            else if (Impersonating != null) // impersonating
            {
                Log.Default.WriteLine(LogLevels.Debug, "[{0}]|[{1}] executing [{2}]", DisplayName, Impersonating.DebugName, commandLine);
                executedSuccessfully = Impersonating.ExecuteCommand(command, rawParameters, parameters);
            }
            else if (Incarnating != null) // incarnating
            {
                Log.Default.WriteLine(LogLevels.Debug, "[{0}]|[{1}] executing [{2}]", DisplayName, Incarnating.DebugName, commandLine);
                executedSuccessfully = Incarnating.ExecuteCommand(command, rawParameters, parameters);
            }
            else
            {
                Wiznet.Wiznet($"[{DisplayName}] is neither out of game nor impersonating nor incarnating", WiznetFlags.Bugs, AdminLevels.Implementor);
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
