using Mud.POC.DeferredAction.StatusEffect;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mud.POC.DeferredAction.Detection;

public class DetectionEngine
{
    private readonly StatusEffectEngine _statusEffectEngine;
    private readonly List<IDetectionRule> _rules;

    public DetectionEngine(StatusEffectEngine statusEffectEngine, IEnumerable<IDetectionRule> rules)
    {
        _statusEffectEngine = statusEffectEngine;
        _rules = rules.ToList();
    }

    public bool TryDetect(Mob detector, Mob target)
    {
        var ctx = new DetectionContext(detector, target);

        foreach (var rule in _rules)
            rule.Apply(ctx);

        _statusEffectEngine.ApplyDetectionModifiers(ctx);

        int roll = Random.Shared.Next(100);

        ctx.Detected = roll < ctx.DetectionChance;

        return ctx.Detected;
    }
}
