using Mud.Domain.Serialization;

namespace Mud.Domain.SerializationData;

[JsonPolymorphism(typeof(ItemData), "portal")]
public class ItemPortalData : ItemData
{
    public required int DestinationRoomId { get; set; }
    public required PortalFlags PortalFlags { get; set; }
    public required int MaxChargeCount { get; set; }
    public required int CurrentChargeCount { get; set; }
}
