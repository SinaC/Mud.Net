using System.Collections.Generic;
using Mud.Domain;
using Mud.Logger;
using Mud.Server.Common;

namespace Mud.Server.Helpers
{
    public static class MiscHelpers
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
                case EquipmentSlots.MainHand: return new[] {WearLocations.Wield, WearLocations.Wield2H};
                case EquipmentSlots.OffHand: return new[] {WearLocations.Hold, WearLocations.Wield, WearLocations.Shield};
                case EquipmentSlots.Float: return WearLocations.Float.Yield();
                default:
                    Log.Default.WriteLine(LogLevels.Error, "ToWearLocations: Invalid equipment slot {0}", equipmentSlot);
                    return WearLocations.None.Yield();
            }
        }

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
                case WearLocations.Wield: return new [] { EquipmentSlots.MainHand, EquipmentSlots.OffHand};
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
}
