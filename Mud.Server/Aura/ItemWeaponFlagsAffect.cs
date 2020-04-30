using Mud.Domain;
using Mud.Server.Item;

namespace Mud.Server.Aura
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
