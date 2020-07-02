using Mud.DataStructures.Flags;

namespace Mud.Server.Flags.Interfaces
{
    public interface IWeaponFlags : IFlags<string, IWeaponFlagValues>
    {
    }

    public interface IWeaponFlagValues : IFlagValues<string>
    {
    }
}
