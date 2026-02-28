using Mud.POC.DeferredAction.StatusEffect;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mud.POC.DeferredAction;

public class InvisibilitySpell : Skill
{
    public InvisibilitySpell()
        : base("invisibility", manaCost: 20, cooldownTicks: 2) { }

    public override void Execute(Mob caster, Mob target, World world)
    {
        if (target == null)
            target = caster;

        target.StatusEffects.RemoveAll(e => e.Type == StatusEffectType.Invisibility);

        target.ApplyEffect(new StatusEffect.StatusEffect(
            Name,
            StatusEffectType.Invisibility,
            20,
            caster.Level,
            null,
            new InvisibilityEffectRule()), world);

        caster.Wait = 2;
    }
}