using Mud.Server.Common.Helpers;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Player;

namespace Mud.Server.Commands.Player.Communication;

[PlayerCommand("tell", "Communication")]
[Syntax("[cmd] <player name> <message>")]
public class Tell : TellGameActionBase
{
    private IPlayerManager PlayerManager { get; }

    public Tell(IPlayerManager playerManager)
    {
        PlayerManager = playerManager;
    }

    protected IPlayer Whom { get; set; } = default!;
    protected string What { get; set; } = default!;

    public override string? Guards(IActionInput actionInput)
    {
        var baseGuards = base.Guards(actionInput);
        if (baseGuards != null)
            return baseGuards;

        if (actionInput.Parameters.Length < 2)
            return "Tell whom what ?";

        Whom = PlayerManager.GetPlayer(actionInput.Parameters[0], true)!;
        if (Whom == null)
            return StringHelpers.CharacterNotFound;

        What = CommandHelpers.JoinParameters(actionInput.Parameters.Skip(1));

        return null;
    }

    public override void Execute(IActionInput actionInput)
    {
        InnerTell(Whom, What);
    }
}
