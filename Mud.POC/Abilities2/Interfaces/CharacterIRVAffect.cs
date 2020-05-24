using Mud.POC.Abilities2.Domain;

namespace Mud.POC.Abilities2.Interfaces
{
    public class CharacterIRVAffect : FlagAffectBase<IRVFlags>, ICharacterAffect
    {
        public IRVAffectLocations Location { get; set; }

        public void Apply(ICharacter character)
        {
            throw new System.NotImplementedException();
        }
    }
}
