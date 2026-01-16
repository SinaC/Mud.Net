using Microsoft.Extensions.Logging;
using Mud.Domain;
using Mud.Server.Ability;
using Mud.Server.Ability.Spell;
using Mud.Server.Common.Attributes;
using Mud.Server.Domain;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Effect;
using Mud.Server.Interfaces.Room;
using Mud.Server.Random;

namespace Mud.Server.Rom24.BreathSpells;

[Spell(SpellName, AbilityEffects.DamageArea | AbilityEffects.Debuff, PulseWaitTime = 24)]
[Syntax("cast [spell] <victim>")]
[Help(
@"These spells are for the use of dragons.  Acid, fire, frost, and lightning
damage one victim, whereas gas damages every PC in the room.  Fire and
frost can break objects, and acid can damage armor.

High level mages may learn and cast these spells as well.")]
[OneLineHelp("drains the life from a foe with the power a white dragon")]
public class FrostBreath : OffensiveSpellBase
{
    private const string SpellName = "Frost Breath";

    private IEffectManager EffectManager { get; }

    public FrostBreath(ILogger<FrostBreath> logger, IRandomManager randomManager, IEffectManager effectManager)
        : base(logger, randomManager)
    {
        EffectManager = effectManager;
    }

    protected override void Invoke()
    {
        Caster.ActToNotVictim(Victim, "{0} breathes out a freezing cone of frost!", Caster);
        Victim.Act(ActOptions.ToCharacter, "{0} breathes a freezing cone of frost over you!", Caster);
        Caster.Send("You breath out a cone of frost.");

        int hp = Math.Max(12, Victim[ResourceKinds.HitPoints]);
        int hpDamage = RandomManager.Range(1 + hp / 11, hp / 6);
        int diceDamage = RandomManager.Dice(Level, 18);
        int damage = Math.Max(hpDamage + diceDamage / 10, diceDamage + hpDamage / 10);

        BreathAreaAction breathAreaAction = new ();
        breathAreaAction.Apply(Victim, Caster, Level, damage, SchoolTypes.Cold, "blast of frost", "Chill Touch",
            () => EffectManager.CreateInstance<IRoom>("Cold"),
            () => EffectManager.CreateInstance<ICharacter>("Cold"));
    }
}
