using Mud.POC.DeferredAction.Combat;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mud.POC.DeferredAction;

public class BackstabSkill : Skill
{
    public BackstabSkill()
        : base("backstab", manaCost: 0, cooldownTicks: 2) { }

    public override void Execute(Mob caster, Mob target, World world)
    {
        if (target == null || target.IsDead)
            return;

        if (caster.InCombat)
        {
            world.Enqueue(new ScriptAction(ctx =>
                ctx.Notify("You cannot backstab while fighting.")));
            return;
        }

        if (!caster.IsWieldingDagger())
        {
            world.Enqueue(new ScriptAction(ctx =>
                ctx.Notify("You must wield a dagger to backstab.")));
            return;
        }

        if (target.CurrentTarget == caster)
        {
            world.Enqueue(new ScriptAction(ctx =>
                ctx.Notify("They are too alert to backstab.")));
            return;
        }

        int skill = caster.GetSkillPercent("backstab");
        int roll = Random.Shared.Next(100);

        if (roll > skill)
        {
            world.Enqueue(new ScriptAction(ctx =>
                ctx.Notify("You fail to land a proper backstab!")));

            caster.Wait = 2;
            caster.CheckImprove("backstab", false);
            return;
        }

        int baseDamage = 100;
        world.Enqueue(new ApplyDamageAction(caster, target, baseDamage));

        caster.BreakStealth(world);

        caster.CurrentTarget = target;
        caster.Position = Position.Fighting;

        caster.CheckImprove("backstab", true);
    }
}
