using Mud.Common.Attributes;
using Mud.Domain;
using Mud.Domain.SerializationData;

namespace Mud.Server.Item;

[JsonBaseType(typeof(ItemData), "portal")]
public class ItemPortalData : ItemData
{
    public required int DestinationRoomId { get; set; }
    public required PortalFlags PortalFlags { get; set; }
    public required int MaxChargeCount { get; set; }
    public required int CurrentChargeCount { get; set; }
}
