using Mud.Domain;
using Mud.Random;
using Mud.Server.Affects;
using Mud.Server.Domain.Attributes;
using Mud.Server.Effects.Interfaces;
using Mud.Server.Interfaces.Affect.Character;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Character;
using System.Text;

namespace Mud.Server.Rom24.Affects;

[AffectNoData("Plague")]
public class PlagueSpreadAndDamageAffect : NoAffectDataAffectBase, ICharacterPeriodicAffect
{
    private IRandomManager RandomManager { get; }
    private IEffectManager EffectManager { get; }

    public PlagueSpreadAndDamageAffect(IRandomManager randomManager, IEffectManager effectManager)
    {
        RandomManager = randomManager;
        EffectManager = effectManager;
    }

    public override void Append(StringBuilder sb)
    {
        sb.Append("%c%applies %r%Disease%x% damage periodically");
    }

    public void Apply(IAura aura, ICharacter character)
    {
        if (aura.Level == 1)
            return;

        character.Act(ActOptions.ToRoom, "{0:N} writhes in agony as plague sores erupt from {0:s} skin.", character);
        character.Send("You writhe in agony from the plague.");

        // spread
        if (character.Room != null)
        {
            var plagueEffect = EffectManager.CreateInstance<ICharacter>("Plague");
            foreach (var victim in character.Room.People)
            {
                if (RandomManager.Chance(6) && !victim.SavesSpell(aura.Level - 2, SchoolTypes.Disease))
                    plagueEffect?.Apply(victim, character, aura.AbilityName, aura.Level-1, -5);
            }
        }

        // damage
        var damage = Math.Min(character.Level, aura.Level / 5 + 1);
        character.UpdateResource(ResourceKinds.MovePoints, -damage);
        character.UpdateResource(ResourceKinds.Mana, -damage);
        character.UpdateResource(ResourceKinds.Psy, -damage);
        character.AbilityDamage(character, damage, SchoolTypes.Disease, "sickness", false);
    }
}
