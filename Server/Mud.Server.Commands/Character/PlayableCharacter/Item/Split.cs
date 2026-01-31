using Mud.Common;
using Mud.Domain;
using Mud.Server.Common.Attributes;
using Mud.Server.GameAction;
using Mud.Server.Guards.PlayableCharacterGuards;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Guards;

namespace Mud.Server.Commands.Character.PlayableCharacter.Item;

[PlayableCharacterCommand("split", "Item", Priority = 600)]
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
    protected override IGuard<IPlayableCharacter>[] Guards => [new RequiresMinPosition(Positions.Standing), new CannotBeInCombat(), new RequiresAtLeastOneArgument { Message = "Split how much ?" }];

    private long Silver { get; set; }
    private long Gold { get; set; }

    public override string? CanExecute(IActionInput actionInput)
    {
        var baseGuards = base.CanExecute(actionInput);
        if (baseGuards != null)
            return baseGuards;

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
