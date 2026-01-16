using Mud.Server.Flags.Interfaces;

namespace Mud.Server.Flags;

public class IRVFlags : DataStructures.Flags.Flags, IIRVFlags
{
    public IRVFlags(params string[] flags)
        : base(flags)
    {
    }
}
