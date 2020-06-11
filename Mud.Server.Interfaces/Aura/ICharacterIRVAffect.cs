using Mud.Domain;

namespace Mud.Server.Interfaces.Aura
{
    public interface ICharacterIRVAffect : IFlagAffect<IRVFlags>, ICharacterAffect
    {
        IRVAffectLocations Location { get; set; }
    }
}
