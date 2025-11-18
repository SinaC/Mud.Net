using Mud.Common;
using Mud.Domain;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.Commands.Character.PlayableCharacter.Item;

[PlayableCharacterCommand("split", "Item", MinPosition = Positions.Standing, Priority = 600, NotInCombat = true)]
[Syntax("[cmd] <silver amount> <gold amount>")]
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
