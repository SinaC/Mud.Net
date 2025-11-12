using Mud.Domain;
using Mud.Server.Common;
using Mud.Server.GameAction;
using Mud.Server.Helpers;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Item;
using System.Linq;
using Mud.Server.Interfaces;

namespace Mud.Server.Character.Item
{
    [CharacterCommand("wield", "Item", "Equipment", MinPosition = Positions.Resting)]
    [Syntax("[cmd] <weapon>")]
    public class Wield : WearCharacterGameActionBase
    {
        public IItem What { get; protected set; }

        public Wield(IWiznet wiznet)
            : base(wiznet)
        {
        }

        public override string Guards(IActionInput actionInput)
        {
            string baseGuards = base.Guards(actionInput);
            if (baseGuards != null)
                return baseGuards;

            if (actionInput.Parameters.Length == 0)
                return "Wield what?";
            What = FindHelpers.FindByName(Actor.Inventory.Where(x => Actor.CanSee(x)), actionInput.Parameters[0]);
            if (What == null)
                return StringHelpers.ItemInventoryNotFound;
            if (What.WearLocation == WearLocations.None)
                return "It cannot be wielded.";
            if (!(What is IItemWeapon))
                return "Only weapons can be wielded.";
            return null;
        }

        public override void Execute(IActionInput actionInput)
        {
            WearItem(What, true);
            Actor.Recompute();
        }
    }
}
