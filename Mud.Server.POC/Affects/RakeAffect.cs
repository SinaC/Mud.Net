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
    [Affect("Rake", typeof(RakeAffectData))]
    public class RakeAffect : ICharacterPeriodicAffect, ICustomAffect
    {
        public void Initialize(AffectDataBase data)
        {
            //TODO: RakeAffectData
        }

        public void Append(StringBuilder sb)
        {
            sb.Append("applies piercing damage periodically");
        }

        public AffectDataBase MapAffectData()
        {
            return new RakeAffectData();
        }

        public void Apply(IAura aura, ICharacter character)
        {
            character.Act(ActOptions.ToAll, "{0:N} suffer{0:v} piercing damage.", character);
            character.AbilityDamage(character, 10, SchoolTypes.Pierce, "rake", false);
        }
    }
}
