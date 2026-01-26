using Mud.Flags.Interfaces;

namespace Mud.Flags;

public class AuraFlags : DataStructures.Flags.Flags, IAuraFlags
{
    public AuraFlags(params string[] flags)
        : base(flags)
    {
    }
}
