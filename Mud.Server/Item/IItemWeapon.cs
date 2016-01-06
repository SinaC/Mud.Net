using Mud.Server.Constants;

namespace Mud.Server.Item
{
    public interface IItemWeapon : IItem, IEquipable
    {
        WeaponTypes Type { get; }
        int DiceCount { get; }
        int DiceValue { get; }
        DamageTypes DamageType { get; }
    }
}
