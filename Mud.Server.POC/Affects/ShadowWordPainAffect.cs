using Mud.Domain;
using Mud.Domain.SerializationData;
using Mud.Server.Affects;
using Mud.Server.Interfaces.Affect.Character;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Character;
using System.Text;

namespace Mud.Server.POC.Affects
{
    [Affect(AffectName, typeof(NoAffectData))]
    public class ShadowWordPainAffect : ICharacterPeriodicAffect
    {
        private const string AffectName = "ShadowWordPain";

        public void Append(StringBuilder sb)
        {
            sb.Append("applies %B%shadow%x% damage periodically");
        }

        public AffectDataBase MapAffectData()
        {
            return new NoAffectData { AffectName = AffectName };
        }

        public void Apply(IAura aura, ICharacter character)
        {
            character.Act(ActOptions.ToAll, "{0:N} suffer{0:v} shadow damage.", character);
            character.AbilityDamage(character, aura.Level / 10 + 1, SchoolTypes.Negative, "word of darkness", false);
        }
    }
}
