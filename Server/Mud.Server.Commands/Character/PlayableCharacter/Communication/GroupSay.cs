using Mud.Server.GameAction;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.Commands.Character.PlayableCharacter.Communication;

[PlayableCharacterCommand("groupsay", "Group", "Communication", Priority = 1000)]
[Alias("gtell")]
[Alias("gsay")]
[Syntax("[cmd] <message>")]
public class GroupSay : PlayableCharacterGameAction
{
    private ICommandParser CommandParser { get; }

    public GroupSay(ICommandParser commandParser)
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
            return "Say your group what?";

        What = CommandParser.JoinParameters(actionInput.Parameters);
        return null;
    }

    public override void Execute(IActionInput actionInput)
    {
        Actor.Act(ActOptions.ToGroup, "%g%{0:N} say{0:v} the group '%x%{1}%g%'%x%", Actor, What);
    }
}
