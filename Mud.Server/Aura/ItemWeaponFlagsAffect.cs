using Mud.Domain;
using Mud.Server.Item;

namespace Mud.Server.Aura
{
    public class ItemWeaponFlagsAffect : FlagAffectBase<WeaponFlags>, IItemAffect<IItemWeapon>
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
