using Mud.Domain;

namespace Mud.POC.Affects
{
    public class CharacterFlagsAffect : FlagAffectBase<CharacterFlags>, ICharacterAffect
    {
        protected override string Target => "Flags";

        public void Apply(ICharacter character)
        {
            character.ApplyAffect(this);
        }
    }
}
