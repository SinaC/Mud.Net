using System;
using System.Collections.Generic;
using System.Text;

namespace Mud.POC.DeferredAction;

public class NpcUpdateAction : IGameAction
{
    public void Execute(World world)
    {
        foreach (var mob in world.AllMobsInWorld().ToList())
        {
            if (!mob.IsNpc || mob.IsDead)
                continue;

            if (mob.InCombat)
                continue;

            HandleAggression(world, mob);
            HandleScavenge(world, mob);
            HandleMemory(world, mob);
            HandleGuard(world, mob);
        }
    }

    private void HandleGuard(World world, Mob npc)
    {
        if (!npc.NpcFlags.HasFlag(NpcFlags.Guard))
            return;

        var criminal = npc.CurrentRoom.Mobs
            .FirstOrDefault(m => m.IsPlayerKiller || m.IsThief);

        if (criminal != null)
        {
            npc.CurrentTarget = criminal;
            npc.Position = Position.Fighting;
            world.Enqueue(new MultiHitAction(npc));
        }
    }

    private void HandleAggression(World world, Mob npc)
    {
        if (!npc.NpcFlags.HasFlag(NpcFlags.Aggressive))
            return;

        var target = npc.CurrentRoom.Mobs
            .FirstOrDefault(m => !m.IsNpc && !m.IsDead);

        if (target != null)
        {
            npc.CurrentTarget = target;
            npc.Position = Position.Fighting;
            world.Enqueue(new MultiHitAction(npc));
        }
    }

    private void HandleScavenge(World world, Mob npc)
    {
        if (!npc.NpcFlags.HasFlag(NpcFlags.Scavenger))
            return;

        var item = npc.CurrentRoom.Items.FirstOrDefault();
        if (item != null)
        {
            world.Enqueue(new TransferItemAction(item, npc));
        }
    }

    private void HandleMemory(World world, Mob npc)
    {
        if (!npc.NpcFlags.HasFlag(NpcFlags.Memory))
            return;

        var hated = npc.CurrentRoom.Mobs
            .FirstOrDefault(m => npc.HateList.Contains(m) && !m.IsDead);

        if (hated != null)
        {
            npc.CurrentTarget = hated;
            npc.Position = Position.Fighting;
        }
    }
}
