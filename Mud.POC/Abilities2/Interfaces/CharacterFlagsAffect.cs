using Mud.POC.Abilities2.Domain;

namespace Mud.POC.Abilities2.Interfaces
{
    public class CharacterFlagsAffect : FlagAffectBase<CharacterFlags>, ICharacterAffect
    {
        public void Apply(ICharacter character)
        {
            throw new System.NotImplementedException();
        }
    }
}
