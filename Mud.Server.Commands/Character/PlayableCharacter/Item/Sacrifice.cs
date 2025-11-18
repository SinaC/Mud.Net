using Mud.Common;
using Mud.Domain;
using Mud.Server.Common;
using Mud.Server.Common.Helpers;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Item;

namespace Mud.Server.Commands.Character.PlayableCharacter.Item;

[PlayableCharacterCommand("sacrifice", "Item", MinPosition = Positions.Standing, NotInCombat = true)]
[Alias("tap")]
[Alias("junk")]
[Syntax(
        "[cmd] all",
        "[cmd] <item>")]
[Help(
@"[cmd] offers an object to your god, who may reward you.
The nature of the reward depends upon the type of object.")]
public class Sacrifice : PlayableCharacterGameAction
{
    protected IItem[] What { get; set; } = default!;

    public override string? Guards(IActionInput actionInput)
    {
        var baseGuards = base.Guards(actionInput);
        if (baseGuards != null)
            return baseGuards;

        if (actionInput.Parameters.Length == 0 || StringCompareHelpers.StringEquals(Actor.Name, actionInput.Parameters[0].Value))
        {
            Actor.Act(ActOptions.ToRoom, "{0:N} offers {0:f} to Mota, who graciously declines.", Actor);
            return "Mota appreciates your offer and may accept it later.";
        }

        if (!actionInput.Parameters[0].IsAll)
        {
            var item = FindHelpers.FindByName(Actor.Room.Content.Where(x => Actor.CanSee(x)), actionInput.Parameters[0]);
            if (item == null)
                return StringHelpers.CantFindIt;
            What = [item];
        }
        else
            What = Actor.Room.Content.Where(x => Actor.CanSee(x)).ToArray();

        return null;
    }

    public override void Execute(IActionInput actionInput)
    {
        foreach (IItem item in What)
            Actor.SacrificeItem(item);
    }
}
