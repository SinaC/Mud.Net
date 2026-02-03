using Mud.Domain;
using Mud.Server.Affects;
using Mud.Server.Domain.Attributes;
using Mud.Server.Interfaces.Affect.Character;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Character;
using System.Text;

namespace Mud.Server.POC.Affects;

[AffectNoData("Rake")]
public class RakeAffect : NoAffectDataAffectBase, ICharacterPeriodicAffect
{
    public override void Append(StringBuilder sb)
    {
        sb.Append("applies piercing damage periodically");
    }

    public void Apply(IAura aura, ICharacter character)
    {
        character.Act(ActOptions.ToAll, "{0:N} suffer{0:v} piercing damage.", character);
        character.AbilityDamage(character, 10, SchoolTypes.Pierce, "rake", false);
    }
}
