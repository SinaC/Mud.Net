using Mud.Domain;
using Mud.Server.GameAction;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Item;

namespace Mud.Server.Commands.Character.Item;

public abstract class WearCharacterGameActionBase : CharacterGameAction
{
    private IWiznet Wiznet { get; }

    protected WearCharacterGameActionBase(IWiznet wiznet)
    {
        Wiznet = wiznet;
    }

    protected virtual bool WearItem(IItem item, bool replace) // equivalent to wear_obj in act_obj.C:1467
    {
        // check level
        if (item.Level > Actor.Level)
        {
            Actor.Send("You must be level {0} to use this object.", item.Level);
            Actor.Act(ActOptions.ToRoom, "{0} tries to use {1}, but is too inexperienced.", Actor, item);
            return false;
        }
        // can be worn ?
        if (item.WearLocation == WearLocations.None)
        {
            if (replace) // replace means, only item is trying to be worn
                Actor.Act(ActOptions.ToCharacter, "{0} cannot be worn.", item);
            return false;
        }
        if (item is IItemQuest)
        {
            Actor.Act(ActOptions.ToCharacter, "{0} cannot be worn.", item);
            return false;
        }
        // search slot
        var equipmentSlot = Actor.SearchEquipmentSlot(item, replace);
        if (equipmentSlot == null)
        {
            if (replace) // we don't want to spam if character is trying to wear all, replace is set to true only when wearing one item
                Actor.Act(ActOptions.ToCharacter, "You cannot wear {0}.", item);
            return false;
        }
        // too heavy to be wielded ?
        var weapon = item as IItemWeapon;
        if (weapon != null && Actor is IPlayableCharacter && !weapon.CanWield(Actor))
        {
            Actor.Send("It is too heavy for you to wield.");
            return false;
        }
        // remove if needed
        if (replace && equipmentSlot.Item != null)
        {
            IItem removeItem = equipmentSlot.Item;
            Actor.Act(ActOptions.ToAll, "{0:N} remove{0:v} {1}.", Actor, removeItem);
            //equipmentSlot.Item = null  already done by ChangeEquippedBy
            removeItem.ChangeEquippedBy(null!, false);
            removeItem.ChangeContainer(Actor);
        }
        // Display
        string wearPhrase = GetEquipmentSlotWearPhrase(equipmentSlot.Slot, item);
        Actor.Act(ActOptions.ToAll, wearPhrase, Actor, item);
        // Equip at last
        equipmentSlot.Item = item; // equip
        item.ChangeContainer(null); // remove from inventory
        item.ChangeEquippedBy(Actor, false); // set as equipped by this
        // Display weapon confidence if wielding weapon
        if (weapon != null)
        {
            string weaponConfidence = GetWeaponConfidence(weapon);
            Actor.Act(ActOptions.ToCharacter, weaponConfidence, weapon);
        }
        // no need to recompute, because it's being done by caller

        return true;
    }

    private string GetEquipmentSlotWearPhrase(EquipmentSlots slot, IItem item)
    {
        switch (slot)
        {
            case EquipmentSlots.Light: return "{0:N} light{0:v} {1} and holds it.";
            case EquipmentSlots.Head: return "{0:N} wear{0:v} {1} on {0:s} head.";
            case EquipmentSlots.Amulet: return "{0:N} wear{0:v} {1} around {0:s} neck.";
            case EquipmentSlots.Chest: return "{0:N} wear{0:v} {1} on {0:s} torso.";
            case EquipmentSlots.Cloak: return "{0:N} wear{0:v} {1} about {0:s} torso.";
            case EquipmentSlots.Waist: return "{0:N} wear{0:v} {1} about {0:s} waist.";
            case EquipmentSlots.Wrists: return "{0:N} wear{0:v} {1} around {0:s} wrist.";
            case EquipmentSlots.Arms: return "{0:N} wear{0:v} {1} on {0:s} arms.";
            case EquipmentSlots.Hands: return "{0:N} wear{0:v} {1} on {0:s} hands.";
            case EquipmentSlots.Ring: return "{0:N} wear{0:v} {1} on {0:s} finger.";
            case EquipmentSlots.Legs: return "{0:N} wear{0:v} {1} on {0:s} legs.";
            case EquipmentSlots.Feet: return "{0:N} wear{0:v} {1} on {0:s} feet.";
            case EquipmentSlots.MainHand: return "{0:N} wield{0:v} {1}.";
            case EquipmentSlots.OffHand:
                return item switch
                {
                    IItemWeapon _ => "{0:N} wield{0:v} {1}.",
                    IItemShield _ => "{0:N} wear{0:v} {1} as a shield.",
                    _ => "{0:N} hold{0:v} {1} in {0:s} hand.",
                };
            case EquipmentSlots.Float: return "{0:N} release{0:v} {1} to float next to {0:m}.";
            default:
                Wiznet.Log($"Invalid EquipmentSlots {slot} for item {item.DebugName} character {Actor.DebugName}", WiznetFlags.Bugs, AdminLevels.Implementor);
                return "{0:N} wear{0:v} {1}.";
        }
    }

    private string GetWeaponConfidence(IItemWeapon weapon)
    {
        var (percentage, _) = Actor.GetWeaponLearnedAndPercentage(weapon);
        if (percentage >= 100)
            return "{0:N} feels like a part of you!";
        if (percentage > 85)
            return "You feel quite confident with {0:N}.";
        if (percentage > 70)
            return "You are skilled with {0:N}.";
        if (percentage > 50)
            return "Your skill with {0:N} is adequate.";
        if (percentage > 25)
            return "{0:N} feels a little clumsy in your hands.";
        if (percentage > 1)
            return "You fumble and almost drop {0:N}.";
        return "You don't even know which end is up on {0:N}.";
    }

}
