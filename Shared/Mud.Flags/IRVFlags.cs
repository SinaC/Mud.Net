using Mud.Flags.Interfaces;

namespace Mud.Flags;

public class IRVFlags : DataStructures.Flags.Flags, IIRVFlags
{
    public IRVFlags(params string[] flags)
        : base(flags)
    {
    }
}
