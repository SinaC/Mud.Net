namespace Mud.Server.Blueprints.Item
{
    public class ItemContainerBlueprint : ItemBlueprintBase
    {
        public int ItemCount { get; set; } // maximum number of items
        public int WeightMultiplier { get; set; } // percentage
    }
}
