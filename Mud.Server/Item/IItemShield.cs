namespace Mud.Server.Item
{
    public interface IItemShield : IItem, IEquippableItem
    {
        int Armor { get; }
        // TODO: resistances
    }
}
