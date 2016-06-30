using System;
using Mud.Server.Blueprints;
using Mud.Server.Constants;

namespace Mud.Server.Item
{
    public class ItemWeapon : ItemEquipableBase, IItemWeapon
    {
        public ItemWeapon(Guid guid, ItemWeaponBlueprint blueprint, IContainer containedInto) 
            : base(guid, blueprint, containedInto)
        {
            Type = blueprint.Type;
            DiceCount = blueprint.DiceCount;
            DiceValue = blueprint.DiceValue;
            DamageType = blueprint.DamageType;
        }

        #region IItemWeapon

        public WeaponTypes Type { get; set; }
        public int DiceCount { get; }
        public int DiceValue { get; }
        public SchoolTypes DamageType { get; }
        // TODO: special type, damage string (see 2nd col  in const.C:208), proc

        #endregion
    }
}
