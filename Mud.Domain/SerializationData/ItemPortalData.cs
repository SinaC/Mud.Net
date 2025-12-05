namespace Mud.Domain.SerializationData;

public class ItemPortalData : ItemData
{
    public required int DestinationRoomId { get; set; }
    public required PortalFlags PortalFlags { get; set; }
    public required int MaxChargeCount { get; set; }
    public required int CurrentChargeCount { get; set; }
}
