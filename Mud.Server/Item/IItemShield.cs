namespace Mud.Server.Item
{
    public interface IItemShield : IItem, IEquipable
    {
        int Armor { get; }
        // TODO: resistances
    }
}
