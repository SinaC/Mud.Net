using Mud.Domain;

namespace Mud.Server.Item
{
    public interface IItemContainer : IItemCloseable, IContainer
    {
        int ItemCount { get; } // maximum number of items
        int WeightMultiplier { get; } // percentage
        ContainerFlags ContainerFlags { get; }
    }
}
