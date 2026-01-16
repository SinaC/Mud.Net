using Mud.Common.Attributes;
using Mud.Domain.SerializationData.Avatar;

namespace Mud.Server.Domain.SerializationData;

[JsonBaseType(typeof(ItemData), "drinkContainer")]
public class ItemDrinkContainerData : ItemData
{
    public required int MaxLiquidAmount { get; set; }

    public required int CurrentLiquidAmount { get; set; }

    public required string? LiquidName { get; set; }

    public required bool IsPoisoned { get; set; }
}
