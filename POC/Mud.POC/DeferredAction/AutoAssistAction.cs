using System;
using System.Collections.Generic;
using System.Text;

namespace Mud.POC.DeferredAction;

public class AutoAssistAction : IGameAction
{
    public void Execute(World world)
    {
        foreach (var mob in world.AllMobsInWorld().Where(m => !m.IsDead))
        {
            foreach (var ally in mob.CurrentRoom.Mobs.Where(a => a.IsNpc && a.NpcFlags.HasFlag(NpcFlags.Guard)))
            {
                if (ally.InCombat)
                    continue;

                var target = mob.CurrentRoom.Mobs
                    .FirstOrDefault(m => m.CurrentTarget == mob); // someone attacking ally

                if (target != null)
                {
                    ally.CurrentTarget = target;
                    ally.Position = Position.Fighting;
                    world.Enqueue(new MultiHitAction(ally));
                    world.Enqueue(new ScriptAction(ctx =>
                        ctx.Notify($"{ally.Name} rushes to assist {mob.Name}!")));
                }
            }
        }
    }
}
