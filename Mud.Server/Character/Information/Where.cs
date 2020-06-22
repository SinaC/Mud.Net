using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mud.Common;
using Mud.Domain;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.Character.Information
{
    [CharacterCommand("where", "Information", MinPosition = Positions.Resting)]
    [Syntax(
        "[cmd]",
        "[cmd] <player name>")]
    public class Where : CharacterGameAction
    {
        public ICommandParameter Pattern { get; protected set; }

        public override string Guards(IActionInput actionInput)
        {
            string baseGuards = base.Guards(actionInput);
            if (baseGuards != null)
                return baseGuards;
            if (Actor.Room == null)
                return "You are nowhere";
            if (Actor.Room.RoomFlags.HasFlag(RoomFlags.NoWhere))
                Actor.Send("You don't recognize where you are.");

            Pattern = actionInput.Parameters.Length > 0
                ? actionInput.Parameters[0]
                : null;
            return null;
        }

        public override void Execute(IActionInput actionInput)
        {
            StringBuilder sb = new StringBuilder();
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
                                                                             && !x.Room.RoomFlags.HasFlag(RoomFlags.NoWhere)
                                                                             && !x.Room.IsPrivate
                                                                             && !x.CharacterFlags.HasFlag(CharacterFlags.Sneak)
                                                                             && !x.CharacterFlags.HasFlag(CharacterFlags.Hide)
                                                                             && Actor.CanSee(x)
                                                                             && StringCompareHelpers.StringListsStartsWith(x.Keywords, Pattern.Tokens));
                notFound = $"You didn't find any {Pattern.Value}.";
            }
            bool found = false;
            foreach (IPlayableCharacter playableCharacter in playableCharacters)
            {
                sb.AppendFormatLine("{0,-28} {1}", playableCharacter.DisplayName, playableCharacter.Room.DisplayName);
                found = true;
            }
            if (!found)
                sb.AppendLine(notFound);
            Actor.Send(sb);
        }
    }
}
