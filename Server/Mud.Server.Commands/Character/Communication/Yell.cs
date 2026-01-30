using Mud.Domain;
using Mud.Server.Common.Attributes;
using Mud.Server.GameAction;
using Mud.Server.Guards.Attributes;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.Commands.Character.Communication;

[CharacterCommand("yell", "Communication"), MinPosition(Positions.Resting), NoArgumentGuard("Yell what ?")]
[Syntax("[cmd] <message>")]
[Help(@"[cmd] sends a message to all awake players within your area.")]
public class Yell : CharacterGameAction
{
    private ICommandParser CommandParser { get; }

    public Yell(ICommandParser commandParser)
    {
        CommandParser = commandParser;
    }

    protected string What { get; set; } = default!;

    public override string? Guards(IActionInput actionInput)
    {
        var baseGuards = base.Guards(actionInput);
        if (baseGuards != null)
            return baseGuards;

        What = CommandParser.JoinParameters(actionInput.Parameters);

        return null;
    }

    public override void Execute(IActionInput actionInput)
    {
        Actor.Act(Actor.Room.Area.Players.Where(x => x.Impersonating != null).Select(x => x.Impersonating!), "%G%{0:n} yell{0:v} '%x%{1}%G%'%x%", Actor, What);
    }
}
