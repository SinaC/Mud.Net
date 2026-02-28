using Mud.POC.DeferredAction.Combat;
using Mud.POC.DeferredAction.Detection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mud.POC.DeferredAction.StatusEffect;

public class DetectHiddenEffectRule : IStatusEffectRule
{
    public void OnApply(Mob target, World world)
    {
        world.Enqueue(new ScriptAction(ctx =>
            ctx.Notify("Your awareness sharpens.")));
    }

    public void OnTick(Mob target, World world) { }

    public void OnRemove(Mob target, World world) {
        world.Enqueue(new ScriptAction(ctx =>
                ctx.Notify("You feel less aware of your surroundings.")));
    }

    public void ModifyCombat(CombatContext context) { }

    public void ModifyDetection(DetectionContext context)
    {
        if (context.Detector.HasEffect(StatusEffectType.DetectHidden))
            context.DetectionChance += 40;
    }
}
