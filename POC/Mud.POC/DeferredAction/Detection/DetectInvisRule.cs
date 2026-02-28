using System;
using System.Collections.Generic;
using System.Text;

namespace Mud.POC.DeferredAction.Detection;

public class DetectInvisRule : IDetectionRule
{
    public void Apply(DetectionContext ctx)
    {
        if (!ctx.Target.HasEffect(StatusEffectType.Invisibility))
            return;

        if (ctx.Detector.HasEffect(StatusEffectType.DetectInvis))
            ctx.DetectionChance += 50;
    }
}
