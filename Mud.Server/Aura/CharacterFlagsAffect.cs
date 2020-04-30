using Mud.Domain;

namespace Mud.Server.Aura
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
