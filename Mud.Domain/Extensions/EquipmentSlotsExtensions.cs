using Mud.Common;
using Mud.Logger;

namespace Mud.Domain.Extensions;

public static class EquipmentSlotsExtensions
{
    public static IEnumerable<WearLocations> ToWearLocations(this EquipmentSlots equipmentSlot)
    {
        switch (equipmentSlot)
        {
            case EquipmentSlots.None: return WearLocations.None.Yield();
            case EquipmentSlots.Light: return WearLocations.Light.Yield();
            case EquipmentSlots.Head: return WearLocations.Head.Yield();
            case EquipmentSlots.Amulet: return WearLocations.Amulet.Yield();
            case EquipmentSlots.Chest: return WearLocations.Chest.Yield();
            case EquipmentSlots.Cloak: return WearLocations.Cloak.Yield();
            case EquipmentSlots.Waist: return WearLocations.Waist.Yield();
            case EquipmentSlots.Wrists: return WearLocations.Wrists.Yield();
            case EquipmentSlots.Arms: return WearLocations.Arms.Yield();
            case EquipmentSlots.Hands: return WearLocations.Hands.Yield();
            case EquipmentSlots.Ring: return WearLocations.Ring.Yield();
            case EquipmentSlots.Legs: return WearLocations.Legs.Yield();
            case EquipmentSlots.Feet: return WearLocations.Feet.Yield();
            case EquipmentSlots.MainHand: return [WearLocations.Wield, WearLocations.Wield2H];
            case EquipmentSlots.OffHand: return [WearLocations.Hold, WearLocations.Wield, WearLocations.Shield];
            case EquipmentSlots.Float: return WearLocations.Float.Yield();
            default:
                Log.Default.WriteLine(LogLevels.Error, "ToWearLocations: Invalid equipment slot {0}", equipmentSlot);
                return WearLocations.None.Yield();
        }
    }
}
