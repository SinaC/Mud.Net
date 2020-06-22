using Mud.Common;
using Mud.Domain;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;
using System.Linq;

namespace Mud.Server.Character.PlayableCharacter.Item
{
    [PlayableCharacterCommand("split", "Item", MinPosition = Positions.Standing, Priority = 600)]
    [Syntax("[cmd] <silver amount> <gold amount>")]
    public class Split : PlayableCharacterGameAction
    {
        public long Silver { get; protected set; }
        public long Gold { get; protected set; }

        public override string Guards(IActionInput actionInput)
        {
            string baseGuards = base.Guards(actionInput);
            if (baseGuards != null)
                return baseGuards;

            if (actionInput.Parameters.Length == 0)
                return "Split how much?";

            if (actionInput.Parameters.Length > 0)
                Silver = actionInput.Parameters[0].AsLong;
            if (actionInput.Parameters.Length > 1)
                Gold = actionInput.Parameters[1].AsLong;

            if (Silver < 0 || Gold < 0)
                return "Your group wouldn't like that.";

            if (Silver == 0 && Gold == 0)
                return "You hand out zero coins, but no one notices.";

            if (Actor.SilverCoins < Silver || Actor.GoldCoins < Gold)
                return "You don't have that much to split.";

            IPlayableCharacter[] members = (Actor.Group?.Members ?? Actor.Yield()).ToArray();
            if (members.Length < 2)
                return "Just keep it all.";

            return null;
        }

        public override void Execute(IActionInput actionInput)
        {
            Actor.SplitMoney(Silver, Gold);
        }
    }
}
