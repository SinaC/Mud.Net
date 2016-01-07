using Mud.Server.Constants;

namespace Mud.Server.Item
{
    public interface IItemArmor : IItem, IEquipable
    {
        int Armor { get; }
        ArmorKinds ArmorKind { get; }
        // TODO: modifier
    }
}
