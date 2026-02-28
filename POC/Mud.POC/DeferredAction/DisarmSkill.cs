using System;
using System.Collections.Generic;
using System.Text;

namespace Mud.POC.DeferredAction;

public class DisarmSkill : Skill
{
    public DisarmSkill() : base("disarm", 0, 2) { }

    public override void Execute(Mob attacker, Mob target, World world)
    {
        if (attacker.IsDead || target.IsDead) return;

        if (target.Wielded is null)
            return; // dedicated message ?

        // Simple success check
        bool success = Random.Shared.Next(100) < 50;

        if (!success)
        {
            world.Enqueue(new ScriptAction(ctx =>
                ctx.Notify($"{attacker.Name} fails to disarm {target.Name}!")));
            return;
        }

        var weapon = target.Wielded;
        world.Enqueue(new TransferItemAction(weapon, target.CurrentRoom));
        world.Enqueue(new ScriptAction(ctx =>
            ctx.Notify($"{attacker.Name} disarms {target.Name}, dropping {weapon.Name}!")));
    }
}