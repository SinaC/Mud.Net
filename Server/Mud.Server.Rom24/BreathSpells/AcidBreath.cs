using Microsoft.Extensions.Logging;
using Mud.Domain;
using Mud.Random;
using Mud.Server.Ability.Spell;
using Mud.Server.Domain;
using Mud.Server.Domain.Attributes;
using Mud.Server.Effects.Interfaces;
using Mud.Server.Interfaces.Character;

namespace Mud.Server.Rom24.BreathSpells;

[Spell(SpellName, AbilityEffects.Damage | AbilityEffects.Debuff, PulseWaitTime = 24)]
[Syntax("cast [spell] <victim>")]
[Help(
@"These spells are for the use of dragons.  Acid, fire, frost, and lightning
damage one victim, whereas gas damages every PC in the room.  Fire and
frost can break objects, and acid can damage armor.

High level mages may learn and cast these spells as well.")]
[OneLineHelp("uses the black dragon's attack upon an enemy")]
public class AcidBreath : OffensiveSpellBase
{
    private const string SpellName = "Acid Breath";

    private IEffectManager EffectManager { get; }

    public AcidBreath(ILogger<AcidBreath> logger, IRandomManager randomManager, IEffectManager effectManager)
        : base(logger, randomManager)
    {
        EffectManager = effectManager;
    }

    protected override void Invoke()
    {
        Caster.ActToNotVictim(Victim, "{0} spits acid at {1}.", Caster, Victim);
        Victim.Act(ActOptions.ToCharacter, "{0} spits a stream of corrosive acid at you.", Caster);
        Caster.Act(ActOptions.ToCharacter, "You spit acid at {0}.", Victim);

        int hp = Math.Max(12, Victim[ResourceKinds.HitPoints]);
        int hpDamage = RandomManager.Range(1 + hp / 11, hp / 6);
        int diceDamage = RandomManager.Dice(Level, 16);
        int damage = Math.Max(hpDamage + diceDamage / 10, diceDamage + hpDamage / 10);

        if (Victim.SavesSpell(Level, SchoolTypes.Acid))
        {
            var effect = EffectManager.CreateInstance<ICharacter>("Acid");
            effect?.Apply(Victim, Caster, SpellName, Level/2, damage/4);
            Victim.AbilityDamage(Caster, damage / 2, SchoolTypes.Acid, "blast of acid", true);
        }
        else
        {
            var effect = EffectManager.CreateInstance<ICharacter>("Acid");
            effect?.Apply(Victim, Caster, SpellName, Level, damage);
            Victim.AbilityDamage(Caster, damage, SchoolTypes.Acid, "blast of acid", true);
        }
    }
}
