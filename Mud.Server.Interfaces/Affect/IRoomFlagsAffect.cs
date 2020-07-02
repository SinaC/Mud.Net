using Mud.Server.Flags.Interfaces;

namespace Mud.Server.Interfaces.Affect
{
    public interface IRoomFlagsAffect : IFlagsAffect<IRoomFlags, IRoomFlagValues>, IRoomAffect
    {
    }
}
