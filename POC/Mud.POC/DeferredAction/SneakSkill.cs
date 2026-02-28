using Mud.POC.DeferredAction.StatusEffect;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mud.POC.DeferredAction;

public class SneakSkill : Skill
{
    public SneakSkill() : base("sneak", manaCost: 0, cooldownTicks: 2) { }

    public override void Execute(Mob caster, Mob target, World world)
    {
        caster.StatusEffects.RemoveAll(e => e.Type == StatusEffectType.Sneak);

        int skill = caster.GetSkillPercent("sneak");
        int roll = Random.Shared.Next(100);

        if (roll < skill)
        {
            caster.ApplyEffect(new StatusEffect.StatusEffect(
                "Sneak",
                StatusEffectType.Sneak,
                10,
                caster.Level,
                null,
                new SneakEffectRule()), world);
        }
        else
        {
            world.Enqueue(new ScriptAction(ctx =>
                ctx.Notify("You fail to move silently.")));

            caster.CheckImprove("sneak", false);
        }

        caster.Wait = 2;
    }
}
//public class SneakSkill : Skill
//{
//    public SneakSkill() : base("sneak", 0, 0) { }

//    public override void Execute(Mob caster, Mob target, World world)
//    {
//        caster.Wait = 1;
//        caster.StatusEffects.Add(new StatusEffect("Sneaking", 5));
//        world.Enqueue(new ScriptAction(ctx =>
//            ctx.Notify($"{caster.Name} moves silently.")));
//    }
//}
