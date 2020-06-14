using Mud.POC.Abilities2.Domain;

namespace Mud.POC.Abilities2.ExistingCode
{
    public interface IItemWeapon : IItem
    {
        WeaponTypes Type { get; }
        int DiceCount { get; }
        int DiceValue { get; }
        SchoolTypes DamageType { get; }

        WeaponFlags BaseWeaponFlags { get; }
        WeaponFlags WeaponFlags { get; }

        string DamageNoun { get; }

        void ApplyAffect(ItemWeaponFlagsAffect affect);
    }
}
