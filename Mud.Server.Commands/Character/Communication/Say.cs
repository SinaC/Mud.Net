using Mud.Domain;
using Mud.Server.Common.Attributes;
using Mud.Server.GameAction;
using Mud.Server.Guards.Attributes;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.Commands.Character.Communication;

[CharacterCommand("say", "Communication"), MinPosition(Positions.Resting)]
[Alias("'")]
[Syntax("[cmd] <message>")]
[Help(@"[cmd] sends a message to all awake players/mobs in your room (In Character channel).")]
public class Say : CharacterGameAction
{
    private ICommandParser CommandParser { get; }

    public Say(ICommandParser commandParser)
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
            return "Say what?";

        What = CommandParser.JoinParameters(actionInput.Parameters);

        return null;
    }

    public override void Execute(IActionInput actionInput)
    {
        Actor.Act(ActOptions.ToAll, "%g%{0:N} say{0:v} '%x%{1}%g%'%x%", Actor, What);
    }
}
