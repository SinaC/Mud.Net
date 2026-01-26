using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mud.Common.Attributes;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Combat;
using Mud.Server.Interfaces.Room;
using Mud.Server.Options;

namespace Mud.Server.Combat;

[Export(typeof(IAggroManager)), Shared]
public class AggroManager : IAggroManager
{
    private readonly Dictionary<INonPlayableCharacter, IAggroTable> _aggroTables = [];

    private ILogger<AggroManager> Logger { get; }
    private bool UseAggro { get; }

    public AggroManager(ILogger<AggroManager> logger, IOptions<WorldOptions> options)
    {
        Logger = logger;
        UseAggro = options.Value.UseAggro;
    }

    public IReadOnlyAggroTable? GetAggroTable(INonPlayableCharacter npc) // used for unit tests
    {
        if (!UseAggro)
            return null;

        if (!_aggroTables.TryGetValue(npc, out var table))
            return null;
        return table;
    }

    public ICharacter? ChooseEnemy(INonPlayableCharacter npc) // this will return enemy in room with highest aggro level
    {
        if (!UseAggro)
            return null;

        if (npc.IsPetOrCharmie || !npc.IsAlive) // pets don't have an aggro table
            return null;
        var aggroTable = GetOrCreateAggroTable(npc);
        if (aggroTable == null)
            return null;
        return aggroTable.GetEnemyInRoom(npc.Room!);
    }

    public void OnStartFight(ICharacter source, ICharacter victim)
    {
        if (!UseAggro)
            return;
        if (source == victim)
            return;
        if (victim is not INonPlayableCharacter npc)
            return;
        if (npc.IsPetOrCharmie || !npc.IsAlive) // pets don't have an aggro table
            return;
        Logger.LogDebug("AggroManager: OnStartFight between {source} and {victim}", source.DebugName, victim.DebugName);
        var aggroTable = GetOrCreateAggroTable(npc);
        aggroTable?.OnAggroGeneratedByEnemy(source, 1); // generate a small amount of aggro
    }

    public void OnDeath(ICharacter victim)
    {
        if (!UseAggro)
            return;

        Logger.LogDebug("AggroManager: OnDeath {victim}", victim.DebugName);
        // remove victim from every aggro table
        foreach (var entry in _aggroTables)
            entry.Value.OnEnemyKilled(victim);
        // remove victim aggro table
        if (victim is INonPlayableCharacter npc && !npc.IsPetOrCharmie) // pets don't have an aggro table
            _aggroTables.Remove(npc);
    }

    public void OnFlee(ICharacter character, IRoom wasInRoom)
    {
        if (!UseAggro)
            return;

        Logger.LogDebug("AggroManager: OnFlee {victim} was in {room}", character.DebugName, wasInRoom.DebugName);
        // decrease aggro by 1/3 on each npc fighting character which was in the same room before fleeing
        foreach (var npc in wasInRoom.People.OfType<INonPlayableCharacter>().Where(x => !x.IsPetOrCharmie && x.IsAlive))
        {
            if (_aggroTables.TryGetValue(npc, out var aggroTable)) // don't use GetOrCreate, we want to decrease existing aggro table
                aggroTable.OnEnemyFled(character);
        }
    }

    public void OnReceiveDamage(ICharacter source, ICharacter victim, int damage)
    {
        if (!UseAggro)
            return;
        if (source == victim)
            return;
        if (victim is not INonPlayableCharacter npc)
            return;
        if (npc.IsPetOrCharmie || !npc.IsAlive) // pets don't have an aggro table
            return;
        Logger.LogDebug("AggroManager: OnReceiveDamage {source} damaged {victim} for {damage}", source.DebugName, victim.DebugName, damage);
        // generate aggro for victm
        var aggroTable = GetOrCreateAggroTable(npc);
        aggroTable?.OnDamageDealtByEnemy(source, damage);
    }

    public void OnHeal(ICharacter source, ICharacter target, int heal)
    {
        if (!UseAggro)
            return;
        Logger.LogDebug("AggroManager: OnHeal {source} healed {victim} for {heal}", source.DebugName, target.DebugName, heal);
        // generate aggro for each non-pet npc fighting target in the same room, don't force to fight npc
        foreach (var npc in target.Room.People.OfType<INonPlayableCharacter>().Where(x => !x.IsPetOrCharmie && x.IsAlive && x.Fighting == target))
        {
            var aggroTable = GetOrCreateAggroTable(npc);
            aggroTable?.OnHealByEnemy(source, heal);// && aggroTable.AggroByEnemy.ContainsKey(source)) // add aggro only if already in aggro table ?
        }
    }

    public void OnTaunt(ICharacter source, ICharacter victim)
    {
        if (!UseAggro)
            return;
        if (source == victim)
            return;
        if (victim is not INonPlayableCharacter npc)
            return;
        if (npc.IsPetOrCharmie || !npc.IsAlive) // pets don't have an aggro table
            return;
        Logger.LogDebug("AggroManager: OnTaunt {source} taunted {victim}", source.DebugName, npc.DebugName);
        var aggroTable = GetOrCreateAggroTable(npc);
        aggroTable?.OnTaunt(source);
    }

    public void OnRemove(ICharacter character)
    {
        if (!UseAggro)
            return;

        Logger.LogDebug("AggroManager: OnRemove {victim}", character.DebugName);
        // remove character from every aggro table
        foreach (var entry in _aggroTables)
            entry.Value.OnEnemyKilled(character);
        // remove character aggro table
        if (character is INonPlayableCharacter npc && !npc.IsPetOrCharmie) // pets don't have an aggro table
            _aggroTables.Remove(npc);
    }

    public void DecreaseAggroOfEnemiesIfNotInSameRoom(ICharacter character)
    {
        if (!UseAggro)
            return;
        if (character is not INonPlayableCharacter npc)
            return;
        if (npc.IsPetOrCharmie || !npc.IsAlive) // pets don't have an aggro table
            return;
        if (!_aggroTables.TryGetValue(npc, out var aggroTable)) // don't use GetOrCreate, we want to decrease existing aggro table
            return;
        Logger.LogDebug("AggroManager: DecreaseAggroOfEnemiesIfNotInSameRoom {character}", character.DebugName);
        // for each enemy not in the same room, decrease aggro by 10%
        foreach (var kv in aggroTable.AggroByEnemy)
        {
            if (kv.Key.Room != npc.Room)
            {
                var delta = -Math.Max(10, kv.Value / 10);
                aggroTable.OnAggroGeneratedByEnemy(kv.Key, delta);
            }
        }
    }

    public void Clear(ICharacter character)
    {
        if (!UseAggro)
            return;
        if (character is not INonPlayableCharacter npc)
            return;
        Logger.LogDebug("AggroManager: Clear {character}", character.DebugName);
        _aggroTables.Remove(npc);
    }

    //
    private IAggroTable? GetOrCreateAggroTable(INonPlayableCharacter npc)
    {
        if (npc.IsPetOrCharmie || !npc.IsAlive) // pets don't have an aggro table
            return null;
        if (!_aggroTables.TryGetValue(npc, out var table))
        {
            Logger.LogDebug("AggroManager: create aggro table for {npc}", npc.DebugName);
            table = new AggroTable();
            _aggroTables.Add(npc, table);
        }
        return table;
    }
}
