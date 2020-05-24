using Mud.POC.Abilities2.Domain;

namespace Mud.POC.Abilities2.Interfaces
{
    public class CharacterAttributeAffect : ICharacterAffect
    {
        public CharacterAttributeAffectLocations Location { get; set; }
        public AffectOperators Operator { get; set; } // Or and Nor cannot be used
        public int Modifier { get; set; }

        public void Apply(ICharacter character)
        {
            throw new System.NotImplementedException();
        }
    }
}
