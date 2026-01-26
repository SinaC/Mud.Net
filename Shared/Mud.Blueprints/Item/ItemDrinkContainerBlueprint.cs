namespace Mud.Blueprints.Item;

public class ItemDrinkContainerBlueprint : ItemBlueprintBase
{
    public int MaxLiquidAmount { get; set; } // v0

    public int CurrentLiquidAmount { get; set; } // v1

    public string? LiquidType { get; set; } // v2

    public bool IsPoisoned { get; set; } // v3 0: normal A: poisoned
}
