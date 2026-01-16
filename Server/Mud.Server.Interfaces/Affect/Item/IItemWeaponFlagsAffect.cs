using Mud.Server.Flags.Interfaces;
using Mud.Server.Interfaces.Item;

namespace Mud.Server.Interfaces.Affect.Item;

public interface IItemWeaponFlagsAffect : IFlagsAffect<IWeaponFlags>, IItemAffect<IItemWeapon>
{
}
