using System;
using System.Collections.Generic;
using System.Text;

namespace Mud.POC.DeferredAction;

public class CombatRoundAction : IGameAction
{
    private readonly Room _room;

    public CombatRoundAction(Room room)
    {
        _room = room;
    }

    public void Execute(World world)
    {
        // Snapshot of mobs in combat
        var participants = _room.Mobs
            .Where(m => m.InCombat && !m.IsDead)
            .ToList();

        foreach (var mob in participants)
        {
            // Skip if mob is hidden or sneaking: chance to be undetected
            if (mob.HasEffect(StatusEffectType.Hidden) || mob.HasEffect(StatusEffectType.Sneak))
            {
                if (Random.Shared.Next(100) < 70) // 70% chance to avoid detection
                    continue;
            }

            // Auto-assist logic: mobs will attack enemy of allies
            foreach (var ally in _room.Mobs.Where(a => a != mob && !a.IsDead))
            {
                if (ally.CurrentTarget != null && ally.CurrentTarget != mob && CanAssist(mob, ally))
                {
                    mob.CurrentTarget ??= ally.CurrentTarget;
                }
            }

            // Perform attacks (multi-hit)
            if (mob.CurrentTarget != null && !mob.CurrentTarget.IsDead)
                world.Enqueue(new MultiHitAction(mob));
        }
    }

    private bool CanAssist(Mob mob, Mob ally)
    {
        // Simple assist rules: not attacking friend, not charmed, ally in combat
        return ally.CurrentTarget != null && !mob.IsDead && !mob.HasEffect(StatusEffectType.Charm);
    }
}
