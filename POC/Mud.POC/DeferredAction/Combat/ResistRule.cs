using System;
using System.Collections.Generic;
using System.Text;

namespace Mud.POC.DeferredAction.Combat;

public class ResistRule : ICombatRule
{
    public void Apply(CombatContext ctx)
    {
        int resist = ctx.Defender.GetResistance(ctx.DamageType);
        ctx.FinalDamage -= ctx.FinalDamage * resist / 100;
    }
}