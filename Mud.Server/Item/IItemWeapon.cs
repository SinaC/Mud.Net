using Mud.Domain;
using Mud.Server.Aura;

namespace Mud.Server.Item
{
    public interface IItemWeapon : IItem
    {
        WeaponTypes Type { get; }
        int DiceCount { get; }
        int DiceValue { get; }
        SchoolTypes DamageType { get; }

        WeaponFlags BaseWeaponFlags { get; }
        WeaponFlags WeaponFlags { get; }

        void ApplyAffect(ItemWeaponFlagsAffect affect);
    }
}
