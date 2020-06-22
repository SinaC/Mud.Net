using Mud.Common;
using Mud.Domain;
using Mud.Server.Common;
using Mud.Server.GameAction;
using Mud.Server.Helpers;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Item;
using System.Linq;

namespace Mud.Server.Character.PlayableCharacter.Item
{
    [PlayableCharacterCommand("sacrifice", "Item", MinPosition = Positions.Standing)]
    [Alias("tap")]
    [Alias("junk")]
    [Syntax(
            "[cmd] all",
            "[cmd] <item>")]
    public class Sacrifice : PlayableCharacterGameAction
    {
        private IItemManager ItemManager { get; }

        public IItem[] What { get; protected set; }

        public Sacrifice(IItemManager itemManager)
        {
            ItemManager = itemManager;
        }

        public override string Guards(IActionInput actionInput)
        {
            string baseGuards = base.Guards(actionInput);
            if (baseGuards != null)
                return baseGuards;

            if (actionInput.Parameters.Length == 0 || StringCompareHelpers.StringEquals(Actor.Name, actionInput.Parameters[0].Value))
            {
                Actor.Act(ActOptions.ToRoom, "{0:N} offers {0:f} to Mota, who graciously declines.", this);
                return "Mota appreciates your offer and may accept it later.";
            }

            if (!actionInput.Parameters[0].IsAll)
            {
                IItem item = FindHelpers.FindByName(Actor.Room.Content.Where(x => Actor.CanSee(x)), actionInput.Parameters[0]);
                if (item == null)
                    return StringHelpers.CantFindIt;
                What = item.Yield().ToArray();
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
}
