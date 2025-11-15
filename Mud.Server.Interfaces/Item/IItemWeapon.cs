using Mud.Domain;
using Mud.Server.Flags.Interfaces;
using Mud.Server.Interfaces.Affect;
using Mud.Server.Interfaces.Character;

namespace Mud.Server.Interfaces.Item;

public interface IItemWeapon : IItem
{
    WeaponTypes Type { get; }
    int DiceCount { get; }
    int DiceValue { get; }
    SchoolTypes DamageType { get; }

    IWeaponFlags BaseWeaponFlags { get; }
    IWeaponFlags WeaponFlags { get; }

    string DamageNoun { get; }

    bool CanWield(ICharacter character);

    void ApplyAffect(IItemWeaponFlagsAffect affect);
}
