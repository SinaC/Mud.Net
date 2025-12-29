using Mud.Domain;
using Mud.Server.Common.Attributes;
using Mud.Server.Common.Helpers;
using Mud.Server.GameAction;
using Mud.Server.Guards.Attributes;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.Commands.Character.Communication;

[CharacterCommand("whisper", "Communication"), MinPosition(Positions.Standing), NotInCombat]
[Syntax("[cmd] <character> <message>")]
[Help(
@"[cmd] sends a message to a player/mob within the same room as you. Other 
players/mobs in the room can't hear the message.")]
public class Whisper : CharacterGameAction
{
    private ICommandParser CommandParser { get; }

    public Whisper(ICommandParser commandParser)
    {
        CommandParser = commandParser;
    }

    protected ICharacter Whom { get; set; } = default!;
    protected string What { get; set; } = default!;

    public override string? Guards(IActionInput actionInput)
    {
        var baseGuards = base.Guards(actionInput);
        if (baseGuards != null)
            return baseGuards;

        if (actionInput.Parameters.Length <= 1)
            return "Whisper whom what?";
            
        Whom = FindHelpers.FindByName(Actor.Room.People, actionInput.Parameters[0])!;
        if (Whom == null)
            return StringHelpers.CharacterNotFound;

        What = CommandParser.JoinParameters(actionInput.Parameters.Skip(1));
        return null;
    }

    public override void Execute(IActionInput actionInput)
    {
        Actor.Act(ActOptions.ToCharacter, "You whisper '{0}' to {1:n}.", What, Whom);
        if (Actor != Whom)
            Whom.Act(ActOptions.ToCharacter, "{0:n} whispers you '{1}'.", Actor, What); // TODO: when used on ourself (player is pouet), pouet whispers you 'blabla'
        Actor.ActToNotVictim(Whom, "{0:n} whispers something to {1:n}.", Actor, Whom);
        // ActOptions.ToAll cannot be used because 'something' is sent except for 'this' and 'whom'
    }
}
