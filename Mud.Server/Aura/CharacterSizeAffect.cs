using Mud.Domain;
using System.Text;

namespace Mud.Server.Aura
{
    public class CharacterSizeAffect : ICharacterAffect
    {
        public Sizes Value { get; set; }

        public void Append(StringBuilder sb)
        {
            sb.AppendFormat("%c%modifies %y%size %c%by setting to %y%{0}%x%", Value);
        }

        public CharacterSizeAffect()
        {
        }

        public CharacterSizeAffect(CharacterSizeAffectData data)
        {
            Value = data.Value;
        }

        public void Apply(ICharacter character)
        {
            character.ApplyAffect(this);
        }

        public AffectDataBase MapAffectData()
        {
            return new CharacterSizeAffectData
            {
                Value = Value
            };
        }
    }
}
