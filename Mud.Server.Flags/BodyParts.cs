using Mud.Common.Attributes;
using Mud.DataStructures.Flags;
using Mud.Server.Flags.Interfaces;

namespace Mud.Server.Flags;

[Export(typeof(IBodyParts))]
public class BodyParts : Flags<IBodyPartValues>, IBodyParts
{
    public BodyParts(IBodyPartValues flagValues)
        : base(flagValues)
    {
    }
}
