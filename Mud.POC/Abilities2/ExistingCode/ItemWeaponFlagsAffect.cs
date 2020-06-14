using Mud.POC.Abilities2.Domain;

namespace Mud.POC.Abilities2.ExistingCode
{
    public class ItemWeaponFlagsAffect : FlagAffectBase<WeaponFlags>, IItemAffect<IItemWeapon>
    {
        public void Apply(IItemWeapon item)
        {
            throw new System.NotImplementedException();
        }
    }
}
