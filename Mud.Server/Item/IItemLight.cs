namespace Mud.Server.Item
{
    public interface IItemLight : IItem, IEquipableItem
    {
        bool IsLighten { get; }
    }
}
