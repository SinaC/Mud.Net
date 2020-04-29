using Mud.Domain;

namespace Mud.POC.Affects
{
    public class ItemWeaponFlagsAffect : FlagAffectBase<WeaponFlags>, IItemAffect<IItemWeapon>
    {
        protected override string Target => "Weapon flags";

        public void Apply(IItemWeapon item)
        {
            item.ApplyAffect(this);
        }
    }
}
