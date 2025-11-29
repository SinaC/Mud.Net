using Mud.Common.Attributes;
using Mud.DataStructures.Flags;
using Mud.Server.Flags.Interfaces;

namespace Mud.Server.Flags;

[Export(typeof(IIRVFlags))]
public class IRVFlags : Flags<IIRVFlagValues>, IIRVFlags
{
    public IRVFlags(IIRVFlagValues flagValues)
        : base(flagValues)
    {
    }
}
