using Mud.Domain;
using Mud.Server.CommandParser.Interfaces;
using Mud.Server.Domain.Attributes;
using Mud.Server.GameAction;
using Mud.Server.Guards.CharacterGuards;
using Mud.Server.Guards.Interfaces;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.Commands.Character.Communication;

[CharacterCommand("emote", "Communication")]
[Syntax("[cmd] <message>")]
[Help(
@"[cmd] is used to express emotions or actions.  Besides [cmd], there are
several dozen built-in social commands, such as CACKLE, HUG, and THANK
(type socials or help socials for a listing).")]
public class Emote : CharacterGameAction
{
    protected override IGuard<ICharacter>[] Guards => [new RequiresMinPosition(Positions.Resting), new RequiresAtLeastOneArgument { Message = "Emote what ?" }];

    private ICommandParser CommandParser { get; }

    public Emote(ICommandParser commandParser)
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
        Actor.Act(ActOptions.ToAll, "{0:n} {1}", Actor, What);
    }
}
