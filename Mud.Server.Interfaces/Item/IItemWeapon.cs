using Mud.Domain;
using Mud.Domain.SerializationData;
using Mud.Server.Blueprints.Item;
using Mud.Server.Flags.Interfaces;
using Mud.Server.Interfaces.Affect.Item;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Entity;

namespace Mud.Server.Interfaces.Item;

public interface IItemWeapon : IItem
{
    void Initialize(Guid guid, ItemWeaponBlueprint blueprint, IContainer containedInto);
    void Initialize(Guid guid, ItemWeaponBlueprint blueprint, ItemWeaponData itemData, IContainer containedInto);

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
