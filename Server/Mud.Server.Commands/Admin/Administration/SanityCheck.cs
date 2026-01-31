using Mud.Server.Common.Helpers;
using Mud.Server.GameAction;
using Mud.Server.Guards.AdminGuards;
using Mud.Server.Interfaces.Admin;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Guards;
using Mud.Server.Interfaces.Player;
using System.Text;

namespace Mud.Server.Commands.Admin.Administration;

[AdminCommand("sanitycheck", "Admin")]
[Syntax("[cmd] <character>")]
public class SanityCheck : AdminGameAction
{
    protected override IGuard<IAdmin>[] Guards => [new RequiresAtLeastOneArgument()];

    private IPlayerManager PlayerManager { get; }

    public SanityCheck(IPlayerManager playerManager)
    {
        PlayerManager = playerManager;
    }

    private IPlayer Whom { get; set; } = default!;

    public override string? CanExecute(IActionInput actionInput)
    {
        var baseGuards = base.CanExecute(actionInput);
        if (baseGuards != null)
            return baseGuards;

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
