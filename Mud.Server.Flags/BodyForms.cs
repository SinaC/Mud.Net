using Mud.Common.Attributes;
using Mud.DataStructures.Flags;
using Mud.Server.Flags.Interfaces;

namespace Mud.Server.Flags;

[Export(typeof(IBodyForms))]
public class BodyForms : Flags<IBodyFormValues>, IBodyForms
{
    public BodyForms(IBodyFormValues flagValues)
        : base(flagValues)
    {
    }
}
