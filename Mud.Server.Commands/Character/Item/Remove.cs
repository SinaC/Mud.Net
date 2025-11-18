using Mud.Domain;
using Mud.Server.Common;
using Mud.Server.Common.Helpers;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.Commands.Character.Item;

[CharacterCommand("remove", "Item", "Equipment", MinPosition = Positions.Resting)]
[Syntax("[cmd] <item>")]
[Help(@"[cmd] will take any object from your equipment and put it back into your\r\ninventory.")]
public class Remove : CharacterGameAction
{
    protected IEquippedItem What { get; set; } = default!;

    public override string? Guards(IActionInput actionInput)
    {
        var baseGuards = base.Guards(actionInput);
        if (baseGuards != null)
            return baseGuards;

        if (actionInput.Parameters.Length == 0)
            return "Remove what?";
        //
        What = FindHelpers.FindByName(Actor.Equipments.Where(x => x.Item != null && Actor.CanSee(x.Item)), x => x.Item!, actionInput.Parameters[0])!;
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
        if (equipmentSlot.Item == null)
        {
            Actor.Act(ActOptions.ToCharacter, "You are not using {0}.", equipmentSlot.Slot);
            return false;
        }

        if (equipmentSlot.Item.ItemFlags.IsSet("NoRemove"))
        {
            Actor.Act(ActOptions.ToCharacter, "You cannot remove {0}.", equipmentSlot.Item);
            return false;
        }

        // TODO: check weight + item count
        Actor.Act(ActOptions.ToAll, "{0:N} stop{0:v} using {1}.", Actor, equipmentSlot.Item);
        equipmentSlot.Item.ChangeContainer(Actor); // add in inventory
        equipmentSlot.Item.ChangeEquippedBy(null!, false); // clear equipped by
        equipmentSlot.Item = null; // unequip TODO: remove it's already done in Unequip
        // no need to recompute, because it's being done by caller
        return true;
    }
}
