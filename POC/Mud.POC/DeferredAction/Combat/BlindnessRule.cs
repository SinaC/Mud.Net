using System;
using System.Collections.Generic;
using System.Text;

namespace Mud.POC.DeferredAction.Combat;

public class BlindnessRule : ICombatRule
{
    public void Apply(CombatContext ctx)
    {
        if (ctx.Attacker.HasEffect(StatusEffectType.Blindness))
            ctx.HitRoll -= 20;
    }
}
