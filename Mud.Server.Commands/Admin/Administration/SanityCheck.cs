using Mud.Server.Common;
using Mud.Server.Common.Helpers;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Player;
using System.Text;

namespace Mud.Server.Commands.Admin.Administration;

[AdminCommand("sanitycheck", "Admin")]
[Syntax("[cmd] <character>")]
public class SanityCheck : AdminGameAction
{
    private IPlayerManager PlayerManager { get; }

    public SanityCheck(IPlayerManager playerManager)
    {
        PlayerManager = playerManager;
    }

    protected IPlayer Whom { get; set; } = default!;

    public override string? Guards(IActionInput actionInput)
    {
        var baseGuards = base.Guards(actionInput);
        if (baseGuards != null)
            return baseGuards;

        if (actionInput.Parameters.Length == 0)
            return BuildCommandSyntax();

        Whom = FindHelpers.FindByName(PlayerManager.Players, actionInput.Parameters[0])!;
        if (Whom == null)
            return StringHelpers.CharacterNotFound;

        return null;
    }

    public override void Execute(IActionInput actionInput)
    {
        StringBuilder info = Whom.PerformSanityChecks();
        Actor.Page(info);
    }
}
