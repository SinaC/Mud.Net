using Mud.Common;
using Mud.Domain;
using Mud.Server.Common;
using Mud.Server.GameAction;
using Mud.Server.Helpers;
using Mud.Server.Input;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.Admin;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Player;
using System.Linq;

namespace Mud.Server.Admin.Administration
{
    [AdminCommand("promote", "Admin", Priority = 999, NoShortcut = true, MinLevel = AdminLevels.Supremacy, CannotBeImpersonated = true)]
    [Syntax("[cmd] <player name> <level>")]
    public class Promote : AdminGameAction
    {
        private IPlayerManager PlayerManager { get; }
        private IServerAdminCommand ServerAdminCommand { get; }

        public AdminLevels Level { get; protected set; }
        public IPlayer Player { get; protected set; }

        public Promote(IPlayerManager playerManager, IServerAdminCommand serverAdminCommand)
        {
            PlayerManager = playerManager;
            ServerAdminCommand = serverAdminCommand;
        }

        public override string Guards(IActionInput actionInput)
        {
            string baseGuards = base.Guards(actionInput);
            if (baseGuards != null)
                return baseGuards;

            if (actionInput.Parameters.Length < 2)
                return BuildCommandSyntax();

            // whom
            Player = FindHelpers.FindByName(PlayerManager.Players, actionInput.Parameters[0], true);
            if (Player == null)
                return StringHelpers.CharacterNotFound;
            if (Player == this)
                return "You cannot promote yourself.";
            if (Player is IAdmin)
                return $"{Player.DisplayName} is already Admin";

            // what
            AdminLevels level;
            if (!EnumHelpers.TryFindByName(actionInput.Parameters[1].Value, out level))
                return $"{actionInput.Parameters[1].Value} is not a valid admin levels. Values are : {string.Join(", ", EnumHelpers.GetValues<AdminLevels>().Select(x => x.ToString()))}";
            Level = level;

            return null;
        }

        public override void Execute(IActionInput actionInput)
        {
            ServerAdminCommand.Promote(Player, Level);
        }
    }
}
