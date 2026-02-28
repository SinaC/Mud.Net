using Mud.POC.DeferredAction.StatusEffect;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mud.POC.DeferredAction;

public class BlindnessSpell : Skill
{
    public BlindnessSpell()
        : base("blindness", manaCost: 20, cooldownTicks: 2) { }

    public override void Execute(Mob caster, Mob target, World world)
    {
        //target.ApplyEffect(new StatusEffect(
        //    Name,
        //    StatusEffectType.Blindness,
        //    durationTicks: 8,
        //    source: caster)
        //{
        //    Modifiers = new AffectModifiers
        //    {
        //        HitRollBonus = -20
        //    }
        //});

        //world.Enqueue(new ScriptAction(ctx =>
        //    ctx.Notify($"{target.Name} is blinded!")));
        target.ApplyEffect(new StatusEffect.StatusEffect(
            Name,
            StatusEffectType.Blindness,
            5,
            caster.Level,
            new AffectModifiers
            {
                HitRollBonus = -20
            },
            new BlindnessEffectRule()), world);
    }
}
