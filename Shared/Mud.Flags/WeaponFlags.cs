using Mud.Flags.Interfaces;

namespace Mud.Flags;

public class WeaponFlags : DataStructures.Flags.Flags, IWeaponFlags
{
    public WeaponFlags(params string[] flags)
        : base(flags)
    {
    }
}
