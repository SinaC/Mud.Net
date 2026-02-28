using Mud.POC.DeferredAction.Combat;
using Mud.POC.DeferredAction.Detection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mud.POC.DeferredAction.StatusEffect;

public class HideEffectRule : IStatusEffectRule
{
    public void OnApply(Mob target, World world) {
        world.Enqueue(new ScriptAction(ctx =>
            ctx.Notify("You blend into the shadows.")));
    }

    public void OnTick(Mob target, World world) { }

    public void OnRemove(Mob target, World world) { }

    public void ModifyCombat(CombatContext context)
    {
        if (context.Attacker.HasEffect(StatusEffectType.Hidden))
            context.IsBackstab = true;
    }

    public void ModifyDetection(DetectionContext context)
    {
        if (context.Target.HasEffect(StatusEffectType.Hidden))
            context.DetectionChance -= 50;
    }
}
