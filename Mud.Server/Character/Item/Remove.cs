using Mud.Domain;
using Mud.Server.Common;
using Mud.Server.GameAction;
using Mud.Server.Helpers;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;
using System.Linq;

namespace Mud.Server.Character.Item
{
    [CharacterCommand("remove", "Item", "Equipment", MinPosition = Positions.Resting)]
    [Syntax("[cmd] <item>")]
    public class Remove : CharacterGameAction
    {
        public IEquippedItem What { get; protected set; }

        public override string Guards(IActionInput actionInput)
        {
            string baseGuards = base.Guards(actionInput);
            if (baseGuards != null)
                return baseGuards;

            if (actionInput.Parameters.Length == 0)
                return "Remove what?";
            //
            What = FindHelpers.FindByName(Actor.Equipments.Where(x => x.Item != null && Actor.CanSee(x.Item)), x => x.Item, actionInput.Parameters[0]);
            if (What?.Item == null)
                return StringHelpers.ItemInventoryNotFound;
            return null;
        }

        public override void Execute(IActionInput actionInput)
        {
            bool removed = RemoveItem(What);
            if (removed)
                Actor.Recompute();

        }

        protected virtual bool RemoveItem(IEquippedItem equipmentSlot)
        {
            //
            if (equipmentSlot.Item.ItemFlags.IsSet("NoRemove"))
            {
                Actor.Act(ActOptions.ToCharacter, "You cannot remove {0}.", equipmentSlot.Item);
                return false;
            }

            // TODO: check weight + item count
            Actor.Act(ActOptions.ToAll, "{0:N} stop{0:v} using {1}.", Actor, equipmentSlot.Item);
            equipmentSlot.Item.ChangeContainer(Actor); // add in inventory
            equipmentSlot.Item.ChangeEquippedBy(null, false); // clear equipped by
            equipmentSlot.Item = null; // unequip TODO: remove it's already done in Unequip
            // no need to recompute, because it's being done by caller
            return true;
        }
    }
}
