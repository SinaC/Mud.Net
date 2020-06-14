namespace Mud.Server.Item
{
    public interface IItemContainer : IItem, IContainer
    {
        int ItemCount { get; } // maximum number of items
        int WeightMultiplier { get; } // percentage
    }
}
