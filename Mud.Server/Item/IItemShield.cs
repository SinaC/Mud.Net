namespace Mud.Server.Item
{
    public interface IItemShield : IItem, IEquipableItem
    {
        int Armor { get; }
        // TODO: resistances
    }
}
