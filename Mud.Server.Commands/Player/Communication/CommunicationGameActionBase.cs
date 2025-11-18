using Mud.Server.GameAction;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Player;

namespace Mud.Server.Commands.Player.Communication;

public abstract class CommunicationGameActionBase : PlayerGameAction
{
    private IPlayerManager PlayerManager { get; }

    protected abstract string NoParamMessage { get; }
    protected abstract string ActorSendPattern { get; }
    protected abstract string OtherSendPattern { get; }

    protected CommunicationGameActionBase(IPlayerManager playerManager)
    {
        PlayerManager = playerManager;
    }

    protected string What { get; set; } = default!;

    public override string? Guards(IActionInput actionInput)
    {
        var baseGuards = base.Guards(actionInput);
        if (baseGuards != null)
            return baseGuards;

        if (actionInput.Parameters.Length == 0)
            return NoParamMessage;

        What = CommandHelpers.JoinParameters(actionInput.Parameters);
        return null;
    }

    public override void Execute(IActionInput actionInput)
    {
        Actor.Send(ActorSendPattern, What);

        string other = string.Format(OtherSendPattern, Actor.DisplayName, What);
        foreach (IPlayer player in PlayerManager.Players.Where(x => x != Actor))
            player.Send(other);
    }
}
