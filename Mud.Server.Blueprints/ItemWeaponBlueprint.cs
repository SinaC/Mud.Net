using Mud.Server.Constants;

namespace Mud.Server.Blueprints
{
    public class ItemWeaponBlueprint : ItemBlueprintBase
    {
        public WeaponTypes Type {get; set; }
        public int DiceCount { get; set; }
        public int DiceValue { get; set; }
        public DamageTypes DamageType { get; set; }
    }
}
