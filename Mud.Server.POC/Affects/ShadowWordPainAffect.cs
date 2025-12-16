using Mud.Domain;
using Mud.Server.Affects;
using Mud.Server.Interfaces.Affect.Character;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Character;
using System.Text;

namespace Mud.Server.POC.Affects;

[AffectNoData("ShadowWordPain")]
public class ShadowWordPainAffect : NoAffectDataAffectBase, ICharacterPeriodicAffect
{
    public override void Append(StringBuilder sb)
    {
        sb.Append("applies %B%shadow%x% damage periodically");
    }

    public void Apply(IAura aura, ICharacter character)
    {
        character.Act(ActOptions.ToAll, "{0:N} suffer{0:v} shadow damage.", character);
        character.AbilityDamage(character, aura.Level / 10 + 1, SchoolTypes.Negative, "word of darkness", false);
    }
}
