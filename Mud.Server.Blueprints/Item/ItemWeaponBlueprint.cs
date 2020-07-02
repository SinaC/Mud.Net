using Mud.Domain;
using Mud.Server.Flags.Interfaces;

namespace Mud.Server.Blueprints.Item
{
    public class ItemWeaponBlueprint : ItemBlueprintBase
    {
        public WeaponTypes Type {get; set; }
        public int DiceCount { get; set; }
        public int DiceValue { get; set; }
        public SchoolTypes DamageType { get; set; }
        public IWeaponFlags Flags { get; set; }
        public string DamageNoun { get; set; }
    }
}
