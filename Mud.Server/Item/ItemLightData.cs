using Mud.Domain.Serialization;
using Mud.Domain.SerializationData;

namespace Mud.Server.Item;

[JsonBaseType(typeof(ItemData), "light")]
public class ItemLightData : ItemData
{
    public required int TimeLeft { get; set; }
}
