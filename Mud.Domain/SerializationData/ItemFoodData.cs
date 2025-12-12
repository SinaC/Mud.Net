using Mud.Domain.Serialization;

namespace Mud.Domain.SerializationData;

[JsonPolymorphism(typeof(ItemData), "food")]
public class ItemFoodData : ItemData
{
    public required int FullHours { get; set; }
    public required int HungerHours { get; set; }
    public required bool IsPoisoned { get; set; }
}
