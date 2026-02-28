namespace Mud.POC.DeferredAction;

public class DetectionAction : IGameAction
{
    public void Execute(World world)
    {
        foreach (var detector in world.AllMobsInWorld())
        {
            foreach (var target in detector.CurrentRoom.Mobs)
            {
                TryDetect(world, detector, target);
            }
        }
    }

    private void TryDetect(World world, Mob detector, Mob victim)
    {
        if (victim == detector)
            return;

        if (!victim.IsStealthed())
            return;

        if (world.DetectionEngine.TryDetect(detector, victim))
        {
            victim.RemoveStealth();

            world.Enqueue(new ScriptAction(ctx =>
                ctx.Notify($"{detector.Name} detects {victim.Name}!")));

            // --------------------------------------------------
            // GUARD LOGIC (ROM style)
            // --------------------------------------------------
            if (victim.NpcFlags.HasFlag(NpcFlags.Guard) &&
                (detector.IsPlayerKiller || detector.IsThief))
            {
                Engage(world, victim, detector);
                CallGuardAssist(world, victim, detector);
                return;
            }

            // --------------------------------------------------
            // AGGRESSIVE NPC LOGIC
            // --------------------------------------------------
            if (victim.NpcFlags.HasFlag(NpcFlags.Aggressive) && !detector.IsNpc)
            {
                Engage(world, victim, detector);
                return;
            }
        }
    }

    private void Engage(World world, Mob npc, Mob target)
    {
        npc.CurrentTarget = target;
        npc.Position = Position.Fighting;

        world.Enqueue(new ScriptAction(ctx =>
            ctx.Notify($"{npc.Name} attacks {target.Name}!")));

        world.Enqueue(new MultiHitAction(npc));
    }

    private void CallGuardAssist(World world, Mob caller, Mob target)
    {
        foreach (var guard in caller.CurrentRoom.Mobs
                     .Where(m => m.IsNpc &&
                                 m != caller &&
                                 m.NpcFlags.HasFlag(NpcFlags.Guard) &&
                                 !m.InCombat &&
                                 !m.IsDead))
        {
            guard.CurrentTarget = target;
            guard.Position = Position.Fighting;

            world.Enqueue(new ScriptAction(ctx =>
                ctx.Notify($"{guard.Name} rushes to assist {caller.Name}!")));

            world.Enqueue(new MultiHitAction(guard));
        }
    }
}

/*
public class DetectionAction : IGameAction
{
    public void Execute(World world)
    {
        foreach (var npc in world.AllMobsInWorld()
                                 .Where(m => m.IsNpc && !m.IsDead))
        {
            var room = npc.CurrentRoom;
            if (room == null) continue;

            foreach (var victim in room.Mobs.Where(m => m != npc && !m.IsDead))
            {
                TryDetect(world, npc, victim);
            }
        }
    }

    private void TryDetect(World world, Mob detector, Mob victim)
    {
        var isHidden = victim.HasEffect(StatusEffectType.Hidden);
        var isSneaking = victim.HasEffect(StatusEffectType.Sneak);
        var isInvis = victim.HasEffect(StatusEffectType.Invisibility);

        if (!isHidden && !isSneaking && !isInvis)
            return;

        int chance = CalculateDetectionChance(detector, victim);

        // AFF_DETECT_HIDDEN bonus
        if (isHidden &&
            detector.HasEffect(StatusEffectType.DetectHidden))
        {
            chance += 50;
        }

        // AFF_DETECT_INVIS bonus
        if (isInvis &&
            detector.HasEffect(StatusEffectType.DetectInvis))
        {
            chance += 70;
        }

        // Sneak skill reduces detection chance
        if (isSneaking)
        {
            int sneakSkill = victim.GetSkillPercent("sneak");
            chance -= sneakSkill / 2;
        }

        int roll = Random.Shared.Next(100);
        if (roll >= chance)
            return;

        // Remove stealth effects
        if (isHidden) victim.StatusEffects.RemoveAll(x => x.Type == StatusEffectType.Hidden);
        if (isSneaking) victim.StatusEffects.RemoveAll(x => x.Type == StatusEffectType.Sneak);
        if (isInvis) victim.StatusEffects.RemoveAll(x => x.Type == StatusEffectType.Invisibility);

        world.Enqueue(new ScriptAction(ctx =>
            ctx.Notify($"{detector.Name} detects {victim.Name}!")));

        // --------------------------------------------------
        // GUARD LOGIC (ROM style)
        // --------------------------------------------------
        if (victim.NpcFlags.HasFlag(NpcFlags.Guard) &&
            (detector.IsPlayerKiller || detector.IsThief))
        {
            Engage(world, victim, detector);
            CallGuardAssist(world, victim, detector);
            return;
        }

        // --------------------------------------------------
        // AGGRESSIVE NPC LOGIC
        // --------------------------------------------------
        if (victim.NpcFlags.HasFlag(NpcFlags.Aggressive) && !detector.IsNpc)
        {
            Engage(world, victim, detector);
            return;
        }
    }

    private int CalculateDetectionChance(Mob detector, Mob victim)
    {
        int baseChance = 25;

        int levelDiff = detector.Level - victim.Level;

        baseChance += levelDiff * 3; // higher level sees easier

        if (detector.HasEffect(StatusEffectType.DetectHidden))
            baseChance += 40;

        if (detector.HasEffect(StatusEffectType.DetectInvis))
            baseChance += 60;

        if (victim.HasEffect(StatusEffectType.Sneak))
        {
            int sneakSkill = victim.GetSkillPercent("sneak");
            baseChance -= sneakSkill / 2;
        }

        return Math.Clamp(baseChance, 5, 95);
    }

    private void Engage(World world, Mob npc, Mob target)
    {
        npc.CurrentTarget = target;
        npc.Position = Position.Fighting;

        world.Enqueue(new ScriptAction(ctx =>
            ctx.Notify($"{npc.Name} attacks {target.Name}!")));

        world.Enqueue(new MultiHitAction(npc));
    }

    private void CallGuardAssist(World world, Mob caller, Mob target)
    {
        foreach (var guard in caller.CurrentRoom.Mobs
                     .Where(m => m.IsNpc &&
                                 m != caller &&
                                 m.NpcFlags.HasFlag(NpcFlags.Guard) &&
                                 !m.InCombat &&
                                 !m.IsDead))
        {
            guard.CurrentTarget = target;
            guard.Position = Position.Fighting;

            world.Enqueue(new ScriptAction(ctx =>
                ctx.Notify($"{guard.Name} rushes to assist {caller.Name}!")));

            world.Enqueue(new MultiHitAction(guard));
        }
    }
}
*/
/*
public class DetectionAction : IGameAction
{
    private readonly int _baseDetectionChance;

    /// <summary>
    /// Base detection chance in percent (0-100)
    /// </summary>
    public DetectionAction(int baseDetectionChance = 50)
    {
        _baseDetectionChance = baseDetectionChance;
    }

    public void Execute(World world)
    {
        foreach (var npc in world.AllMobsInWorld().Where(m => m.IsNpc && !m.IsDead))
        {
            foreach (var mob in npc.CurrentRoom.Mobs.ToList())
            {
                if (mob == npc || mob.IsDead)
                    continue;

                // Check if mob has Hidden or Sneak status effect
                var hasHiddenEffect = StatusEffect.Has(mob, "Hide");
                var hasSneakEffect = StatusEffect.Has(mob, "Sneak");

                if (!hasHiddenEffect && !hasSneakEffect)
                    continue; // mob is not hidden or sneaking

                // Roll detection
                int roll = Random.Shared.Next(100);
                if (roll >= _baseDetectionChance)
                    continue; // failed detection

                // Remove the effects
                if (hasHiddenEffect)
                    StatusEffect.Remove(mob, "Hide");
                if (hasSneakEffect)
                    StatusEffect.Remove(mob, "Sneak");

                world.Enqueue(new ScriptAction(ctx =>
                    ctx.Notify($"{npc.Name} spots {mob.Name}!")));

                // --------------------------------------------------
                // GUARD LOGIC (ROM style)
                // --------------------------------------------------
                if (npc.NpcFlags.HasFlag(NpcFlags.Guard) &&
                    (mob.IsPlayerKiller || mob.IsThief))
                {
                    Engage(world, npc, mob);
                    CallGuardAssist(world, npc, mob);
                    continue;
                }

                // --------------------------------------------------
                // AGGRESSIVE NPC LOGIC
                // --------------------------------------------------
                if (npc.NpcFlags.HasFlag(NpcFlags.Aggressive) && !mob.IsNpc)
                {
                    Engage(world, npc, mob);
                    continue;
                }
            }
        }
    }

    private void Engage(World world, Mob npc, Mob target)
    {
        npc.CurrentTarget = target;
        npc.Position = Position.Fighting;

        world.Enqueue(new ScriptAction(ctx =>
            ctx.Notify($"{npc.Name} attacks {target.Name}!")));

        world.Enqueue(new MultiHitAction(npc));
    }

    private void CallGuardAssist(World world, Mob caller, Mob target)
    {
        foreach (var guard in caller.CurrentRoom.Mobs
                     .Where(m => m.IsNpc &&
                                 m != caller &&
                                 m.NpcFlags.HasFlag(NpcFlags.Guard) &&
                                 !m.InCombat &&
                                 !m.IsDead))
        {
            guard.CurrentTarget = target;
            guard.Position = Position.Fighting;

            world.Enqueue(new ScriptAction(ctx =>
                ctx.Notify($"{guard.Name} rushes to assist {caller.Name}!")));

            world.Enqueue(new MultiHitAction(guard));
        }
    }
}
*/