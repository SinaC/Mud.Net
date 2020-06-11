using Mud.Domain;
using Mud.Server.Interfaces.Affect;
using Mud.Server.Interfaces.Character;

namespace Mud.Server.Affect
{
    public class CharacterFlagsAffect : FlagAffectBase<CharacterFlags>, ICharacterFlagsAffect
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
