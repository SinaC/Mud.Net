using Mud.Server.Flags.Interfaces;

namespace Mud.Server.Flags;

public class BodyParts : DataStructures.Flags.Flags, IBodyParts
{
    public BodyParts(params string[] flags)
        : base(flags)
    {
    }
}
