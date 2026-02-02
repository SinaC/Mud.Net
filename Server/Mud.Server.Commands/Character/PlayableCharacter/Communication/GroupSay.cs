using Mud.Server.CommandParser.Interfaces;
using Mud.Server.Domain.Attributes;
using Mud.Server.GameAction;
using Mud.Server.Guards.Interfaces;
using Mud.Server.Guards.PlayableCharacterGuards;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.Commands.Character.PlayableCharacter.Communication;

[PlayableCharacterCommand("groupsay", "Group", "Communication", Priority = 1000)]
[Alias("gtell")]
[Alias("gsay")]
[Syntax("[cmd] <message>")]
public class GroupSay : PlayableCharacterGameAction
{
    protected override IGuard<IPlayableCharacter>[] Guards => [new RequiresAtLeastOneArgument { Message = "Say your group what ?" }];

    private ICommandParser CommandParser { get; }

    public GroupSay(ICommandParser commandParser)
    {
        CommandParser = commandParser;
    }

    private string What { get; set; } = default!;

    public override string? CanExecute(IActionInput actionInput)
    {
        var baseGuards = base.CanExecute(actionInput);
        if (baseGuards != null)
            return baseGuards;

        What = CommandParser.JoinParameters(actionInput.Parameters);
        return null;
    }

    public override void Execute(IActionInput actionInput)
    {
        Actor.Act(ActOptions.ToGroup, "%g%{0:N} say{0:v} the group '%x%{1}%g%'%x%", Actor, What);
    }
}
