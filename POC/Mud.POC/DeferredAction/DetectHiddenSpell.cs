using Mud.POC.DeferredAction.StatusEffect;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mud.POC.DeferredAction;

public class DetectHiddenSpell : Skill
{
    public DetectHiddenSpell()
        : base("detect hidden", manaCost: 5, cooldownTicks: 1) { }

    public override void Execute(Mob caster, Mob target, World world)
    {
        caster.ApplyEffect(new StatusEffect.StatusEffect(
            Name,
            StatusEffectType.DetectHidden,
            20,
            caster.Level,
            null,
            new DetectHiddenEffectRule()), world);
    }
}