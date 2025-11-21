using Microsoft.Extensions.Logging;
using Mud.Domain;
using Mud.Server.Ability;
using Mud.Server.Ability.Spell;
using Mud.Server.Common;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Character;
using Mud.Server.Random;
using Mud.Server.Rom24.Effects;
using System.Collections.ObjectModel;

namespace Mud.Server.Rom24.BreathSpells;

[Spell(SpellName, AbilityEffects.DamageArea | AbilityEffects.Debuff)]
[Syntax("cast [spell] <victim>")]
[Help(
@"These spells are for the use of dragons.  Acid, fire, frost, and lightning
damage one victim, whereas gas damages every PC in the room.  Fire and
frost can break objects, and acid can damage armor.

High level mages may learn and cast these spells as well.")]
public class GasBreath : NoTargetSpellBase
{
    private const string SpellName = "Gas Breath";

    private IServiceProvider ServiceProvider { get; }
    private IAuraManager AuraManager { get; }

    public GasBreath(ILogger<GasBreath> logger, IServiceProvider serviceProvider, IRandomManager randomManager, IAuraManager auraManager)
        : base(logger, randomManager)
    {
        ServiceProvider = serviceProvider;
        AuraManager = auraManager;
    }

    protected override void Invoke()
    {
        Caster.Act(ActOptions.ToRoom, "{0} breathes out a cloud of poisonous gas!", Caster);
        Caster.Send("You breath out a cloud of poisonous gas.");

        int hp = Math.Max(16, Caster.HitPoints);
        int hpDamage = RandomManager.Range(1 + hp / 15, hp / 8);
        int diceDamage = RandomManager.Dice(Level, 12);
        int damage = Math.Max(hpDamage + diceDamage / 10, diceDamage + hpDamage / 10);

        PoisonEffect roomPoisonEffect = new(ServiceProvider, RandomManager, AuraManager);
        roomPoisonEffect.Apply(Caster.Room, Caster, SpellName, Level, damage);
        var clone = new ReadOnlyCollection<ICharacter>(Caster.Room.People.Where(x =>
            !(x.IsSafeSpell(Caster, true) 
              || (x is INonPlayableCharacter && Caster is INonPlayableCharacter && (Caster.Fighting == x || x.Fighting == Caster)))).ToList());
        foreach (ICharacter victim in clone)
        {
            if (victim.SavesSpell(Level, SchoolTypes.Poison))
            {
                PoisonEffect victimPoisonEffect = new(ServiceProvider, RandomManager, AuraManager);
                victimPoisonEffect.Apply(victim, Caster, SpellName, Level/2, damage/4);
                victim.AbilityDamage(Caster, damage / 2, SchoolTypes.Poison, "blast of gas", true);
            }
            else
            {
                PoisonEffect victimPoisonEffect = new(ServiceProvider, RandomManager, AuraManager);
                victimPoisonEffect.Apply(victim, Caster, SpellName, Level, damage);
                victim.AbilityDamage(Caster, damage, SchoolTypes.Poison, "blast of gas", true);
            }
        }
    }
}
