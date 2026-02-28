using System;
using System.Collections.Generic;
using System.Text;

namespace Mud.POC.DeferredAction;

public class BashSkill : Skill
{
    public BashSkill() : base("bash", 0, cooldownTicks: 2) { }

    public override void Execute(Mob caster, Mob target, World world)
    {
        if (target == null || target.IsDead)
            return;

        caster.Wait = 2;

        if (Random.Shared.Next(100) < 60) // success %
        {
            int damage = 5; // TODO
            world.Enqueue(new ApplyDamageAction(caster, target, damage));

            world.Enqueue(new ScriptAction(ctx =>
            {
                target.Position = Position.Stunned;
                target.Wait = 2;
                ctx.Notify($"{caster.Name} bashes {target.Name} to the ground!");
            }));
        }
        else
        {
            caster.Position = Position.Stunned;
            caster.Wait = 2;
        }
    }
}
