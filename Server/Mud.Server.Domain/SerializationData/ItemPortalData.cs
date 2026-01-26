using Mud.Common.Attributes;
using Mud.Domain.SerializationData.Avatar;

namespace Mud.Server.Domain.SerializationData;

[JsonBaseType(typeof(ItemData), "portal")]
public class ItemPortalData : ItemData
{
    public required int DestinationRoomId { get; set; }
    public required string PortalFlags { get; set; }
    public required int MaxChargeCount { get; set; }
    public required int CurrentChargeCount { get; set; }
}
