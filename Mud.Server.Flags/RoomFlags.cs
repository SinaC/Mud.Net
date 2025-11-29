using Mud.Common.Attributes;
using Mud.DataStructures.Flags;
using Mud.Server.Flags.Interfaces;

namespace Mud.Server.Flags;

[Export(typeof(IRoomFlags))]
public class RoomFlags : Flags<IRoomFlagValues>, IRoomFlags
{
    public RoomFlags(IRoomFlagValues flagValues)
        : base(flagValues)
    {
    }
}
