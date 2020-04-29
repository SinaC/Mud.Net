using Mud.Domain;

namespace Mud.POC.Affects
{
    public interface IItemWeapon : IItem
    {
        WeaponFlags BaseWeaponFlags { get; }
        WeaponFlags CurrentWeaponFlags { get; }

        void ApplyAffect(ItemWeaponFlagsAffect affect);
    }
}
