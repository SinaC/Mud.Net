using Mud.Domain;
using Mud.Server.Item;

namespace Mud.Server.Interfaces.Aura
{
    public interface IItemWeaponFlagsAffect : IFlagAffect<WeaponFlags>, IItemAffect<IItemWeapon>
    {
    }
}
