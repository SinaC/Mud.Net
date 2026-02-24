using Mud.Server.Parser.Interfaces;
using Mud.Server.Domain.Attributes;
using Mud.Server.Common.Helpers;
using Mud.Server.Guards.Interfaces;
using Mud.Server.Guards.PlayerGuards;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Player;

namespace Mud.Server.Commands.Player.Communication;

[PlayerCommand("reply", "Communication")]
[Syntax("[cmd] <message>")]
[Help(
@"[cmd] sends a message to the last player who sent you a TELL.  [cmd] will work
even if you can't see the player, and without revealing their identity.  This
is handy for talking to invisible or switched immortal players.")]
public class Reply : TellGameActionBase
{
    protected override IGuard<IPlayer>[] Guards => [new RequiresAtLeastOneArgument { Message = "Reply what ?" }];

    private IParser Parser { get; }

    public Reply(IParser parser)
    {
        Parser = parser;
    }

    private string What { get; set; } = default!;

    public override string? CanExecute(IActionInput actionInput)
    {
        var baseGuards = base.CanExecute(actionInput);
        if (baseGuards != null)
            return baseGuards;

        if (Actor.LastTeller == null)
            return StringHelpers.CharacterNotFound;

        What = actionInput.RawParameters;
        return null;
    }

    public override void Execute(IActionInput actionInput)
    {
        InnerTell(Actor.LastTeller!, What);
    }
}
