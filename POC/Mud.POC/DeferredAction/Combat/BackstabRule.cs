using System;
using System.Collections.Generic;
using System.Text;

namespace Mud.POC.DeferredAction.Combat;

public class BackstabRule : ICombatRule
{
    public void Apply(CombatContext ctx)
    {
        if (!ctx.IsBackstab) return;

        int multiplier = 2 + ctx.Attacker.Level / 10;
        ctx.BaseDamage *= multiplier;
    }
}
