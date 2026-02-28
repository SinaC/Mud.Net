using Mud.POC.DeferredAction.Combat;
using Mud.POC.DeferredAction.Detection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mud.POC.DeferredAction.StatusEffect;

public class InvisibilityEffectRule : IStatusEffectRule
{
    public void OnApply(Mob target, World world) {
        world.Enqueue(new ScriptAction(ctx =>
           ctx.Notify("You fade out of existence.")));
        //fades out of sight. for item
    }

    public void OnTick(Mob target, World world) { }

    public void OnRemove(Mob target, World world) {
        world.Enqueue(new ScriptAction(ctx =>
            ctx.Notify("You are no longer invisible.")));
    }

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
