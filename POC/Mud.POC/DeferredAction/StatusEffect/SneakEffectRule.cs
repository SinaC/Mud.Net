using Mud.POC.DeferredAction.Combat;
using Mud.POC.DeferredAction.Detection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mud.POC.DeferredAction.StatusEffect;

public class SneakEffectRule : IStatusEffectRule
{
    public void OnApply(Mob target, World world)
    {
        world.Enqueue(new ScriptAction(ctx =>
            ctx.Notify($"{target.Name} moves silently.")));
    }

    public void OnTick(Mob target, World world) { }

    public void OnRemove(Mob target, World world)
    {
        world.Enqueue(new ScriptAction(ctx =>
            ctx.Notify($"{target.Name} is no longer sneaking.")));
    }

    public void ModifyCombat(CombatContext context) { }

    public void ModifyDetection(DetectionContext context)
    {
        if (context.Target.HasEffect(StatusEffectType.Sneak))
            context.DetectionChance -= 30;
    }
}
