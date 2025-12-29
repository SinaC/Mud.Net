using Mud.Common;
using Mud.Domain;
using Mud.Server.Common.Attributes;
using Mud.Server.GameAction;
using Mud.Server.Guards.Attributes;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;
using System.Text;

namespace Mud.Server.Commands.Character.Information;

[CharacterCommand("where", "Information"), MinPosition(Positions.Resting)]
[Syntax(
    "[cmd]",
    "[cmd] <player name>")]
[Help(
@"[cmd] without an argument tells you the location of visible players in the same
area as you are.

[cmd] with an argument tells you the location of one character with that name
within your area, including monsters.")]
public class Where : CharacterGameAction
{
    protected ICommandParameter Pattern { get; set; } = default!;

    public override string? Guards(IActionInput actionInput)
    {
        var baseGuards = base.Guards(actionInput);
        if (baseGuards != null)
            return baseGuards;

        if (Actor.Room == null)
            return "You are nowhere";

        if (Actor.Room.RoomFlags.IsSet("NoWhere"))
            return "You don't recognize where you are.";

        Pattern = actionInput.Parameters.Length > 0
            ? actionInput.Parameters[0]
            : null!;
        return null;
    }

    public override void Execute(IActionInput actionInput)
    {
        StringBuilder sb = new();
        sb.AppendFormatLine($"[{Actor.Room.Area.DisplayName}].");
        //
        IEnumerable<IPlayableCharacter> playableCharacters;
        string notFound;
        if (Pattern == null)
        {
            sb.AppendLine("Players near you:");
            playableCharacters = Actor.Room.Area.PlayableCharacters.Where(x => Actor.CanSee(x));
            notFound = "None";
        }
        else
        {
            playableCharacters = Actor.Room.Area.PlayableCharacters.Where(x => x.Room != null
                                                                         && !x.Room.RoomFlags.IsSet("NoWhere")
                                                                         && !x.Room.IsPrivate
                                                                         && !x.CharacterFlags.IsSet("Sneak")
                                                                         && !x.CharacterFlags.IsSet("Hide")
                                                                         && Actor.CanSee(x)
                                                                         && StringCompareHelpers.AllStringsStartsWith(x.Keywords, Pattern.Tokens));
            notFound = $"You didn't find any {Pattern.Value}.";
        }
        bool found = false;
        foreach (var playableCharacter in playableCharacters)
        {
            sb.AppendFormatLine("{0,-28} {1}", playableCharacter.DisplayName, playableCharacter.Room.DisplayName);
            found = true;
        }
        if (!found)
            sb.AppendLine(notFound);
        Actor.Send(sb);
    }
}
