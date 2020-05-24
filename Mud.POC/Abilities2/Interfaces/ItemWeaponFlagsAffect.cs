using Mud.POC.Abilities2.Domain;

namespace Mud.POC.Abilities2.Interfaces
{
    public class ItemWeaponFlagsAffect : FlagAffectBase<WeaponFlags>, IItemAffect<IItemWeapon>
    {
        public void Apply(IItemWeapon item)
        {
            throw new System.NotImplementedException();
        }
    }
}
