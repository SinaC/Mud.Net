using System;
using System.Collections.Generic;
using System.Text;

namespace Mud.POC.DeferredAction;

public class SpecGuardAction : IGameAction
{
    public void Execute(World world)
    {
        foreach (var guard in world.AllMobsInWorld().Where(m => m.IsNpc && m.NpcFlags.HasFlag(NpcFlags.Guard)))
        {
            if (guard.IsDead)
                continue;

            var criminal = guard.CurrentRoom.Mobs
                .FirstOrDefault(m => m.IsPlayerKiller || m.IsThief);

            if (criminal != null)
            {
                guard.CurrentTarget = criminal;
                guard.Position = Position.Fighting;

                // Enqueue multi-hit attack
                world.Enqueue(new MultiHitAction(guard));

                // Optional: bash if available
                if (guard.HasSkill("bash"))
                {
                    var bashSkill = SkillBook.GetSkillByName("bash");
                    world.Enqueue(new SkillAction(guard, bashSkill, criminal));
                }
            }
        }
    }
}
