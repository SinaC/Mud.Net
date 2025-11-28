using Mud.Server.Flags.Interfaces;

namespace Mud.Server.Interfaces.Affect.Room;

public interface IRoomFlagsAffect : IFlagsAffect<IRoomFlags, IRoomFlagValues>, IRoomAffect
{
}
