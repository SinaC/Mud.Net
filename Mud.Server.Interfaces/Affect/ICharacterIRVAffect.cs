using Mud.Domain;

namespace Mud.Server.Interfaces.Affect
{
    public interface ICharacterIRVAffect : IFlagAffect<IRVFlags>, ICharacterAffect
    {
        IRVAffectLocations Location { get; set; }
    }
}
