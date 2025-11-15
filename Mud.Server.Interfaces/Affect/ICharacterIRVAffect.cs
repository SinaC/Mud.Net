using Mud.Domain;
using Mud.Server.Flags.Interfaces;

namespace Mud.Server.Interfaces.Affect;

public interface ICharacterIRVAffect : IFlagsAffect<IIRVFlags, IIRVFlagValues>, ICharacterAffect
{
    IRVAffectLocations Location { get; set; }
}
