using Mud.POC.DeferredAction.StatusEffect;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mud.POC.DeferredAction;

public class SanctuarySpell : Skill
{
    public SanctuarySpell()
        : base("sanctuary", manaCost: 75, cooldownTicks: 2) { }

    public override void Execute(Mob caster, Mob target, World world)
    {
        if (target == null)
            target = caster;

        // Remove existing sanctuary
        target.StatusEffects.RemoveAll(e => e.Type == StatusEffectType.Sanctuary);

        target.ApplyEffect(new StatusEffect.StatusEffect(
            Name,
            StatusEffectType.Sanctuary,
            10,
            caster.Level,
            null,
            new SanctuaryEffectRule()), world);

        //world.Enqueue(new ScriptAction(ctx =>
        //    ctx.Notify($"{target.Name} is surrounded by a white aura.")));

        caster.Wait = 2;
    }
}