using Mud.Flags.Interfaces;

namespace Mud.Flags;

public class BodyParts : DataStructures.Flags.Flags, IBodyParts
{
    public BodyParts(params string[] flags)
        : base(flags)
    {
    }
}
