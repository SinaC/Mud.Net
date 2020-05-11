namespace Mud.POC.Affects
{
    public class ItemWeapon : ItemBase, IItemWeapon
    {
        public ItemWeapon(string name, IEntity containedInto, ICharacter equippedBy)
            : base(name, containedInto, equippedBy)
        {
        }

        public ItemWeapon(string name, IEntity containedInto, ICharacter equippedBy, ItemFlags itemFlags, WeaponFlags weaponFlags)
            : base(name, containedInto, equippedBy, itemFlags)
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
            }
        }

        public override void Recompute() // overriding recompute and calling base will cause every collection to be iterate twice
        {
            base.Recompute();

            // 1/ Apply auras from room containing item if in a room
            if (ContainedIn is IRoom room && room.IsValid)
            {
                ApplyAuras<IItemWeapon>(ContainedIn, this);
            }

            // 2/ Apply auras from charcter equipping item if equipped by a character
            if (EquippedBy != null && EquippedBy.IsValid)
            {
                ApplyAuras<IItemWeapon>(EquippedBy, this);
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
