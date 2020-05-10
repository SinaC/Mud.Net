using Mud.Domain;

namespace Mud.Server.Blueprints.Item
{
    public class ItemContainerBlueprint : ItemBlueprintBase
    {
        public int ItemCount { get; set; } // maximum number of items
        public int WeightMultiplier { get; set; } // percentage
        public int Key { get; set; }
        public ContainerFlags ContainerFlags { get; set; }
    }
}
