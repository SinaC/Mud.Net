using Mud.Domain;
using Mud.Server.Aura;

namespace Mud.Server.Item
{
    public interface IItemWeapon : IItem, IEquipable
    {
        WeaponTypes Type { get; }
        int DiceCount { get; }
        int DiceValue { get; }
        SchoolTypes DamageType { get; }

        WeaponFlags BaseWeaponFlags { get; }
        WeaponFlags CurrentWeaponFlags { get; }

        void ApplyAffect(ItemWeaponFlagsAffect affect);
    }
}
