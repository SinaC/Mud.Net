using Mud.Common;
using Mud.Logger;

namespace Mud.Domain.Extensions;

public static class WearLocationsExtensions
{
    public static IEnumerable<EquipmentSlots> ToEquipmentSlots(this WearLocations wearLocation)
    {
        switch (wearLocation)
        {
            case WearLocations.None: return EquipmentSlots.None.Yield();
            case WearLocations.Light: return EquipmentSlots.Light.Yield();
            case WearLocations.Head: return EquipmentSlots.Head.Yield();
            case WearLocations.Amulet: return EquipmentSlots.Amulet.Yield();
            case WearLocations.Chest: return EquipmentSlots.Chest.Yield();
            case WearLocations.Cloak: return EquipmentSlots.Cloak.Yield();
            case WearLocations.Waist: return EquipmentSlots.Waist.Yield();
            case WearLocations.Wrists: return EquipmentSlots.Wrists.Yield();
            case WearLocations.Arms: return EquipmentSlots.Arms.Yield();
            case WearLocations.Hands: return EquipmentSlots.Hands.Yield();
            case WearLocations.Ring: return EquipmentSlots.Ring.Yield();
            case WearLocations.Legs: return EquipmentSlots.Legs.Yield();
            case WearLocations.Feet: return EquipmentSlots.Feet.Yield();
            //case WearLocations.Trinket:
            case WearLocations.Wield: return new[] { EquipmentSlots.MainHand, EquipmentSlots.OffHand };
            case WearLocations.Hold: return EquipmentSlots.OffHand.Yield();
            case WearLocations.Shield: return EquipmentSlots.OffHand.Yield();
            case WearLocations.Wield2H: return EquipmentSlots.MainHand.Yield();
            case WearLocations.Float: return EquipmentSlots.Float.Yield();
            default:
                Log.Default.WriteLine(LogLevels.Error, "ToEquipmentSlots: Invalid wear location {0}", wearLocation);
                return EquipmentSlots.None.Yield();
        }
    }
}
