using Mud.POC.DeferredAction.Combat;
using Mud.POC.DeferredAction.Detection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mud.POC.DeferredAction.StatusEffect;

public class DetectInvisEffectRule : IStatusEffectRule
{
    public void OnApply(Mob target, World world)
    {
        world.Enqueue(new ScriptAction(ctx =>
            ctx.Notify("Your awareness sharpens.")));
    }

    public void OnTick(Mob target, World world) { }

    public void OnRemove(Mob target, World world) {
        world.Enqueue(new ScriptAction(ctx =>
                ctx.Notify("You no longer see invisible objects.")));
    }

    public void ModifyCombat(CombatContext context) { }

    public void ModifyDetection(DetectionContext context)
    {
        if (context.Detector.HasEffect(StatusEffectType.DetectInvis))
            context.DetectionChance += 40;
    }
}
