using Mud.Common;
using Mud.Domain;
using Mud.Server.Common;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.Commands.Character.PlayableCharacter.Item;

[PlayableCharacterCommand("split", "Item", MinPosition = Positions.Standing, Priority = 600, NotInCombat = true)]
[Syntax("[cmd] <silver amount> <gold amount>")]
[Help(
@"[cmd] splits some coins  between you and all the members of your
group who are in the same room as you.  It's customary to SPLIT
the loot after a kill.  The first argument is the amount of silver
the split (0 is acceptable), and the second gold (optional).
Examples:
split 30 	--> split 30 silver
split 20 50	--> split 20 silver, 50 gold
split  0 10	--> split 10 gold")]
public class Split : PlayableCharacterGameAction
{
    protected long Silver { get; set; }
    protected long Gold { get; set; }

    public override string? Guards(IActionInput actionInput)
    {
        var baseGuards = base.Guards(actionInput);
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

        var members = (Actor.Group?.Members ?? Actor.Yield()).ToArray();
        if (members.Length < 2)
            return "Just keep it all.";

        return null;
    }

    public override void Execute(IActionInput actionInput)
    {
        Actor.SplitMoney(Silver, Gold);
    }
}
