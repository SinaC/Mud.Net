using Mud.Domain;
using Mud.Server.Common.Attributes;
using Mud.Server.GameAction;
using Mud.Server.Guards.CharacterGuards;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Guards;

namespace Mud.Server.Commands.Character.Communication;

[CharacterCommand("yell", "Communication")]
[Syntax("[cmd] <message>")]
[Help(@"[cmd] sends a message to all awake players within your area.")]
public class Yell : CharacterGameAction
{
    protected override IGuard<ICharacter>[] Guards => [new RequiresMinPosition(Positions.Resting), new RequiresAtLeastOneArgument { Message = "Yell what ?" }];

    private ICommandParser CommandParser { get; }

    public Yell(ICommandParser commandParser)
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
        Actor.Act(Actor.Room.Area.Players.Where(x => x.Impersonating != null).Select(x => x.Impersonating!), "%G%{0:n} yell{0:v} '%x%{1}%G%'%x%", Actor, What);
    }
}
