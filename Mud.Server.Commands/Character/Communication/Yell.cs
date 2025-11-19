using Mud.Domain;
using Mud.Server.Common;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.Commands.Character.Communication;

[CharacterCommand("yell", "Communication", MinPosition = Positions.Resting)]
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

        if (actionInput.Parameters.Length == 0)
            return "Yell what?";

        What = CommandParser.JoinParameters(actionInput.Parameters);

        return null;
    }

    public override void Execute(IActionInput actionInput)
    {
        Actor.Act(Actor.Room.Area.Players.Where(x => x.Impersonating != null).Select(x => x.Impersonating!), "%G%{0:n} yell{0:v} '%x%{1}%G%'%x%", Actor, What);
    }
}
