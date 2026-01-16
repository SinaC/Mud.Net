using Mud.Domain;
using Mud.Flags.Interfaces;

namespace Mud.Blueprints.Item;

public class ItemWeaponBlueprint : ItemBlueprintBase
{
    public WeaponTypes Type {get; set; }
    public int DiceCount { get; set; }
    public int DiceValue { get; set; }
    public SchoolTypes DamageType { get; set; }
    public IWeaponFlags Flags { get; set; } = default!;
    public string DamageNoun { get; set; } = default!;
}
