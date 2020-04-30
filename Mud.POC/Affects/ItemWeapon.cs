using Mud.Domain;

namespace Mud.POC.Affects
{
    public class ItemWeapon : ItemBase, IItemWeapon
    {
        public ItemWeapon(string name, IEntity containedInto, ICharacter equipedBy)
            : base(name, containedInto, equipedBy)
        {
        }

        public ItemWeapon(string name, IEntity containedInto, ICharacter equipedBy, ItemFlags itemFlags, WeaponFlags weaponFlags)
            : base(name, containedInto, equipedBy, itemFlags)
        {
            BaseWeaponFlags = weaponFlags;
        }

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
                default:
                    break;
            }
            return;
        }

        public override void Recompute() // overriding recompute and calling base will cause every collection to be iterate twice
        {
            base.Recompute();

            // 1/ Apply auras from room containing item if in a room
            if (ContainedIn is IRoom room && room.IsValid)
            {
                ApplyAuras<IItemWeapon>(ContainedIn, this);
            }

            // 2/ Apply auras from charcter equiping item if equiped by a character
            if (EquipedBy != null && EquipedBy.IsValid)
            {
                ApplyAuras<IItemWeapon>(EquipedBy, this);
            }

            // 3/ Apply own auras
            ApplyAuras<IItemWeapon>(this, this);
        }

        protected override void ResetAttributes()
        {
            base.ResetAttributes();

            CurrentWeaponFlags = BaseWeaponFlags;
        }
    }
}
