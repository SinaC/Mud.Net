using Mud.Server.Flags.Interfaces;

namespace Mud.Server.Flags;

public class BodyForms : DataStructures.Flags.Flags, IBodyForms
{
    public BodyForms(params string[] flags)
        : base(flags)
    {
    }
}
