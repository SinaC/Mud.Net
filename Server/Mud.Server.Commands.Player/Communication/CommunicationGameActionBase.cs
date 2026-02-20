using Mud.Server.Parser.Interfaces;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Player;

namespace Mud.Server.Commands.Player.Communication;

public abstract class CommunicationGameActionBase : PlayerGameAction
{
    private IParser Parser { get; }
    private IPlayerManager PlayerManager { get; }

    protected CommunicationGameActionBase(IParser parser, IPlayerManager playerManager)
    {
        Parser = parser;
        PlayerManager = playerManager;
    }

    protected abstract string ActorSendPattern { get; }
    protected abstract string OtherSendPattern { get; }

    protected string What { get; set; } = default!;

    public override string? CanExecute(IActionInput actionInput)
    {
        var baseGuards = base.CanExecute(actionInput);
        if (baseGuards != null)
            return baseGuards;

        What = actionInput.RawParameters;
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
