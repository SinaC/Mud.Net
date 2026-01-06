using Microsoft.Extensions.Logging;
using Mud.Domain;
using Mud.Domain.SerializationData.Avatar;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Item;

namespace Mud.Server.Character;

public class EquippedItem : IEquippedItem
{
    private ILogger Logger { get; }

    public EquipmentSlots Slot { get; }
    public IItem? Item { get; set; }

    public EquippedItem(ILogger logger, EquipmentSlots slot)
    {
        Logger = logger;

        Slot = slot;
    }

    public EquippedItemData MapEquippedData()
    {
        return new EquippedItemData
        {
            Slot = Slot,
            Item = Item?.MapItemData()
        };
    }

    public string EquipmentSlotsToString()
    {
        switch (Slot)
        {
            case EquipmentSlots.Light:
                return "%C%<used as light>          %x%";
            case EquipmentSlots.Head:
                return "%C%<worn on head>           %x%";
            case EquipmentSlots.Amulet:
                return "%C%<worn on neck>           %x%";
            case EquipmentSlots.Chest:
                return "%C%<worn on chest>          %x%";
            case EquipmentSlots.Cloak:
                return "%C%<worn about body>        %x%";
            case EquipmentSlots.Waist:
                return "%C%<worn about waist>       %x%";
            case EquipmentSlots.Wrists:
                return "%C%<worn around wrists>     %x%";
            case EquipmentSlots.Arms:
                return "%C%<worn on arms>           %x%";
            case EquipmentSlots.Hands:
                return "%C%<worn on hands>          %x%";
            case EquipmentSlots.Ring:
                return "%C%<worn on finger>         %x%";
            case EquipmentSlots.Legs:
                return "%C%<worn on legs>           %x%";
            case EquipmentSlots.Feet:
                return "%C%<worn on feet>           %x%";
            case EquipmentSlots.MainHand:
                return "%C%<wielded>                %x%";
            case EquipmentSlots.OffHand:
                if (Item != null)
                {
                    if (Item is IItemShield)
                        return "%C%<worn as shield>         %x%";
                    if (Item.WearLocation == WearLocations.Hold)
                        return "%C%<held>                   %x%";
                }
                return "%c%<offhand>                %x%";
            case EquipmentSlots.Float:
                return "%C%<floating nearby>        %x%";
            default:
                Logger.LogError("DoEquipment: missing WearLocation {slot}", Slot);
                break;
        }
        return "%C%<unknown>%x%";
    }
}
