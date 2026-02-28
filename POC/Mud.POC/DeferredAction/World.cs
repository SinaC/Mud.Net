using Mud.Flags;
using Mud.POC.DeferredAction.Combat;
using Mud.POC.DeferredAction.Detection;
using Mud.POC.DeferredAction.StatusEffect;

namespace Mud.POC.DeferredAction;

public class World
{
    private readonly Queue<IGameAction> _actionQueue = new();

    private readonly List<Action> _pendingMutations = new();

    // Tracks who attacked whom during this tick
    private readonly Dictionary<Mob, HashSet<Mob>> _recentAttackers = new();

    public int CurrentTick { get; private set; } = 0;
    public List<Room> Rooms { get; } = new();

    public CombatEngine CombatEngine { get; }
    public DetectionEngine DetectionEngine { get; }
    public StatusEffectEngine StatusEffectEngine { get; }

    public World()
    {

        StatusEffectEngine = new StatusEffectEngine();

        CombatEngine = new CombatEngine(StatusEffectEngine,
            preHit: new ICombatRule[]
            {
                new BlindnessRule(),
                new HitRollRule(),
                new FaerieFireRule()
            },
            postHit: Array.Empty<ICombatRule>(),
            damage: new ICombatRule[]
            {
                new BackstabRule(),
                new SanctuaryRule(),
                new ResistRule(),
                new SavingThrowRule(new DefaultSavingThrowRule())
            });

        DetectionEngine = new DetectionEngine(StatusEffectEngine,
            new IDetectionRule[]
        {
            new LevelScalingDetectionRule(),
            new DetectHiddenRule(),
            new DetectInvisRule()
        });
    }

    public void Enqueue(IGameAction action)
    {
        _actionQueue.Enqueue(action);
    }

    public bool IsPkAllowedRoom(Room room)
    {
        return room.Flags.HasFlag(RoomFlags.PkAllowed);
    }

    public void CheckKiller(Mob attacker, Mob victim)
    {
        if (!attacker.IsPlayer || !victim.IsPlayer)
            return;

        if (attacker == victim)
            return;

        if (IsPkAllowedRoom(attacker.CurrentRoom))
            return;

        if (attacker.IsSameGroup(victim))
            return;

        // Self-defense: victim was already attacking attacker
        if (victim.CurrentTarget == attacker)
            return;

        // If victim already criminal, free to attack
        if (victim.IsPlayerKiller || victim.IsThief)
            return;

        attacker.PlayerFlags |= PlayerFlags.Killer;

        Enqueue(new ScriptAction(ctx =>
            ctx.Notify($"{attacker.Name} is now flagged as a KILLER!")));
    }

    // Record an attack for XP purposes
    public void RecordAttack(Mob attacker, Mob victim)
    {
        if (!_recentAttackers.TryGetValue(victim, out var attackers))
        {
            attackers = new HashSet<Mob>();
            _recentAttackers[victim] = attackers;
        }
        attackers.Add(attacker);
    }

    // Returns a snapshot of mobs who attacked a victim this tick
    public List<Mob> GetMobsWhoAttacked(Mob victim)
    {
        if (_recentAttackers.TryGetValue(victim, out var attackers))
            return attackers.ToList();
        return new List<Mob>();
    }

    public IEnumerable<Mob> AllMobsInWorld()
    {
        foreach (var room in Rooms)
            foreach (var mob in room.Mobs)
                yield return mob;
    }

    public Mob FindMobInRoom(Room room, string name)
    {
        return room.Mobs.FirstOrDefault(m => m.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    public Room FindRoom(string name)
    {
        return Rooms.FirstOrDefault(r => r.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    // Clear any mobs who were targeting this mob
    public void ClearDeadMobTargets(Mob deadMob)
    {
        foreach (var attacker in GetMobsWhoAttacked(deadMob))
        {
            if (attacker.CurrentTarget == deadMob)
                attacker.CurrentTarget = null;
        }
    }

    // Call at the end of the tick to clear attack history
    private void ClearAttackHistory()
    {
        _recentAttackers.Clear();
    }

    // ---- Scheduling ----

    public void ScheduleMutation(Action action)
    {
        _pendingMutations.Add(action);
    }

    public void ScheduleAddMob(Room room, Mob newMob)
    {
        _pendingMutations.Add(() =>
        {
            room.Mobs.Add(newMob);
            newMob.CurrentRoom = room;
        });
    }

    public void ScheduleAddRoom(Room newRoom)
    {
        _pendingMutations.Add(() => Rooms.Add(newRoom));
    }

    public void ScheduleMove(Mob mob, Room newRoom)
    {
        _pendingMutations.Add(() =>
        {
            mob.CurrentRoom?.Mobs.Remove(mob);
            newRoom.Mobs.Add(mob);
            mob.CurrentRoom = newRoom;
        });
    }

    public void ScheduleRemoveMob(Mob mob)
    {
        _pendingMutations.Add(() =>
        {
            mob.CurrentRoom?.Mobs.Remove(mob);
            mob.IsDeleted = true;
        });
    }

    public void ScheduleItemTransfer(Item item, IContainer newContainer)
    {
        _pendingMutations.Add(() =>
        {
            item.Container?.Items.Remove(item);
            newContainer.Items.Add(item);
            item.Container = newContainer;
        });
    }

    // ---- Tick ----
    public void Tick()
    {
        foreach (var mob in AllMobsInWorld())
        {
            if (mob.Wait > 0)
                mob.Wait--;
        }

        // Phase 1: npc updates, guard logic, detection, auto-assist, violence updates
        // NPC behavior
        Enqueue(new NpcUpdateAction());

        // Guard logic
        Enqueue(new SpecGuardAction());

        // Detection
        Enqueue(new DetectionAction());

        // Auto-assist
        Enqueue(new AutoAssistAction());

        // ROM-style violence update
        Enqueue(new ViolenceUpdateAction());

        // Phase 2: Process queued actions
        int actionCount = 0;
        const int maxActions = 10000;

        while (_actionQueue.Count > 0)
        {
            if (actionCount++ > maxActions) break;
            var action = _actionQueue.Dequeue();
            action.Execute(this);
        }

        // Phase 3: Status effects
        //foreach (var mob in AllMobsInWorld())
        //{
        //    foreach (var effect in mob.StatusEffects.ToList())
        //        effect.Tick(mob, this);
        //}
        StatusEffectEngine.Tick(this);

        //// 4 Combat tick: perform auto-assist, multi-hit, hidden detection
        //foreach (var room in Rooms)
        //{
        //    if (room.Mobs.Any(m => m.InCombat && !m.IsDead))
        //    {
        //        Enqueue(new CombatRoundAction(room));
        //    }
        //}

        // Phase 4: Structural mutations
        CommitMutations();

        // Phase 5: Clear attack history
        ClearAttackHistory();

        // Phase 6: Increment tick counter
        CurrentTick++;
    }
    /*
    public void Tick()
    {
        // 1️ Process queued actions
        int actionCount = 0;
        const int maxActions = 10000;

        while (_actionQueue.Count > 0)
        {
            if (actionCount++ > maxActions) break;
            var action = _actionQueue.Dequeue();
            action.Execute(this);
        }

        // 2️ Status effects tick
        foreach (var mob in AllMobsInWorld().ToList())
        {
            foreach (var effect in mob.StatusEffects.ToList())
                effect.Tick(mob, this);
        }

        // 3️ Combat tick: perform auto-assist, multi-hit, hidden detection
        foreach (var room in Rooms)
        {
            if (room.Mobs.Any(m => m.InCombat && !m.IsDead))
            {
                Enqueue(new CombatRoundAction(room));
            }
        }

        // 4️⃣ Apply structural mutations (move, loot, item transfers)
        CommitMutations();

        // 5️⃣ Clear attack history for this tick
        ClearAttackHistory();

        // 6️⃣ Increment tick counter
        CurrentTick++;
    }
    */

    private void CommitMutations()
    {
        foreach (var mutation in _pendingMutations)
            mutation();

        _pendingMutations.Clear();
    }
}