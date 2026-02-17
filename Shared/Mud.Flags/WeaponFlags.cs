using Mud.Flags.Interfaces;

namespace Mud.Flags;

public class WeaponFlags(params string[] flags) : DataStructures.Flags.Flags(flags), IWeaponFlags
{
}
