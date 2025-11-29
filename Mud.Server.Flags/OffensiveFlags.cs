using Mud.Common.Attributes;
using Mud.DataStructures.Flags;
using Mud.Server.Flags.Interfaces;

namespace Mud.Server.Flags;

[Export(typeof(IOffensiveFlags))]
public class OffensiveFlags : Flags<IOffensiveFlagValues>, IOffensiveFlags
{
    public OffensiveFlags(IOffensiveFlagValues flagValues)
        : base(flagValues)
    {
    }
}
