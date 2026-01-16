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
using Mud.Random;

namespace Mud.Server.Rom24.BreathSpells;

[Spell(SpellName, AbilityEffects.DamageArea | AbilityEffects.Debuff, PulseWaitTime = 24)]
[AbilityCharacterWearOffMessage("The smoke leaves your eyes.")]
[Syntax("cast [spell] <victim>")]
[Help(
@"These spells are for the use of dragons.  Acid, fire, frost, and lightning
damage one victim, whereas gas damages every PC in the room.  Fire and
frost can break objects, and acid can damage armor.

High level mages may learn and cast these spells as well.")]
[OneLineHelp("calls forth the flames of a red dragon")]
public class FireBreath : OffensiveSpellBase
{
    private const string SpellName = "Fire Breath";

    private IEffectManager EffectManager { get; }

    public FireBreath(ILogger<FireBreath> logger, IRandomManager randomManager, IEffectManager effectManager)
        : base(logger, randomManager)
    {
        EffectManager = effectManager;
    }

    protected override void Invoke()
    {
        Caster.ActToNotVictim(Victim, "{0} breathes forth a cone of fire.", Caster);
        Victim.Act(ActOptions.ToCharacter, "{0} breathes a cone of hot fire over you!", Caster);
        Caster.Send("You breath forth a cone of fire.");

        int hp = Math.Max(10, Victim[ResourceKinds.HitPoints]);
        int hpDamage = RandomManager.Range(1 + hp / 9, hp / 5);
        int diceDamage = RandomManager.Dice(Level, 20);
        int damage = Math.Max(hpDamage + diceDamage / 10, diceDamage + hpDamage / 10);

        BreathAreaAction breathAreaAction = new ();
        breathAreaAction.Apply(Victim, Caster, Level, damage, SchoolTypes.Fire, "blast of fire", SpellName,
            () => EffectManager.CreateInstance<IRoom>("Fire"),
            () => EffectManager.CreateInstance<ICharacter>("Fire"));
    }
}
