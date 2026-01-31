using Mud.Server.Common.Attributes;
using Mud.Server.Common.Helpers;
using Mud.Server.GameAction;
using Mud.Server.Guards.PlayerGuards;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Guards;
using Mud.Server.Interfaces.Player;

namespace Mud.Server.Commands.Player.Communication;

[PlayerCommand("tell", "Communication")]
[Syntax("[cmd] <player name> <message>")]
[Help(@"[cmd] sends a message to one player anywhere in the world.")]
public class Tell : TellGameActionBase
{
    protected override IGuard<IPlayer>[] Guards => [new RequiresAtLeastTwoArguments { Message = "Tell whom what ?" }];

    private ICommandParser CommandParser { get; }
    private IPlayerManager PlayerManager { get; }

    public Tell(ICommandParser commandParser, IPlayerManager playerManager)
    {
        CommandParser = commandParser;
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

        What = CommandParser.JoinParameters(actionInput.Parameters.Skip(1));

        return null;
    }

    public override void Execute(IActionInput actionInput)
    {
        InnerTell(Whom, What);
    }
}
