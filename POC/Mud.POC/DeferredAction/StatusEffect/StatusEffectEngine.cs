using Mud.POC.DeferredAction.Combat;
using Mud.POC.DeferredAction.Detection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mud.POC.DeferredAction.StatusEffect;

public class StatusEffectEngine
{
    public void Tick(World world)
    {
        foreach (var mob in world.AllMobsInWorld())
        {
            foreach (var effect in mob.StatusEffects.ToList())
            {
                effect.Tick(mob, world);

                if (effect.IsExpired)
                {
                    effect.Expire(mob, world);
                    mob.StatusEffects.Remove(effect);
                }
            }
        }
    }

    public void ApplyCombatModifiers(CombatContext ctx)
    {
        foreach (var effect in ctx.Attacker.StatusEffects)
            effect.Rule.ModifyCombat(ctx);

        foreach (var effect in ctx.Defender.StatusEffects)
            effect.Rule.ModifyCombat(ctx);
    }

    public void ApplyDetectionModifiers(DetectionContext ctx)
    {
        foreach (var effect in ctx.Detector.StatusEffects)
            effect.Rule.ModifyDetection(ctx);
    }
}
