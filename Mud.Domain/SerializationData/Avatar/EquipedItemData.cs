namespace Mud.Domain.SerializationData.Avatar;

public class EquippedItemData
{
    public required EquipmentSlots Slot { get; set; }

    public required ItemData? Item { get; set; }
}
