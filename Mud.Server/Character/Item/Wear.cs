using Mud.Common;
using Mud.Domain;
using Mud.Server.Common;
using Mud.Server.GameAction;
using Mud.Server.Helpers;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Item;
using Mud.Server.Interfaces.Table;
using System.Linq;

namespace Mud.Server.Character.Item
{
    [CharacterCommand("wear", "Item", "Equipment", MinPosition = Positions.Resting)]
    [Syntax(
            "[cmd] <item>",
            "[cmd] all")]
    public class Wear : WearCharacterGameActionBase
    {
        public IItem[] What { get; protected set; }
        public bool Replace { get; protected set; }

        public Wear(ITableValues tableValues, IWiznet wiznet)
            : base(tableValues, wiznet)
        {
        }

        public override string Guards(IActionInput actionInput)
        {
            string baseGuards = base.Guards(actionInput);
            if (baseGuards != null)
                return baseGuards;

            if (actionInput.Parameters.Length == 0)
                return "Wear, wield, or hold what?";

            ICommandParameter whatParameter = actionInput.Parameters[0];
            // wear all, wear all.item
            if (actionInput.Parameters[0].IsAll)
            {
                // We have to clone list because it'll be modified when wearing an item
                What = !string.IsNullOrWhiteSpace(whatParameter.Value)
                    // get all.item
                    ? FindHelpers.FindAllByName(Actor.Inventory.Where(x => Actor.CanSee(x)), whatParameter).ToArray()
                    // get all
                    : Actor.Inventory.Where(x => Actor.CanSee(x)).ToArray();
                if (!What.Any())
                    return StringHelpers.ItemInventoryNotFound;
                return null;
            }
            // wear item
            IItem item = FindHelpers.FindByName(Actor.Inventory.Where(x => Actor.CanSee(x)), whatParameter);
            if (item == null)
                return StringHelpers.ItemInventoryNotFound;
            What = item.Yield().ToArray();
            return null;
        }

        public override void Execute(IActionInput actionInput)
        {
            bool recomputeNeeded = false;
            foreach (IItem item in What)
                recomputeNeeded |= WearItem(item, Replace);
            if (recomputeNeeded)
                Actor.Recompute();
        }
    }
}
