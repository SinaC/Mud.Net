using Mud.Domain;
using Mud.Server.Interfaces.Affect;
using Mud.Server.Interfaces.Item;

namespace Mud.Server.Affects
{
    public class ItemWeaponFlagsAffect : FlagAffectBase<WeaponFlags>, IItemWeaponFlagsAffect
    {
        protected override string Target => "Weapon flags";

        public ItemWeaponFlagsAffect()
        {
        }

        public ItemWeaponFlagsAffect(ItemWeaponFlagsAffectData data)
        {
            Operator = data.Operator;
            Modifier = data.Modifier;
        }

        public void Apply(IItemWeapon item)
        {
            item.ApplyAffect(this);
        }

        public override AffectDataBase MapAffectData()
        {
            return new ItemWeaponFlagsAffectData
            {
                Operator = Operator,
                Modifier = Modifier
            };
        }
    }
}
