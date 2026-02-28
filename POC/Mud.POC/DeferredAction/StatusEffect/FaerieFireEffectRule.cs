using Mud.POC.DeferredAction.Combat;
using Mud.POC.DeferredAction.Detection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mud.POC.DeferredAction.StatusEffect;

public class FaerieFireEffectRule : IStatusEffectRule
{
    public void OnApply(Mob target, World world)
    {
        world.Enqueue(new ScriptAction(ctx =>
            ctx.Notify($"{target.Name} is surrounded by a pink outline!")));
    }

    public void OnTick(Mob target, World world) { }

    public void OnRemove(Mob target, World world)
    {
        world.Enqueue(new ScriptAction(ctx =>
            ctx.Notify($"The pink outline around {target.Name} fades.")));
    }

    public void ModifyCombat(CombatContext ctx)
    {
        if (ctx.Defender.HasEffect(StatusEffectType.FaerieFire))
            ctx.ArmorClass += 100;
    }

    public void ModifyDetection(DetectionContext ctx)
    {
        if (ctx.Target.HasEffect(StatusEffectType.FaerieFire))
            ctx.DetectionChance += 80;
    }
}
