using Mud.DataStructures.Flags;

namespace Mud.Server.Flags.Interfaces
{
    public interface IItemFlags : IFlags<string, IItemFlagValues>
    {
    }

    public interface IItemFlagValues : IFlagValues<string>
    {
    }
}
