using Mud.Common.Attributes;
using Mud.DataStructures.Flags;
using Mud.Server.Flags.Interfaces;

namespace Mud.Server.Flags;

[Export(typeof(IActFlags))]
public class ActFlags : Flags<IActFlagValues>, IActFlags
{
    public ActFlags(IActFlagValues flagValues)
        : base(flagValues)
    {
    }
}
