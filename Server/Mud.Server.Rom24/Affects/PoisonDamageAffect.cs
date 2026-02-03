using Mud.Domain;
using Mud.Server.Affects;
using Mud.Server.Domain.Attributes;
using Mud.Server.Interfaces.Affect.Character;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Character;
using System.Text;

namespace Mud.Server.Rom24.Affects;

[AffectNoData("Poison")]
public class PoisonDamageAffect : NoAffectDataAffectBase, ICharacterPeriodicAffect
{
    public override void Append(StringBuilder sb)
    {
        sb.Append("%c%applies %G%Poison%x% damage periodically");
    }

    public void Apply(IAura aura, ICharacter character)
    {
        if (!character.CharacterFlags.IsSet("Slow"))
        {
            character.Act(ActOptions.ToAll, "{0:N} shiver{0:v} and suffer{0:v}.", character);
            character.AbilityDamage(character, aura.Level / 10 + 1, SchoolTypes.Poison, "poison", false);
        }
    }
}
