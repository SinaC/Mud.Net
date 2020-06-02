using System.Text;
using Mud.Domain;

namespace Mud.Server.Aura
{
    public class PoisonDamageAffect : ICharacterPeriodicAffect
    {
        public void Append(StringBuilder sb)
        {
            sb.Append("%c%applies %G%poison%x% damage periodically");
        }

        public AffectDataBase MapAffectData()
        {
            return new PoisonDamageAffectData();
        }

        public void Apply(IAura aura, ICharacter character)
        {
            if (!character.CharacterFlags.HasFlag(CharacterFlags.Slow))
            {
                character.Act(ActOptions.ToAll, "{0:N} shiver{0:v} and suffer{0:v}.", character);

                character.AbilityDamage(character, aura.Ability, aura.Level / 10 + 1, SchoolTypes.Poison, false);
            }
        }
    }
}
