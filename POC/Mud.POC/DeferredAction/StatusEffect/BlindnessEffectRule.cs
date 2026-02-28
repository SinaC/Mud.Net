using Mud.POC.DeferredAction.Combat;
using Mud.POC.DeferredAction.Detection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mud.POC.DeferredAction.StatusEffect;

public class BlindnessEffectRule : IStatusEffectRule
{
    public void OnApply(Mob target, World world)
    {
        world.Enqueue(new ScriptAction(ctx =>
            ctx.Notify($"{target.Name} is blinded!")));
    }

    public void OnTick(Mob target, World world) { }

    public void OnRemove(Mob target, World world)
    {
        world.Enqueue(new ScriptAction(ctx =>
            ctx.Notify($"{target.Name} can see again.")));
    }

    public void ModifyCombat(CombatContext ctx)
    {
        if (ctx.Attacker.HasEffect(StatusEffectType.Blindness))
            ctx.HitRoll -= 20;
    }

    public void ModifyDetection(DetectionContext ctx)
    {
        if (ctx.Detector.HasEffect(StatusEffectType.Blindness))
            ctx.DetectionChance -= 50;
    }
}
