namespace Mud.Server.Item
{
    public interface IItemPortal : IItem
    {
        IRoom Destination { get; }
    }
}
