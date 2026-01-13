using Mud.Common.Attributes;
using Mud.Domain.SerializationData.Avatar;

namespace Mud.Server.Item.SerializationData;

[JsonBaseType(typeof(ItemData), "light")]
public class ItemLightData : ItemData
{
    public required int TimeLeft { get; set; }
}
