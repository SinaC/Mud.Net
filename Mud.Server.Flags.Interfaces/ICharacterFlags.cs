using Mud.DataStructures.Flags;

namespace Mud.Server.Flags.Interfaces
{
    public interface ICharacterFlags : IFlags<string, ICharacterFlagValues>
    {
    }

    public interface ICharacterFlagValues : IFlagValues<string>
    {
    }
}
