using Mud.Domain;
using Mud.Domain.SerializationData;
using Mud.Server.Interfaces.Item;

namespace Mud.Server.Interfaces.Character;

public interface IEquippedItem
{
    EquipmentSlots Slot { get; }
    IItem? Item { get; set; }

    string EquipmentSlotsToString();

    EquippedItemData MapEquippedData();
}
