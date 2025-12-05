using Mud.Common;
using Mud.Domain;
using Mud.Server.Common.Helpers;
using Mud.Server.GameAction;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.Admin;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Player;

namespace Mud.Server.Commands.Admin.Administration;

[AdminCommand("promote", "Admin", Priority = 999, NoShortcut = true, MinLevel = AdminLevels.Supremacy, CannotBeImpersonated = true)]
[Syntax("[cmd] <player name> <level>")]
public class Promote : AdminGameAction
{
    private IPlayerManager PlayerManager { get; }
    private IServerAdminCommand ServerAdminCommand { get; }

    public Promote(IPlayerManager playerManager, IServerAdminCommand serverAdminCommand)
    {
        PlayerManager = playerManager;
        ServerAdminCommand = serverAdminCommand;
    }

    protected AdminLevels Level { get; set; }
    protected IPlayer Whom { get; set; } = default!;

    public override string? Guards(IActionInput actionInput)
    {
        var baseGuards = base.Guards(actionInput);
        if (baseGuards != null)
            return baseGuards;

        if (actionInput.Parameters.Length < 2)
            return BuildCommandSyntax();

        // whom
        Whom = FindHelpers.FindByName(PlayerManager.Players, actionInput.Parameters[0], true)!;
        if (Whom == null)
            return StringHelpers.CharacterNotFound;
        if (Whom == Actor)
            return "You cannot promote yourself.";
        if (Whom is IAdmin)
            return $"{Whom.DisplayName} is already Admin";

        // what
        if (!EnumHelpers.TryFindByName(actionInput.Parameters[1].Value, out AdminLevels level))
            return $"{actionInput.Parameters[1].Value} is not a valid admin levels. Values are : {string.Join(", ", Enum.GetNames<AdminLevels>())}";
        Level = level;

        return null;
    }

    public override void Execute(IActionInput actionInput)
    {
        ServerAdminCommand.Promote(Whom, Level);
    }
}
