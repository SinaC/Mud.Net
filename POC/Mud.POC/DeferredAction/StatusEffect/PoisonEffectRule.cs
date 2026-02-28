using Mud.POC.DeferredAction.Combat;
using Mud.POC.DeferredAction.Detection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mud.POC.DeferredAction.StatusEffect;

public class PoisonEffectRule : IStatusEffectRule
{
    public void OnApply(Mob target, World world)
    {
        world.Enqueue(new ScriptAction(ctx =>
            ctx.Notify($"{target.Name} is poisoned!")));
    }

    public void OnTick(Mob target, World world)
    {
        int damage = Math.Max(1, target.Level / 2);

        var ctx = world.CombatEngine.ResolveSpell(
            target,
            target,
            SkillBook.GetSkillByName("Poison"),
            damage,
            SaveType.Poison);

        world.Enqueue(new DamageAction(null, target, ctx.FinalDamage));
    }

    public void OnRemove(Mob target, World world)
    {
        world.Enqueue(new ScriptAction(ctx =>
            ctx.Notify($"{target.Name} looks healthier.")));
    }

    public void ModifyCombat(CombatContext ctx)
    {
        if (ctx.Attacker.HasEffect(StatusEffectType.Poison))
            ctx.HitRoll -= 2;
    }

    public void ModifyDetection(DetectionContext ctx) { }
}
