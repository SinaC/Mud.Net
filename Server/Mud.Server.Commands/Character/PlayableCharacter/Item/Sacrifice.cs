using Mud.Common;
using Mud.Domain;
using Mud.Server.Common.Attributes;
using Mud.Server.Common.Helpers;
using Mud.Server.GameAction;
using Mud.Server.Guards.PlayableCharacterGuards;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Guards;
using Mud.Server.Interfaces.Item;

namespace Mud.Server.Commands.Character.PlayableCharacter.Item;

[PlayableCharacterCommand("sacrifice", "Item")]
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
    protected override IGuard<IPlayableCharacter>[] Guards => [new RequiresMinPosition(Positions.Standing), new CannotBeInCombat()];

    private IItem[] What { get; set; } = default!;

    public override string? CanExecute(IActionInput actionInput)
    {
        var baseGuards = base.CanExecute(actionInput);
        if (baseGuards != null)
            return baseGuards;

        if (actionInput.Parameters.Length == 0 || StringCompareHelpers.StringEquals(Actor.Name, actionInput.Parameters[0].Value))
        {
            Actor.Act(ActOptions.ToRoom, "{0:N} offers {0:f} to Mota, who graciously declines.", Actor);
            return "Mota appreciates your offer and may accept it later.";
        }

        if (!actionInput.Parameters[0].IsAll)
        {
            var item = FindHelpers.FindByName(Actor.Room.Content.Where(Actor.CanSee), actionInput.Parameters[0]);
            if (item == null)
                return StringHelpers.CantFindIt;
            What = [item];
        }
        else
            What = [.. Actor.Room.Content.Where(x => Actor.CanSee(x))];

        return null;
    }

    public override void Execute(IActionInput actionInput)
    {
        foreach (IItem item in What)
            Actor.SacrificeItem(item);
    }
}
