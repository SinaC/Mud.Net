using System;
using Mud.Domain;
using Mud.Server.Aura;
using Mud.Server.Blueprints.Item;

namespace Mud.Server.Item
{
    public class ItemWeapon : ItemEquipableBase<ItemWeaponBlueprint>, IItemWeapon
    {
        public ItemWeapon(Guid guid, ItemWeaponBlueprint blueprint, IContainer containedInto) 
            : base(guid, blueprint, containedInto)
        {
            Type = blueprint.Type;
            DiceCount = blueprint.DiceCount;
            DiceValue = blueprint.DiceValue;
            DamageType = blueprint.DamageType;
            BaseWeaponFlags = blueprint.Flags;
        }

        public ItemWeapon(Guid guid, ItemWeaponBlueprint blueprint, ItemWeaponData itemData, IContainer containedInto)
            : base(guid, blueprint, itemData, containedInto)
        {
            Type = blueprint.Type;
            DiceCount = blueprint.DiceCount;
            DiceValue = blueprint.DiceValue;
            DamageType = blueprint.DamageType;
            BaseWeaponFlags = itemData.WeaponFlags;
        }

        #region IItemWeapon

        #region IItem

        public override void Recompute() // overriding recompute and calling base will cause every collection to be iterate twice
        {
            base.Recompute();

            // 1/ Apply auras from room containing item if in a room
            if (ContainedInto is IRoom room && room.IsValid)
            {
                ApplyAuras<IItemWeapon>(ContainedInto, this);
            }

            // 2/ Apply auras from charcter equiping item if equiped by a character
            if (EquipedBy != null && EquipedBy.IsValid)
            {
                ApplyAuras<IItemWeapon>(EquipedBy, this);
            }

            // 3/ Apply own auras
            ApplyAuras<IItemWeapon>(this, this);
        }

        #endregion

        public WeaponTypes Type { get; }
        public int DiceCount { get; }
        public int DiceValue { get; }
        public SchoolTypes DamageType { get; }

        public WeaponFlags BaseWeaponFlags { get; protected set; }
        public WeaponFlags CurrentWeaponFlags { get; protected set; }

        public void ApplyAffect(ItemWeaponFlagsAffect affect)
        {
            switch (affect.Operator)
            {
                case AffectOperators.Add:
                case AffectOperators.Or:
                    CurrentWeaponFlags |= affect.Modifier;
                    break;
                case AffectOperators.Assign:
                    CurrentWeaponFlags = affect.Modifier;
                    break;
                case AffectOperators.Nor:
                    CurrentWeaponFlags &= ~affect.Modifier;
                    break;
            }
        }

        #endregion

        #region ItemBase

        public override ItemData MapItemData()
        {
            return new ItemWeaponData
            {
                ItemId = Blueprint.Id,
                Level = Level,
                DecayPulseLeft = DecayPulseLeft,
                ItemFlags = BaseItemFlags,
                WeaponFlags = BaseWeaponFlags,
                Auras = MapAuraData(),
            };
        }

        protected override void ResetAttributes()
        {
            base.ResetAttributes();

            CurrentWeaponFlags = BaseWeaponFlags;
        }

        #endregion
    }
}
