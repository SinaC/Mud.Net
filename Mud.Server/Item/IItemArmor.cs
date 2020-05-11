namespace Mud.Server.Item
{
    public interface IItemArmor : IItem, IEquippableItem
    {
        int Bash { get; }
        int Pierce { get; }
        int Slash { get; }
        int Exotic { get; }
    }
}
