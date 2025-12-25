using Mud.Domain;

namespace Mud.POC.Combat;

public class CombatManager : ICombatManager
{
    private readonly Dictionary<INonPlayableCharacter, IAggroTable> _aggroTables = [];
    private readonly Dictionary<ICharacter, ICharacter> _fighting = []; // 'key' is fighting 'value' (non-symmetrical relation)

    public IAggroTableReadOnly? GetAggroTable(INonPlayableCharacter npc)
    {
        if (!_aggroTables.TryGetValue(npc, out var table))
            return null;
        return table;

    }

    public ICharacter? GetFighting(ICharacter character)
    {
        if (!_fighting.TryGetValue(character, out var fighting) || fighting.IsDead)
            return null;
        return fighting;
    }

    public void StartCombat(ICharacter source, ICharacter victim)
    {
        // source, source's group members and source's pets fight victim
        JoinFight(source, victim);

        // victim, victim's group members and victim's pets fight source
        JoinFight(victim, source);
    }

    public void StopFighting(ICharacter character)
    {
        // clear victim.Fighting
        _fighting.Remove(character);
        // clear all character which were fighting victim
        foreach (var kv in _fighting)
        {
            if (kv.Value == character)
                _fighting.Remove(kv.Key);
        }
    }

    public void FleeCombat(ICharacter character)
    {
        // still fighting ?
        if (GetFighting(character) == null)
            return;

        // TODO: other checks such as daze
        // TODO: get flee direction
        // TODO: move to room
        var destinationRoom = Room.FleeRoom;
        var wasInRoom = character.Room;
        character.Room?.Remove(character);
        character.SetRoom(destinationRoom);
        //
        StopFighting(character); // stop fighting but stay in aggro list
        // decrease aggro by 1/3 on each npc fighting character which were in the same room before fleeing
        foreach (var kv in _aggroTables)
        {
            if (kv.Key.Room == wasInRoom && kv.Value.AggroByEnemy.TryGetValue(character, out var aggro))
            {
                var delta = -Math.Max(1, aggro / 3);
                kv.Value.OnAggroGeneratedByEnemy(character, delta);
            }
        }
        //
        wasInRoom?.Recompute();
        destinationRoom?.Recompute();
    }

    public void CombatRound(INonPlayableCharacter npc)
    {
        if (npc.IsDead || npc.Room == null)
            return;

        if (npc.Master == null) // only non-pet mob can have an aggro table
        {
            // pick an enemy using aggro table
            ChooseEnemy(npc);
        }

        var npcFighting = GetFighting(npc);

        // if no enemy, stops
        if (npcFighting == null || npc.IsDead)
            return;

        // if enemy not in same room
        if (npcFighting.Room != npc.Room)
            return;

        // perform auto attacks
        PerformAutoAttacks(npc, npcFighting);
    }

    public void CombatRound(IPlayableCharacter pc)
    {
        if (pc.IsDead || pc.Room == null)
            return;

        var pcFighting = GetFighting(pc);

        // if no enemy, stops
        if (pcFighting == null || pcFighting.IsDead)
            return;

        // if enemy not in same room, stops
        if (pcFighting.Room != pc.Room)
            return;

        // force group members and pets in the same room to fight the same enemy if not already fighting
        JoinFight(pc, pcFighting);

        // perform auto attacks
        PerformAutoAttacks(pc, pcFighting);
    }

    public DamageResults AbilityDamage(ICharacter source, ICharacter victim, int damage, SchoolTypes damageType)
    {
        var damageResults = Damage(source, victim, damage, damageType, DamageSources.Ability);
        if (damageResults == DamageResults.Done && source != victim)
        {
            // TODO: handle wimpy
        }
        return damageResults;
    }

    public void Heal(ICharacter source, ICharacter target, int heal)
    {
        target.SetHitPoints(target.HitPoints + heal);
        // generate aggro for each npc fighting target, don't force to fight npc
        foreach(var kv in _fighting)
        {
            if (kv.Key is INonPlayableCharacter npc && npc.Master == null && kv.Key.Room == source.Room) // only non-pet mob can have an aggro table
            {
                var aggroTable = GetOrCreateAggroTable(npc);
                if (aggroTable != null)// && aggroTable.AggroByEnemy.ContainsKey(source)) // add aggro only if already in aggro table
                {
                    aggroTable.OnHealByEnemy(source, heal);
                }
            }
        }
    }

    public void DecreaseAggroOfEnemiesIfNotInSameRoom(INonPlayableCharacter npc)
    {
        if (npc.Master != null) // pet don't have an aggro table
            return;
        var aggroTable = GetOrCreateAggroTable(npc);
        if (aggroTable == null)
            return;
        // for each enemy not in the same room, decrease aggro by 10%
        foreach (var kv in aggroTable.AggroByEnemy)
        {
            if (kv.Key.Room != npc.Room)
            {
                var delta = -Math.Max(1, kv.Value / 10);
                aggroTable.OnAggroGeneratedByEnemy(kv.Key, delta);
            }
        }
    }

    public void ClearAggroTable(INonPlayableCharacter npc)
    {
        _aggroTables.Remove(npc);
    }

    private void PerformAutoAttacks(ICharacter source, ICharacter victim)
    {
        // can't autoattack if source or victim is already dead
        if (IsCombatInvalid(source, victim))
            return;

        // auto attacks only in same room
        if (source.Room != victim.Room)
            return;

        // simple auto attack simulator
        for (int i = 0; i < source.AutoAttackCount; i++)
        {
            PerformAutoAttack(source, victim);

            if (IsNotFightingVictim(source, victim))
                return;
        }
    }

    private void PerformAutoAttack(ICharacter source, ICharacter victim)
    {
        // can't autoattack if source or victim is already dead
        if (IsCombatInvalid(source, victim))
            return;

        // auto attack only in same room
        if (source.Room != victim.Room)
            return;

        // simple auto attack simulator
        var damageResult = HitDamage(source, victim, source.AutoAttachDamage, SchoolTypes.Pierce);

        // simple counter attack
        if (damageResult == DamageResults.Done && victim.IsCounterAttackActive)
            AbilityDamage(victim, source, victim.CounterAttackDamage, SchoolTypes.Bash);
    }

    private void JoinFight(ICharacter source, ICharacter victim)
    {
        var npcAggroTable = victim is INonPlayableCharacter npcVictim && npcVictim.Master == null // only non-pet mob can have an aggro table
            ? GetOrCreateAggroTable(npcVictim)
            : null;
        if (GetFighting(source) == null && source.Room == victim.Room)
        {
            SetFighting(source, victim);
            npcAggroTable?.OnAggroGeneratedByEnemy(source, 1);
        }
        // force pets and groupies to fight if not already fighting
        if (source is IPlayableCharacter pc)
        {
            PetsJoinFight(pc, victim, npcAggroTable);

            if (pc.Group != null)
            {
                foreach (var member in pc.Group.Members.Where(x => x != source && x.Room == victim.Room)) // TODO: check autoassist
                {
                    // don't call JoinFight it would lead to infinite recursive call
                    if (GetFighting(member) == null)
                    {
                        SetFighting(member, victim);
                        npcAggroTable?.OnAggroGeneratedByEnemy(member, 1);
                    }
                    PetsJoinFight(member, victim, npcAggroTable);
                }
            }
        }
        if (source is INonPlayableCharacter npc && npc.Master != null)
            JoinFight(npc.Master, victim);
    }

    private void PetsJoinFight(IPlayableCharacter pc, ICharacter victim, IAggroTable? npcVictimAggroTable)
    {
        foreach (var pet in pc.Pets.Where(x => x.Room == victim.Room && GetFighting(x) == null))
        {
            SetFighting(pet, victim);
            npcVictimAggroTable?.OnAggroGeneratedByEnemy(pet, 1);
        }
    }

    private DamageResults HitDamage(ICharacter source, ICharacter victim, int damage, SchoolTypes damageType)
    {
        var damageResults = Damage(source, victim, damage, damageType, DamageSources.Hit);
        if (damageResults == DamageResults.Done && source != victim)
        {
            // TODO: handle wimpy
        }
        return damageResults;
    }

    private DamageResults Damage(ICharacter source, ICharacter victim, int damage, SchoolTypes damageType, DamageSources damageSource)
    {
        // can't damage if source or victim is already dead
        if (IsCombatInvalid(source, victim))
            return DamageResults.AlreadyDead;

        // damage only in same room
        if (source.Room != victim.Room)
            return DamageResults.NotInSameRoom;

        if (IsSafe(source, victim))
            return DamageResults.Safe;

        var npcVictim = victim as INonPlayableCharacter;

        // source, source's group members and source's pets fight victim
        JoinFight(source, victim);

        // victim, victim's group members and victim's pets fight source
        JoinFight(victim, source);

        // TODO: damage modifiers

        // TODO: display damage

        // no damage will be done, stops here
        if (damage <= 0)
            return DamageResults.NoDamage;

        // deals damage + handle deaths
        var newHitPoints = victim.HitPoints - damage;
        var isDead = newHitPoints <= 0;
        // deals damage
        victim.SetHitPoints(newHitPoints);
        // if dead, handle death
        if (isDead)
        {
            HandleDeath(victim, source);

            return DamageResults.Killed;
        }

        // generate aggro
        if (npcVictim != null && npcVictim.Master == null && source.Room == npcVictim.Room) // only non-pet mob can have an aggro table
        {
            var aggroTable = GetOrCreateAggroTable(npcVictim);
            aggroTable.OnDamageDealtByEnemy(source, damage);
        }

        // TODO: on damage received
        OnDamageReceived(victim, damage, damageSource);

        // TODO: on damage dealt
        OnDamagePerformed(source, damage, damageSource);

        return isDead ? DamageResults.Killed : DamageResults.Done;
    }

    private void OnDamageReceived(ICharacter character, int damage, DamageSources damageSource)
    {
        // TODO
        //GenerateRageFromIncomingDamage(character, damage, damageSource);
    }

    private void OnDamagePerformed(ICharacter character, int damage, DamageSources damageSource)
    {
        // TODO
        //GenerateRageFromOutgoingDamage(character, damage, damageSource);
    }

    private bool IsSafe(ICharacter source, ICharacter victim)
    {
        // TODO
        return false;
    }

    private void HandleDeath(ICharacter victim, ICharacter killer)
    {
        if (victim is IPlayableCharacter pc)
            HandlePlayableCharacterDeath(pc, killer);
        else if (victim is INonPlayableCharacter npc)
            HandleNonPlayableCharacterDeath(npc, killer);
    }

    private void HandlePlayableCharacterDeath(IPlayableCharacter pc, ICharacter killer)
    {
        // remove victim from every aggro table
        foreach (var entry in _aggroTables)
            entry.Value.OnEnemyKilled(pc);

        // set fighting to null for each character with Fighting == victim
        StopFighting(pc);

        // if any pets, destroy pets
        foreach (var pet in pc.Pets)
        {
            pc.RemovePet(pet);
            pet.SetMaster(null);
            pc.FlagAsDead();
        }
        // create corpse
        // TODO: move inventory/equipments to corpse
        var corpse = new ItemCorpse("pc corpse", pc);
        corpse.SetRoom(pc.Room);
        pc.Room?.Add(corpse);
        // teleport player to death room
        var wasInRoom = pc.Room;
        pc.Room?.Remove(pc);
        pc.SetRoom(Room.DeathRoom);
        Room.DeathRoom.Add(pc);
        //
        pc.FlagAsDead();
        //
        wasInRoom?.Recompute();
    }

    private void HandleNonPlayableCharacterDeath(INonPlayableCharacter npc, ICharacter killer)
    {
        // remove victim from every aggro table
        foreach (var entry in _aggroTables)
        {
            entry.Value.OnEnemyKilled(npc);
        }

        // set fighting to null for each character with Fighting == victim
        StopFighting(npc);

        // clear victim's aggro table
        ClearAggroTable(npc);

        // if pet, remove from pet list
        if (npc.Master != null)
        {
            npc.Master.RemovePet(npc);
            npc.SetMaster(null);
        }
        // create corpse
        // TODO: move inventory/equipments to corpse
        var corpse = new ItemCorpse("npc corpse", npc);
        corpse.SetRoom(npc.Room);
        npc.Room?.Add(corpse);
        // nullify npc room
        var wasInRoom = npc.Room;
        npc.Room?.Remove(npc);
        npc.SetRoom(null);
        //
        npc.FlagAsDead();
        //
        wasInRoom?.Recompute();
    }

    private void SetFighting(ICharacter character, ICharacter victim)
    {
        _fighting[character] = victim;
    }

    private void ChooseEnemy(INonPlayableCharacter npc) // this will return enemy in room with highest aggro level
    {
        if (npc.Master != null) // only non-pet mob can have an aggro table
            return;
        var aggroTable = GetOrCreateAggroTable(npc);
        var enemy = aggroTable.GetEnemyInRoom(npc.Room!);
        if (enemy != null)
            SetFighting(npc, enemy);
    }

    private IAggroTable GetOrCreateAggroTable(INonPlayableCharacter npc)
    {
        if (!_aggroTables.TryGetValue(npc, out var table))
        {
            table = new AggroTable();
            _aggroTables.Add(npc, table);
        }
        return table;
    }

    private static bool IsCombatInvalid(ICharacter source, ICharacter victim)
        => source == victim || source == null || victim == null || victim.IsDead || source.IsDead;
    private bool IsNotFightingVictim(ICharacter source, ICharacter victim)
        => IsCombatInvalid(source, victim) || GetFighting(source) != victim;
}

public interface ICombatManager : ICombatManagerReadOnly
{
    void StartCombat(ICharacter source, ICharacter victim);
    void StopFighting(ICharacter character);
    void FleeCombat(ICharacter character);

    void CombatRound(INonPlayableCharacter npc);
    void CombatRound(IPlayableCharacter pc);

    DamageResults AbilityDamage(ICharacter source, ICharacter victim, int damage, SchoolTypes damageType);
    void Heal(ICharacter source, ICharacter target, int heal);

    void DecreaseAggroOfEnemiesIfNotInSameRoom(INonPlayableCharacter npc);
    void ClearAggroTable(INonPlayableCharacter npc);
}

public interface ICombatManagerReadOnly
{
    IAggroTableReadOnly? GetAggroTable(INonPlayableCharacter npc);
    ICharacter? GetFighting(ICharacter character);
}

public enum DamageResults
{
    AlreadyDead, // target was already dead
    NotInSameRoom, // target was not in the same room
    Safe, // target is safe
    NoDamage, // damage has been reduced to 0
    Killed, // target has been killed by damage
    Done, // normal damage
}