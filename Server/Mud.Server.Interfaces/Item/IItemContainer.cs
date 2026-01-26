using Mud.Flags.Interfaces;

namespace Mud.Server.Interfaces.Item;

public interface IItemContainer : IItemCloseable, IItemCanContain
{
    int MaxItems { get; }
    int MaxWeight { get; }
    IContainerFlags ContainerFlags { get; }
    // Key already found in ICloseable
    int MaxWeightPerItem { get; }
    int WeightMultiplier { get; } // percentage

    // TODO: to remove, should only be used just after item creation
    void SetCustomValues(int level, int maxWeight, int maxWeightPerItem); // TODO: should be remove once a system to create item with custom values is implemented
}
