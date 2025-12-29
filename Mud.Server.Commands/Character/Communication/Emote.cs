using Mud.Domain;
using Mud.Server.Common.Attributes;
using Mud.Server.GameAction;
using Mud.Server.Guards.Attributes;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.Commands.Character.Communication;

[CharacterCommand("emote", "Communication"), MinPosition(Positions.Resting)]
[Syntax("[cmd] <message>")]
[Help(
@"[cmd] is used to express emotions or actions.  Besides [cmd], there are
several dozen built-in social commands, such as CACKLE, HUG, and THANK
(type socials or help socials for a listing).")]
public class Emote : CharacterGameAction
{
    private ICommandParser CommandParser { get; }

    public Emote(ICommandParser commandParser)
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
            return "Emote what?";

        What = CommandParser.JoinParameters(actionInput.Parameters);

        return null;
    }

    public override void Execute(IActionInput actionInput)
    {
        Actor.Act(ActOptions.ToAll, "{0:n} {1}", Actor, What);
    }
}
