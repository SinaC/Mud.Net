using Mud.Domain;
using Mud.Server.Interfaces.Item;

namespace Mud.Server.Interfaces.Affect
{
    public interface IItemWeaponFlagsAffect : IFlagAffect<WeaponFlags>, IItemAffect<IItemWeapon>
    {
    }
}
