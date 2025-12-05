using Mud.Common.Attributes;
using Mud.Domain;
using Mud.Domain.SerializationData;
using Mud.Server.Affects;
using Mud.Server.Interfaces.Affect;
using Mud.Server.Interfaces.Affect.Character;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Character;
using System.Text;

namespace Mud.Server.POC.Affects
{
    [Affect("ShadowWordPain", typeof(ShadowWordPainAffectData))]
    public class ShadowWordPainAffect : ICharacterPeriodicAffect, ICustomAffect
    {
        public void Initialize(AffectDataBase data)
        {
            //TODO: ShadowWordPainAffectData
        }

        public void Append(StringBuilder sb)
        {
            sb.Append("%c%applies %B%shadow%B% damage periodically");
        }

        public AffectDataBase MapAffectData()
        {
            return new PoisonDamageAffectData();
        }

        public void Apply(IAura aura, ICharacter character)
        {
            character.Act(ActOptions.ToAll, "{0:N} suffer{0:v} shadow damage.", character);
            character.AbilityDamage(character, aura.Level / 10 + 1, SchoolTypes.Negative, "word of darkness", false);
        }
    }
}
