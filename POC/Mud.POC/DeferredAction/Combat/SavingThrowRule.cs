using System;
using System.Collections.Generic;
using System.Text;

namespace Mud.POC.DeferredAction.Combat;

public class SavingThrowRule : ICombatRule
{
    private readonly ISavingThrowRule _saveRule;

    public SavingThrowRule(ISavingThrowRule saveRule)
    {
        _saveRule = saveRule;
    }

    public void Apply(CombatContext ctx)
    {
        if (!ctx.IsSpell || ctx.SaveType == null)
            return;

        if (_saveRule.CheckSave(ctx))
        {
            ctx.ModifiedDamage /= 2;
        }
    }
}
