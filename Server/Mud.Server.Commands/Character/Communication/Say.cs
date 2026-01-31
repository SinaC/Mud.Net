using Mud.Domain;
using Mud.Server.Common.Attributes;
using Mud.Server.GameAction;
using Mud.Server.Guards.CharacterGuards;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Guards;

namespace Mud.Server.Commands.Character.Communication;

[CharacterCommand("say", "Communication")]
[Alias("'")]
[Syntax("[cmd] <message>")]
[Help(@"[cmd] sends a message to all awake players/mobs in your room (In Character channel).")]
public class Say : CharacterGameAction
{
    protected override IGuard<ICharacter>[] Guards => [new RequiresMinPosition(Positions.Resting), new RequiresAtLeastOneArgument { Message = "Say what ?" }];

    private ICommandParser CommandParser { get; }

    public Say(ICommandParser commandParser)
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
        Actor.Act(ActOptions.ToAll, "%g%{0:N} say{0:v} '%x%{1}%g%'%x%", Actor, What);
    }
}
