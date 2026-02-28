using Mud.POC.DeferredAction.StatusEffect;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mud.POC.DeferredAction;

public class DetectInvisSpell : Skill
{
    public DetectInvisSpell()
        : base("detect invis", manaCost: 5, cooldownTicks: 1) { }

    public override void Execute(Mob caster, Mob target, World world)
    {
        caster.ApplyEffect(new StatusEffect.StatusEffect(
            Name,
            StatusEffectType.DetectInvis,
            20,
            caster.Level,
            null,
            new DetectInvisEffectRule()), world);
    }
}