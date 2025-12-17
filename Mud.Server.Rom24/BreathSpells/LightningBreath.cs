using Microsoft.Extensions.Logging;
using Mud.Domain;
using Mud.Server.Ability;
using Mud.Server.Ability.Spell;
using Mud.Server.Common;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Effect;
using Mud.Server.Random;

namespace Mud.Server.Rom24.BreathSpells;

[Spell(SpellName, AbilityEffects.Damage | AbilityEffects.Debuff)]
[Syntax("cast [spell] <victim>")]
[Help(
@"These spells are for the use of dragons.  Acid, fire, frost, and lightning
damage one victim, whereas gas damages every PC in the room.  Fire and
frost can break objects, and acid can damage armor.

High level mages may learn and cast these spells as well.")]
[OneLineHelp("summons the electrical fury of a blue dragon")]
public class LightningBreath : OffensiveSpellBase
{
    private const string SpellName = "Lightning Breath";

    private IEffectManager EffectManager { get; }

    public LightningBreath(ILogger<LightningBreath> logger, IRandomManager randomManager, IEffectManager effectManager)
        : base(logger, randomManager)
    {
        EffectManager = effectManager;
    }

    protected override void Invoke()
    {
        Caster.ActToNotVictim(Victim, "{0} breathes a bolt of lightning at {1}.", Caster, Victim);
        Victim.Act(ActOptions.ToCharacter, "{0} breathes a bolt of lightning at you!", Caster);
        Caster.Act(ActOptions.ToCharacter, "You breathe a bolt of lightning at {0}.", Victim);

        int hp = Math.Max(10, Victim.CurrentHitPoints);
        int hpDamage = RandomManager.Range(1 + hp / 9, hp / 5);
        int diceDamage = RandomManager.Dice(Level, 20);
        int damage = Math.Max(hpDamage + diceDamage / 10, diceDamage + hpDamage / 10);

        if (Victim.SavesSpell(Level, SchoolTypes.Lightning))
        {
            var victimShockEffect = EffectManager.CreateInstance<ICharacter>("Shock");
            victimShockEffect?.Apply(Victim, Caster, SpellName, Level / 2, damage / 4);
            Victim.AbilityDamage(Caster, damage / 2, SchoolTypes.Lightning, "blast of lightning", true);
        }
        else
        {
            var victimShockEffect = EffectManager.CreateInstance<ICharacter>("Shock");
            victimShockEffect?.Apply(Victim, Caster, SpellName, Level, damage);
            Victim.AbilityDamage(Caster, damage, SchoolTypes.Lightning, "blast of lightning", true);
        }
    }
}
