using Mud.Domain;

namespace Mud.Server.Item
{
    public interface IItemContainer : IItemCloseable, IContainer
    {
        int MaxWeight { get; }
        ContainerFlags ContainerFlags { get; }
        // Key already found in ICloseable
        int MaxWeightPerItem { get; }
        int WeightMultiplier { get; } // percentage
    }
}
