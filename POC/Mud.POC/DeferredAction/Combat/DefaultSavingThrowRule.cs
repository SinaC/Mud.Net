using System;
using System.Collections.Generic;
using System.Text;

namespace Mud.POC.DeferredAction.Combat;

public class DefaultSavingThrowRule : ISavingThrowRule
{
    public bool CheckSave(CombatContext ctx)
    {
        int levelDiff = ctx.Attacker.Level - ctx.Defender.Level;

        int baseSave = ctx.Defender.GetSave(ctx.SaveType ?? SaveType.Spell);

        int roll = Random.Shared.Next(1, 101);

        int adjusted = baseSave - (levelDiff * 2);

        return roll < adjusted;
    }
}
