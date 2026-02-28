using System;
using System.Collections.Generic;
using System.Text;

namespace Mud.POC.DeferredAction.Combat;

public class SanctuaryRule : ICombatRule
{
    public void Apply(CombatContext ctx)
    {
        if (!ctx.IsHit) return;

        if (ctx.Defender.HasEffect(StatusEffectType.Sanctuary))
            ctx.FinalDamage /= 2;
    }
}
