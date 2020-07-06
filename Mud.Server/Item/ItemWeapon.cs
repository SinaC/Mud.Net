using System;
using System.Text;
using Mud.Container;
using Mud.Domain;
using Mud.Logger;
using Mud.Server.Blueprints.Item;
using Mud.Server.Flags;
using Mud.Server.Flags.Interfaces;
using Mud.Server.Interfaces.Affect;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Entity;
using Mud.Server.Interfaces.Item;
using Mud.Server.Interfaces.Room;
using Mud.Server.Interfaces.Table;

namespace Mud.Server.Item
{
    public class ItemWeapon : ItemBase<ItemWeaponBlueprint, ItemWeaponData>, IItemWeapon
    {
        public ITableValues TableValues => DependencyContainer.Current.GetInstance<ITableValues>();

        public ItemWeapon(Guid guid, ItemWeaponBlueprint blueprint, IContainer containedInto) 
            : base(guid, blueprint, containedInto)
        {
            Type = blueprint.Type;
            DiceCount = blueprint.DiceCount;
            DiceValue = blueprint.DiceValue;
            DamageType = blueprint.DamageType;
            BaseWeaponFlags = NewAndCopyAndSet<IWeaponFlags, IWeaponFlagValues>(() => new WeaponFlags(), blueprint.Flags, null);
            WeaponFlags = NewAndCopyAndSet<IWeaponFlags, IWeaponFlagValues>(() => new WeaponFlags(), BaseWeaponFlags, null);
            DamageNoun = blueprint.DamageNoun;
        }

        public ItemWeapon(Guid guid, ItemWeaponBlueprint blueprint, ItemWeaponData itemData, IContainer containedInto)
            : base(guid, blueprint, itemData, containedInto)
        {
            Type = blueprint.Type;
            DiceCount = blueprint.DiceCount;
            DiceValue = blueprint.DiceValue;
            DamageType = blueprint.DamageType;
            BaseWeaponFlags = NewAndCopyAndSet<IWeaponFlags, IWeaponFlagValues>(() => new WeaponFlags(), itemData.WeaponFlags, null);
            WeaponFlags = NewAndCopyAndSet<IWeaponFlags, IWeaponFlagValues>(() => new WeaponFlags(), BaseWeaponFlags, null);
            DamageNoun = blueprint.DamageNoun;
        }

        #region IItemWeapon

        #region IItem

        public override void Recompute() // overriding recompute and calling base will cause every collection to be iterate twice
        {
            Log.Default.WriteLine(LogLevels.Debug, "ItemWeapon.Recompute: {0}", DebugName);

            base.Recompute();

            // 1/ Apply auras from room containing item if in a room
            if (ContainedInto is IRoom room && room.IsValid)
            {
                ApplyAuras<IItemWeapon>(ContainedInto, this);
            }

            // 2/ Apply auras from character equiping item if equipped by a character
            if (EquippedBy != null && EquippedBy.IsValid)
            {
                ApplyAuras<IItemWeapon>(EquippedBy, this);
            }

            // 3/ Apply own auras
            ApplyAuras<IItemWeapon>(this, this);
        }

        #endregion

        public WeaponTypes Type { get; }
        public int DiceCount { get; }
        public int DiceValue { get; }
        public SchoolTypes DamageType { get; }

        public IWeaponFlags BaseWeaponFlags { get; protected set; }
        public IWeaponFlags WeaponFlags { get; protected set; }

        public string DamageNoun { get; set; }

        public bool CanWield(ICharacter character)
        {
            return TotalWeight <= TableValues.WieldBonus(character) * 10;
        }

        public void ApplyAffect(IItemWeaponFlagsAffect affect)
        {
            switch (affect.Operator)
            {
                case AffectOperators.Add:
                case AffectOperators.Or:
                    WeaponFlags.Set(affect.Modifier);
                    break;
                case AffectOperators.Assign:
                    WeaponFlags.Copy(affect.Modifier);
                    break;
                case AffectOperators.Nor:
                    WeaponFlags.Unset(affect.Modifier);
                    break;
            }
        }

        #endregion

        #region ItemBase

        public override StringBuilder Append(StringBuilder sb, ICharacter viewer, bool shortDisplay)
        {
            WeaponFlags.Append(sb, shortDisplay);
            return base.Append(sb, viewer, shortDisplay);
        }

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

            WeaponFlags.Copy(BaseWeaponFlags);
        }

        #endregion
    }
}
