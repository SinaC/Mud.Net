using Mud.Flags.Interfaces;

namespace Mud.Flags;

public class BodyForms : DataStructures.Flags.Flags, IBodyForms
{
    public BodyForms(params string[] flags)
        : base(flags)
    {
    }
}
