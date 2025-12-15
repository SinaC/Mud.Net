using Mud.Domain;
using Mud.Domain.SerializationData;
using Mud.Server.Affects;
using Mud.Server.Interfaces.Affect.Character;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Character;
using System.Text;

namespace Mud.Server.Rom24.Affects;

[Affect(AffectName, typeof(NoAffectData))]
public class PoisonDamageAffect : ICharacterPeriodicAffect
{
    private const string AffectName = "Poison";

    public void Append(StringBuilder sb)
    {
        sb.Append("%c%applies %G%poison%x% damage periodically");
    }

    public AffectDataBase MapAffectData()
    {
        return new NoAffectData { AffectName = AffectName };
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
