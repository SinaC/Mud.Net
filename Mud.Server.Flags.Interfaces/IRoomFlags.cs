using Mud.DataStructures.Flags;

namespace Mud.Server.Flags.Interfaces
{
    public interface IRoomFlags : IFlags<string, IRoomFlagValues>
    {
    }

    public interface IRoomFlagValues : IFlagValues<string>
    {
    }
}
