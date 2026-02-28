using Mud.POC.DeferredAction.Combat;
using Mud.POC.DeferredAction.Detection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mud.POC.DeferredAction.StatusEffect;

public class SanctuaryEffectRule : IStatusEffectRule
{
    public void OnApply(Mob target, World world)
    {
        world.Enqueue(new ScriptAction(ctx =>
            ctx.Notify($"{target.Name} is surrounded by a white aura.")));
    }

    public void OnTick(Mob target, World world) { }

    public void OnRemove(Mob target, World world)
    {
        world.Enqueue(new ScriptAction(ctx =>
            ctx.Notify("The white aura fades.")));
    }

    public void ModifyCombat(CombatContext ctx)
    {
        if (ctx.Defender.HasEffect(StatusEffectType.Sanctuary))
            ctx.ModifiedDamage /= 2;
    }

    public void ModifyDetection(DetectionContext context) { }
}
