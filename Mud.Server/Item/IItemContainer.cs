using Mud.Domain;

namespace Mud.Server.Item
{
    public interface IItemContainer : IItemCloseable, IContainer
    {
        // MaxWeight already found in IContainer
        ContainerFlags ContainerFlags { get; }
        // Key already found in ICloseable
        // MaxWeightPerItem already found in IContainer
        int WeightMultiplier { get; } // percentage
    }
}
