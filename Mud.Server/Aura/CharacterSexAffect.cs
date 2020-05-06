using Mud.Domain;
using System.Text;

namespace Mud.Server.Aura
{
    public class CharacterSexAffect : ICharacterAffect
    {
        public Sex Value { get; set; }

        public void Append(StringBuilder sb)
        {
            sb.AppendFormat("%c%modifies %y%sex %c%by setting to %y%{0}%x%", Value);
        }

        public CharacterSexAffect()
        {
        }

        public CharacterSexAffect(CharacterSexAffectData data)
        {
            Value = data.Value;
        }

        public void Apply(ICharacter character)
        {
            character.ApplyAffect(this);
        }

        public AffectDataBase MapAffectData()
        {
            return new CharacterSexAffectData
            {
                Value = Value
            };
        }
    }
}
