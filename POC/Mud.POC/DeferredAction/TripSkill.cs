using System;
using System.Collections.Generic;
using System.Text;

namespace Mud.POC.DeferredAction;

public class TripSkill : Skill
{
    public TripSkill() : base("trip", 0, cooldownTicks: 2) { }

    public override void Execute(Mob caster, Mob target, World world)
    {
        if (target == null || target.IsDead)
            return;

        caster.Wait = 2;

        if (Random.Shared.Next(100) < 65)
        {
            world.Enqueue(new ScriptAction(ctx =>
            {
                target.Position = Position.Resting;
                target.Wait = 2;
                ctx.Notify($"{caster.Name} trips {target.Name}!");
            }));
        }
        else
        {
            world.Enqueue(new ScriptAction(ctx =>
                ctx.Notify($"{caster.Name} fails to trip {target.Name}.")));
        }
    }
}
