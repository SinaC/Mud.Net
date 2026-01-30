using Mud.Server.Common.Attributes;
using Mud.Server.Common.Helpers;
using Mud.Server.GameAction;
using Mud.Server.Guards.Attributes;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.Commands.Player.Communication;

[PlayerCommand("reply", "Communication"), NoArgumentGuard("Reply what ?")]
[Syntax("[cmd] <message>")]
[Help(
@"[cmd] sends a message to the last player who sent you a TELL.  [cmd] will work
even if you can't see the player, and without revealing their identity.  This
is handy for talking to invisible or switched immortal players.")]
public class Reply : TellGameActionBase
{
    private ICommandParser CommandParser { get; }

    public Reply(ICommandParser commandParser)
    {
        CommandParser = commandParser;
    }

    protected string What { get; set; } = default!;

    public override string? Guards(IActionInput actionInput)
    {
        var baseGuards = base.Guards(actionInput);
        if (baseGuards != null)
            return baseGuards;

        if (Actor.LastTeller == null)
            return StringHelpers.CharacterNotFound;

        What = CommandParser.JoinParameters(actionInput.Parameters);
        return null;
    }

    public override void Execute(IActionInput actionInput)
    {
        InnerTell(Actor.LastTeller!, What);
    }
}
