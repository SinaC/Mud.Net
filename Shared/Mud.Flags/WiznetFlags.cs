using Mud.Flags.Interfaces;

namespace Mud.Flags;

public class WiznetFlags : DataStructures.Flags.Flags, IWiznetFlags
{
    public WiznetFlags(params string[] flags)
        : base(flags)
    {
    }
}
