using System;
using System.Collections.Generic;
using System.Text;

namespace Mud.POC.DeferredAction.Combat;

public class FaerieFireRule : ICombatRule
{
    public void Apply(CombatContext ctx)
    {
        if (ctx.Defender.HasEffect(StatusEffectType.FaerieFire))
            ctx.ArmorClass += 100;
    }
}
