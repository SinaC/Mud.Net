using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Channels;

namespace Mud.POC.DeferredAction.Detection;

public class DetectSneakRule : IDetectionRule
{
    public void Apply(DetectionContext ctx)
    {
        if (!ctx.Target.HasEffect(StatusEffectType.Sneak))
            return;

        if (ctx.Detector.HasEffect(StatusEffectType.DetectHidden))
        {
            int sneakSkill = ctx.Target.GetSkillPercent("sneak");
            ctx.DetectionChance -= sneakSkill / 2;
        }
    }
}
