using System;
using System.Collections.Generic;
using System.Text;

namespace Mud.POC.DeferredAction.Detection;

public class DetectHiddenRule : IDetectionRule
{
    public void Apply(DetectionContext ctx)
    {
        if (!ctx.Target.HasEffect(StatusEffectType.Hidden))
            return;

        if (ctx.Detector.HasEffect(StatusEffectType.DetectHidden))
            ctx.DetectionChance += 40;
    }
}
