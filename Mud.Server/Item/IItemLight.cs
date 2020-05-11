namespace Mud.Server.Item
{
    public interface IItemLight : IItem, IEquippableItem
    {
        bool IsLighten { get; }
    }
}
