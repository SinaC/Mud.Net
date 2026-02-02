using Mud.DataStructures.Flags;
using System.Text;

namespace Mud.Server.Flags.Interfaces;

public interface IFlagsManager
{
    IEnumerable<string> AvailableValues<TFlags>()
        where TFlags : IFlags<string>;

    bool CheckFlags(Type flagType, IFlags<string>? flags);

    bool CheckFlags<TFlags>(TFlags? flags)
        where TFlags : IFlags<string>;

    StringBuilder Append<TFlags>(StringBuilder sb, TFlags? flags, bool shortDisplay)
       where TFlags : IFlags<string>;

    string PrettyPrint<TFlags>(TFlags? flags, bool shortDisplay)
        where TFlags : IFlags<string>;
}
