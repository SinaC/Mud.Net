using Mud.Common.Attributes;
using Mud.Domain.SerializationData.Avatar;

namespace Mud.Server.Item;

[JsonBaseType(typeof(ItemData), "food")]
public class ItemFoodData : ItemData
{
    public required int FullHours { get; set; }
    public required int HungerHours { get; set; }
    public required bool IsPoisoned { get; set; }
}
