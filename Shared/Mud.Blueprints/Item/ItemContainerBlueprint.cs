using Mud.Flags.Interfaces;

namespace Mud.Blueprints.Item;

public class ItemContainerBlueprint : ItemBlueprintBase
{
    public int MaxItems { get; set; } // maximum number of items
    public int MaxWeight { get; set; } // maximum weight // v0
    public IContainerFlags ContainerFlags { get; set; } = default!; // v1
    public int Key { get; set; } // v2
    public int MaxWeightPerItem { get; set; } // maximum weight for one item // v3
    public int WeightMultiplier { get; set; } // percentage // v4
}
