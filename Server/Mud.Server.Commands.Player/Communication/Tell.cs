using Mud.Server.Parser.Interfaces;
using Mud.Server.Domain.Attributes;
using Mud.Server.Common.Helpers;
using Mud.Server.Guards.Interfaces;
using Mud.Server.Guards.PlayerGuards;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Player;

namespace Mud.Server.Commands.Player.Communication;

[PlayerCommand("tell", "Communication")]
[Syntax("[cmd] <player name> <message>")]
[Help(@"[cmd] sends a message to one player anywhere in the world.")]
public class Tell : TellGameActionBase
{
    protected override IGuard<IPlayer>[] Guards => [new RequiresAtLeastTwoArguments { Message = "Tell whom what ?" }];

    private IParser Parser { get; }
    private IPlayerManager PlayerManager { get; }

    public Tell(IParser parser, IPlayerManager playerManager)
    {
        Parser = parser;
        PlayerManager = playerManager;
    }

    private IPlayer Whom { get; set; } = default!;
    private string What { get; set; } = default!;

    public override string? CanExecute(IActionInput actionInput)
    {
        var baseGuards = base.CanExecute(actionInput);
        if (baseGuards != null)
            return baseGuards;

        Whom = PlayerManager.GetPlayer(actionInput.Parameters[0], true)!;
        if (Whom == null)
            return StringHelpers.CharacterNotFound;

        What = Parser.JoinParameters(actionInput.Parameters.Skip(1));

        return null;
    }

    public override void Execute(IActionInput actionInput)
    {
        InnerTell(Whom, What);
    }
}
