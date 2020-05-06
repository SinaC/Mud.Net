using Mud.Domain;
using System.Text;
using Mud.Server.Helpers;

namespace Mud.Server.Aura
{
    public class CharacterAttributeAffect : ICharacterAffect
    {
        public CharacterAttributeAffectLocations Location { get; set; }
        public AffectOperators Operator { get; set; } // Or and Nor cannot be used
        public int Modifier { get; set; }

        protected string Target => Location.ToString();

        public CharacterAttributeAffect()
        {
        }

        public CharacterAttributeAffect(CharacterAttributeAffectData data)
        {
            Location = data.Location;
            Operator = data.Operator;
            Modifier = data.Modifier;
        }

        public void Append(StringBuilder sb)
        {
            sb.AppendFormat("%c%modifies %y%{0} %c%{1} %y%{2}%x%", Target, Operator.PrettyPrint(), Modifier);
        }

        public void Apply(ICharacter character)
        {
            character.ApplyAffect(this);
        }

        public AffectDataBase MapAffectData()
        {
            return new CharacterAttributeAffectData
            {
                Location = Location,
                Operator = Operator,
                Modifier = Modifier
            };
        }
    }
}
