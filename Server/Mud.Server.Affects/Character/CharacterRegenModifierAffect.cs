using Mud.Domain.SerializationData.Avatar;
using Mud.Server.Domain;
using Mud.Server.Interfaces.Affect.Character;
using System.Text;

namespace Mud.Server.Affects.Character
{
    [Affect("RegenModifier", typeof(CharacterRegenModifierAffectData))]
    public class CharacterRegenModifierAffect : ICharacterRegenModifierAffect
    {
        public int Modifier { get; set; } = 1;
        public AffectOperators Operator { get; set; } = AffectOperators.Multiply;

        public void Initialize(CharacterRegenModifierAffectData data)
        {
            Modifier = data.Modifier;
            Operator = data.Operator;
        }

        public void Append(StringBuilder sb)
        {
            if (Operator == AffectOperators.Multiply)
                sb.AppendFormat("%c%speeds up %y%Regen%x% by a factor %y%{0}%x%", Modifier);
            else if (Operator == AffectOperators.Divide)
            {
                sb.AppendFormat("%c%slows down %y%Regen%x% by a factor %y%1/{0}%x%", Modifier);
            }
            else
                sb.Append("%c%no regen change%x%");
        }

        public AffectDataBase MapAffectData()
        {
            return new CharacterRegenModifierAffectData { Modifier = Modifier, Operator = Operator };
        }
    }
}
