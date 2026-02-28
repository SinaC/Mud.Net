using System;
using System.Collections.Generic;
using System.Text;

namespace Mud.POC.DeferredAction;

public class ViolenceUpdateAction : IGameAction
{
    public void Execute(World world)
    {
        // Snapshot rooms to avoid mutation issues
        var rooms = world.Rooms.ToList();

        foreach (var room in rooms)
        {
            // Snapshot participants in this room
            var fighters = room.Mobs
                .Where(m => m.InCombat && !m.IsDead)
                .ToList();

            foreach (var mob in fighters)
            {
                var target = mob.CurrentTarget;

                // ROM behavior: stop fighting if invalid
                if (target == null ||
                    target.IsDead ||
                    target.CurrentRoom != mob.CurrentRoom)
                {
                    mob.CurrentTarget = null;
                    continue;
                }

                // Enqueue multi-attack logic (ROM multi_hit equivalent)
                world.Enqueue(new MultiHitAction(mob));
            }
        }
    }
}
