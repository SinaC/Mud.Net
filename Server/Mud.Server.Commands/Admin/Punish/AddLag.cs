using Mud.Flags;
using Mud.Server.Common.Attributes;
using Mud.Server.Common.Helpers;
using Mud.Server.GameAction;
using Mud.Server.Guards.AdminGuards;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.Admin;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Guards;
using Mud.Server.Interfaces.Player;

namespace Mud.Server.Commands.Admin.Punish;

[AdminCommand("addlag", "Punish")]
[Syntax("[cmd] <player name> <tick>")]
[Help(@"This command add lag to a player. Be careful when using this command!")]
public class AddLag : AdminGameAction
{
    protected override IGuard<IAdmin>[] Guards => [new RequiresAtLeastTwoArguments()];

    private IPlayerManager PlayerManager { get; }
    private IWiznet Wiznet { get; }

    public AddLag(IPlayerManager playerManager, IWiznet wiznet)
    {
        PlayerManager = playerManager;
        Wiznet = wiznet;
    }

    private IPlayer Whom { get; set; } = default!;
    private int Modifier { get; set; }

    public override string? CanExecute(IActionInput actionInput)
    {
        var baseGuards = base.CanExecute(actionInput);
        if (baseGuards != null)
            return baseGuards;

        if (!actionInput.Parameters[1].IsNumber)
            return BuildCommandSyntax();

        Modifier = actionInput.Parameters[1].AsNumber;
        if (Modifier <= 0)
            return "That makes a LOT of sense.";
        if (Modifier > 100)
            return "There's a limit to cruel and unusual punishment.";

        Whom = PlayerManager.GetPlayer(actionInput.Parameters[0], true)!;
        if (Whom == null)
            return StringHelpers.CharacterNotFound;
        return null;
    }

    public override void Execute(IActionInput actionInput)
    {
        Wiznet.Log($"{Actor.DisplayName} adds lag {Whom.DisplayName}.", new WiznetFlags("Punish"));

        Actor.Send("Adding lag now.");
        Whom.SetLag(Modifier);
    }
}
