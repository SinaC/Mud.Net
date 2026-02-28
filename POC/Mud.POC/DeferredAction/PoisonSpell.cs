using Mud.POC.DeferredAction.StatusEffect;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mud.POC.DeferredAction;

public class PoisonSpell : Skill
{
    public PoisonSpell() : base("poison", 10, cooldownTicks: 3)
    {
    }

    public override void Execute(Mob caster, Mob target, World world)
    {
        if (caster.IsDead || target == null || target.IsDead) return;

        //target.ApplyEffect(new StatusEffect("Poison", StatusEffectType.Poison, _durationTicks, caster)
        //{
        //    Metadata = { ["damagePerTick"] = _damagePerTick }
        //});
        target.ApplyEffect(new StatusEffect.StatusEffect(
            "Poison",
            StatusEffectType.Poison,
            duration: 8,
            level: caster.Level,
            modifiers: new AffectModifiers
            {
                HitRollBonus = -2,
                DamageRollBonus = -2
            },
            rule: new PoisonEffectRule()), world);

        caster.Wait = 2;

        //world.Enqueue(new ScriptAction(ctx =>
        //{
        //    ctx.Notify($"{caster.Name} poisons {target.Name}, dealing {_damagePerTick} damage per tick for {_durationTicks} ticks!");
        //}));
    }
}
