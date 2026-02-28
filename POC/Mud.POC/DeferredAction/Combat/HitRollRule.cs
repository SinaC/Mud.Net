using System;
using System.Collections.Generic;
using System.Text;

namespace Mud.POC.DeferredAction.Combat;

public class HitRollRule : ICombatRule
{
    public void Apply(CombatContext ctx)
    {
        ctx.HitRoll = ctx.Attacker.BaseHitRoll
                      + ctx.Attacker.GetTotalModifiers().HitRollBonus;

        ctx.ArmorClass = ctx.Defender.BaseArmorClass
                          + ctx.Defender.GetTotalModifiers().ArmorClassBonus;

        int roll = Random.Shared.Next(1, 21);

        ctx.IsHit = (roll + ctx.HitRoll) > ctx.ArmorClass;
    }
}
