using Mud.Domain.Serialization;

namespace Mud.Domain.SerializationData;

[JsonPolymorphism(typeof(ItemData), "light")]
public class ItemLightData : ItemData
{
    public required int TimeLeft { get; set; }
}
