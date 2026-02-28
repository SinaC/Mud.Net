using System;
using System.Collections.Generic;
using System.Text;

namespace Mud.POC.DeferredAction.Combat;

public class SpellDamageRule : ICombatRule
{
    public void Apply(CombatContext ctx)
    {
        if (!ctx.IsSpell)
            return;

        ctx.ModifiedDamage = ctx.BaseDamage;
    }
}
