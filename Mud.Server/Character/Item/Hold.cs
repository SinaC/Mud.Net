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
    [CharacterCommand("hold", "Item", "Equipment", MinPosition = Positions.Resting)]
    [Syntax("[cmd] <item>")]
    public class Hold : WearCharacterGameActionBase
    {
        public IItem What { get; protected set; }

        public Hold(ITableValues tableValues, IWiznet wiznet)
            : base(tableValues, wiznet)
        {
        }

        public override string Guards(IActionInput actionInput)
        {
            string baseGuards = base.Guards(actionInput);
            if (baseGuards != null)
                return null;

            if (actionInput.Parameters.Length == 0)
                return "Wield what?";
            What = FindHelpers.FindByName(Actor.Inventory.Where(x => Actor.CanSee(x)), actionInput.Parameters[0]);
            if (What == null)
                return StringHelpers.ItemInventoryNotFound;
            if (What.WearLocation != WearLocations.Hold && What.WearLocation != WearLocations.Shield && What.WearLocation != WearLocations.Light)
                return "It cannot be hold.";
            return null;
        }

        public override void Execute(IActionInput actionInput)
        {
            WearItem(What, true);
            Actor.Recompute();
        }
    }
}
