using System;
using System.Collections.Generic;
using System.Text;

namespace Mud.POC.DeferredAction;

public class RescueSkill : Skill
{
    public RescueSkill() : base("rescue", 0, 2) { }

    public override void Execute(Mob rescuer, Mob target, World world)
    {
        if (!target.InCombat)
            return;

        if (rescuer.IsDead || target.IsDead) return;
        if (target.CurrentTarget == null) return;


        var enemy = target.CurrentTarget;
        if (enemy == null)
            return;

        if (Random.Shared.Next(100) < 70)
        {
            enemy.CurrentTarget = rescuer;
            rescuer.CurrentTarget = enemy;

            rescuer.Position = Position.Fighting;
            rescuer.Wait = 2;

            world.Enqueue(new ScriptAction(ctx =>
                ctx.Notify($"{rescuer.Name} rescues {target.Name}, taking the fight to {enemy.Name}!")));
        }
    }
}
