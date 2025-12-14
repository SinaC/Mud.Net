using Mud.DataStructures.Flags;

namespace Mud.POC.Flags
{
    public interface IFlagsManager
    {
        bool CheckFlags<TFlags>(TFlags flags)
            where TFlags : IFlags<string>;
    }
}
