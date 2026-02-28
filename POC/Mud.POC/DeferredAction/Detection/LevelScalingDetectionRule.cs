using System;
using System.Collections.Generic;
using System.Text;

namespace Mud.POC.DeferredAction.Detection;

public class LevelScalingDetectionRule : IDetectionRule
{
    public void Apply(DetectionContext ctx)
    {
        int levelDiff = ctx.Detector.Level - ctx.Target.Level;

        ctx.DetectionChance += 50 + (levelDiff * 3);
    }
}
