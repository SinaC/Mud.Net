using Microsoft.Extensions.Logging;
using Mud.Domain;
using Mud.Server.Ability;
using Mud.Server.Ability.Spell;
using Mud.Server.Domain.Attributes;
using Mud.Server.Domain;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Effect;
using Mud.Random;
using System.Collections.ObjectModel;

namespace Mud.Server.Rom24.Spells;

[Spell(SpellName, AbilityEffects.Damage | AbilityEffects.Buff, PulseWaitTime = 24)]
[Syntax("cast [spell]")]
[Help(
@"Holy word involves the invocation of the full power of a cleric's god, with
disasterous effects upon the enemies of the priest coupled with powerful
blessings on the priest's allies.  All creatures of like alignment in the
room are blessed and filled with righteous divine wrath, while those of
opposite morals (or both good and evil in the case of neutral priests)
are struck down by holy (or unholy might) and cursed.  The cleric suffers
greatly from the strain of this spell, being left unable to move and 
drained of vitality.  Experience loss is no longer associated with the spell.")]
[OneLineHelp("aids your allies while calling divine wrath upon your foes")]
public class HolyWord : NoTargetSpellBase
{
    private const string SpellName = "Holy Word";

    private IEffectManager EffectManager { get; }

    public HolyWord(ILogger<HolyWord> logger, IRandomManager randomManager, IEffectManager effectManager)
        : base(logger, randomManager)
    {
        EffectManager = effectManager;
    }

    protected override void Invoke()
    {
        Caster.Act(ActOptions.ToRoom, "{0:N} utters a word of divine power!", Caster);
        Caster.Send("You utter a word of divine power.");

        var victims = Caster.Room.People.ToArray(); // to avoid modification during iteration
        foreach (var victim in victims)
        {
            if ((Caster.IsGood && victim.IsGood)
                || (Caster.IsNeutral && victim.IsNeutral)
                || (Caster.IsEvil && victim.IsEvil))
            {
                victim.Send("You feel full more powerful.");
                var frenzyEffect = EffectManager.CreateInstance<ICharacter>("Frenzy");
                frenzyEffect?.Apply(victim, Caster, "Frenzy", Level, 0);
                var blessEffect = EffectManager.CreateInstance<ICharacter>("Bless");
                blessEffect?.Apply(victim, Caster, "Bless", Level, 0);
            }
            else if ((Caster.IsGood && victim.IsEvil)
                     || (Caster.IsEvil && victim.IsGood))
            {
                if (!victim.IsSafeSpell(Caster, true))
                {
                    victim.Send("You are struck down!");
                    int damage = RandomManager.Dice(Level, 6);
                    var damageResult = victim.AbilityDamage(Caster, damage, SchoolTypes.Holy, "divine wrath", true);
                    if (damageResult == DamageResults.Done)
                    {
                        var curseEffect = EffectManager.CreateInstance<ICharacter>("Curse");
                        curseEffect?.Apply(victim, Caster, "Curse", Level, 0);
                    }
                }
                else if (Caster.IsNeutral)
                {
                    if (!victim.IsSafeSpell(Caster, true))
                    {
                        victim.Send("You are struck down!");
                        int damage = RandomManager.Dice(Level, 4);
                        var damageResult = victim.AbilityDamage(Caster, damage, SchoolTypes.Holy, "divine wrath", true);
                        if (damageResult == DamageResults.Done)
                        {
                            var curseEffect = EffectManager.CreateInstance<ICharacter>("Curse");
                            curseEffect?.Apply(victim, Caster, "Curse", Level, 0);
                        }
                    }
                }
            }

            if (Caster.Fighting == null) // caster has been killed with some backlash damage
                break;
        }

        Caster.Send("You feel drained.");
        Caster.UpdateResource(ResourceKinds.MovePoints, -Caster[ResourceKinds.MovePoints]); // set to 0
        Caster.UpdateResource(ResourceKinds.HitPoints, -Caster[ResourceKinds.HitPoints] / 2); // half-life
    }
}
