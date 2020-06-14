using Mud.POC.Abilities2.Domain;

namespace Mud.POC.Abilities2.ExistingCode
{
    public class CharacterSizeAffect : ICharacterAffect
    {
        public Sizes Value { get; set; }

        public void Apply(ICharacter character)
        {
            throw new System.NotImplementedException();
        }
    }
}
