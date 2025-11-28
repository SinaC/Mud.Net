using Mud.Domain;
using Mud.Server.Flags.Interfaces;

namespace Mud.Server.Interfaces.Affect.Character;

public interface ICharacterIRVAffect : IFlagsAffect<IIRVFlags, IIRVFlagValues>, ICharacterAffect
{
    IRVAffectLocations Location { get; set; }
}
