using Mud.Domain.Serialization;
using Mud.Domain.SerializationData;

namespace Mud.Server.Item;

[JsonPolymorphism(typeof(ItemData), "drinkContainer")]
public class ItemDrinkContainerData : ItemData
{
    public required int MaxLiquidAmount { get; set; }

    public required int CurrentLiquidAmount { get; set; }

    public required string? LiquidName { get; set; }

    public required bool IsPoisoned { get; set; }
}
