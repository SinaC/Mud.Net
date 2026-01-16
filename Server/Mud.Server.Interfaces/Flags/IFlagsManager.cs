using Mud.DataStructures.Flags;
using System.Text;

namespace Mud.Server.Interfaces.Flags;

public interface IFlagsManager
{
    bool CheckFlags(Type flagType, IFlags<string>? flags);

    bool CheckFlags<TFlags>(TFlags? flags)
        where TFlags : IFlags<string>;

    StringBuilder Append<TFlags>(StringBuilder sb, TFlags? flags, bool shortDisplay)
       where TFlags : IFlags<string>;
}
