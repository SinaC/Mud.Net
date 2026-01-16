using Mud.Server.Domain;
using Mud.Flags.Interfaces;

namespace Mud.Server.Interfaces.Affect.Character;

public interface ICharacterIRVAffect : IFlagsAffect<IIRVFlags>, ICharacterAffect
{
    IRVAffectLocations Location { get; set; }
}
