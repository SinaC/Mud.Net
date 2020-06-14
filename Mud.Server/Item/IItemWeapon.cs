using Mud.Domain;

namespace Mud.Server.Item
{
    public interface IItemWeapon : IItem, IEquipable
    {
        WeaponTypes Type { get; }
        int DiceCount { get; }
        int DiceValue { get; }
        SchoolTypes DamageType { get; }
    }
}
