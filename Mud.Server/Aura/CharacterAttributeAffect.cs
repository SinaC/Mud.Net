using Mud.Domain;
using System.Text;
using Mud.Server.Helpers;

namespace Mud.Server.Aura
{
    public class CharacterAttributeAffect : ICharacterAffect
    {
        public AffectOperators Operator { get; set; } // Or and Nor cannot be used
        public CharacterAttributeAffectLocations Location { get; set; }
        public int Modifier { get; set; }

        protected string Target => Location.ToString();

        public void Append(StringBuilder sb)
        {
            sb.AppendFormat("%c%modifies %y%{0} %c{1} %y%{2}", Target, Operator.PrettyPrint(), Modifier);
        }

        public void Apply(ICharacter character)
        {
            character.ApplyAffect(this);
        }
    }
}
