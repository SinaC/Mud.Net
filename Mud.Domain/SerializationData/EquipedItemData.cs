using Mud.Domain;

namespace Mud.Domain.SerializationData;

public class EquippedItemData
{
    public required EquipmentSlots Slot { get; set; }

    public required ItemData? Item { get; set; }
}
