using Mud.Flags.Interfaces;

namespace Mud.Flags;

public class ImmortalModes : DataStructures.Flags.Flags, IImmortalModes
{
    public ImmortalModes(params string[] flags)
        : base(flags)
    {
    }
}
