using Mud.Server.Flags.Interfaces;

namespace Mud.Server.Flags;

public class WeaponFlags : DataStructures.Flags.Flags, IWeaponFlags
{
    public WeaponFlags(params string[] flags)
        : base(flags)
    {
    }
}
