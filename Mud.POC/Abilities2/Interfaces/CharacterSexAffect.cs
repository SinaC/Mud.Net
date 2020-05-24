using Mud.POC.Abilities2.Domain;

namespace Mud.POC.Abilities2.Interfaces
{
    public class CharacterSexAffect : ICharacterAffect
    {
        public Sex Value { get; set; }

        public void Apply(ICharacter character)
        {
            throw new System.NotImplementedException();
        }
    }
}
