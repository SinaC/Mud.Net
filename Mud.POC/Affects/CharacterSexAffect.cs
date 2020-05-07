using System.Text;

namespace Mud.POC.Affects
{
    public class CharacterSexAffect : ICharacterAffect
    {
        public Sex Value { get; set; }

        public void Append(StringBuilder sb)
        {
            sb.AppendFormat("%c%modifies %y%sex %cby setting to %y%{0}", Value);
        }

        public void Apply(ICharacter character)
        {
            character.ApplyAffect(this);
        }
    }
}
