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
public class HolyWord : NoTargetSpellBase
{
    private const string SpellName = "Holy Word";

    private IServiceProvider ServiceProvider { get; }
    private IAuraManager AuraManager { get; }

    public HolyWord(ILogger<HolyWord> logger, IServiceProvider serviceProvider, IRandomManager randomManager, IAuraManager auraManager)
        : base(logger, randomManager)
    {
        ServiceProvider = serviceProvider;
        AuraManager = auraManager;
    }

    protected override void Invoke()
    {
        Caster.Act(ActOptions.ToRoom, "{0:N} utters a word of divine power!");
        Caster.Send("You utter a word of divine power.");

        var clone = new ReadOnlyCollection<ICharacter>(Caster.Room.People.ToList()); // to avoid modification during iteration
        foreach (var victim in clone)
        {
            if ((Caster.IsGood && victim.IsGood)
                || (Caster.IsNeutral && victim.IsNeutral)
                || (Caster.IsEvil && victim.IsEvil))
            {
                victim.Send("You feel full more powerful.");
                FrenzyEffect frenzyEffect = new (AuraManager);
                frenzyEffect.Apply(victim, Caster, "Frenzy", Level, 0);
                BlessEffect blessEffect = new (AuraManager);
                blessEffect.Apply(victim, Caster, "Bless", Level, 0);
            }
            else if ((Caster.IsGood && victim.IsEvil)
                     || (Caster.IsEvil && victim.IsGood))
            {
                if (!victim.SavesSpell(Level, SchoolTypes.Holy))
                {
                    victim.Send("You are struck down!");
                    int damage = RandomManager.Dice(Level, 6);
                    var damageResult = victim.AbilityDamage(Caster, damage, SchoolTypes.Holy, "divine wrath", true);
                    if (damageResult == DamageResults.Done)
                    {
                        CurseEffect curseEffect = new (ServiceProvider, AuraManager);
                        curseEffect.Apply(victim, Caster, "Curse", Level, 0);
                    }
                }

                else if (Caster.IsNeutral)
                {
                    if (!victim.SavesSpell(Level, SchoolTypes.Holy))
                    {
                        victim.Send("You are struck down!");
                        int damage = RandomManager.Dice(Level, 4);
                        var damageResult = victim.AbilityDamage(Caster, damage, SchoolTypes.Holy, "divine wrath", true);
                        if (damageResult == DamageResults.Done)
                        {
                            CurseEffect curseEffect = new (ServiceProvider, AuraManager);
                            curseEffect.Apply(victim, Caster, "Curse", Level, 0);
                        }
                    }
                }
            }
        }

        Caster.Send("You feel drained.");
        Caster.UpdateMovePoints(-Caster.MovePoints); // set to 0
        Caster.UpdateHitPoints(-Caster.HitPoints / 2);
    }
}
