using Mud.Server.Flags.Interfaces;

namespace Mud.Server.Interfaces.Affect;

public interface ICharacterFlagsAffect : IFlagsAffect<ICharacterFlags, ICharacterFlagValues>, ICharacterAffect
{
}
