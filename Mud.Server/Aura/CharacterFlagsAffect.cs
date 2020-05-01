using Mud.Domain;

namespace Mud.Server.Aura
{
    public class CharacterFlagsAffect : FlagAffectBase<CharacterFlags>, ICharacterAffect
    {
        protected override string Target => "Flags";

        public CharacterFlagsAffect()
        {
        }

        public CharacterFlagsAffect(CharacterFlagsAffectData data)
        {
            Operator = data.Operator;
            Modifier = data.Modifier;
        }

        public void Apply(ICharacter character)
        {
            character.ApplyAffect(this);
        }

        public override AffectDataBase MapAffectData()
        {
            return new CharacterFlagsAffectData
            {
                Operator = Operator,
                Modifier = Modifier
            };
        }
    }
}
