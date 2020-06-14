using System.Text;
using Mud.Domain;
using Mud.Server.Interfaces.Affect;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Character;

namespace Mud.Server.Rom24.Affects
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

                character.AbilityDamage(character, aura.Level / 10 + 1, SchoolTypes.Poison, "poison", false);
            }
        }
    }
}
