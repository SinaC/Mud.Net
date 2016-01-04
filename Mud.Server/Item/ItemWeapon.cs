using System;
using Mud.Server.Blueprints;
using Mud.Server.Constants;

namespace Mud.Server.Item
{
    public class ItemWeapon : ItemBase
    {
        public WeaponTypes Type { get; set; }
        public int DiceCount { get; private set; }
        public int DiceValue { get; private set; }
        public DamageTypes DamageType { get; private set; }
        // TODO: special type, damage string (see 2nd col  in const.C:208), proc

        public ItemWeapon(Guid guid, ItemWeaponBlueprint blueprint, IContainer containedInto) 
            : base(guid, blueprint, containedInto)
        {
            Type = blueprint.Type;
            DiceCount = blueprint.DiceCount;
            DiceValue = blueprint.DiceValue;
            DamageType = blueprint.DamageType;
        }
    }
}
