using Mud.Server.Domain;
using Mud.Server.Interfaces.Affect.Character;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Combat;
using Mud.Server.Interfaces.Room;
using System.Reflection.Metadata.Ecma335;

namespace Mud.Server.Combat;

public class AggroTable : IAggroTable
{
    private readonly Dictionary<ICharacter, long> _aggroByEnemy = [];

    public IReadOnlyDictionary<ICharacter, long> AggroByEnemy => _aggroByEnemy;

    public ICharacter? GetEnemyInRoom(IRoom room)
        => _aggroByEnemy.Where(x => x.Key.Room == room).OrderByDescending(x => x.Value).FirstOrDefault().Key; // TODO: find data structure enabling to quickly retrieve by ICharacter and also get top aggro ICharacter

    public AggroResults OnAggroGeneratedByEnemy(ICharacter aggroGenerator, long delta)
    {
        var aggroDelta = ModifyAggroDeltaUsingAffects(aggroGenerator, delta);
        return UpdateAggro(aggroGenerator, delta);
    }

    public AggroResults OnDamageDealtByEnemy(ICharacter damageDealer, int amount)
    {
        var aggroDelta = ModifyAggroDeltaUsingAffects(damageDealer, amount);
        return UpdateAggro(damageDealer, aggroDelta);
    }

    public AggroResults OnHealByEnemy(ICharacter healer, int amount)
    {
        var aggroDelta = ModifyAggroDeltaUsingAffects(healer, Math.Max(1, amount / 4));
        return UpdateAggro(healer, aggroDelta);
    }

    public AggroResults OnEnemyKilled(ICharacter enemy)
    {
        _aggroByEnemy.Remove(enemy);
        return AggroResults.Removed;
    }

    public AggroResults OnEnemyFled(ICharacter enemy)
    {
        if (!_aggroByEnemy.TryGetValue(enemy, out var aggro))
            return AggroResults.NoAggro;

        var aggroDelta = -Math.Max(1, aggro / 3);
        return UpdateAggro(enemy, aggroDelta);
    }

    public AggroResults OnTaunt(ICharacter enemy)
    {
        if (_aggroByEnemy.Count == 0)
            return AggroResults.NoAggro;
        // get highest aggro
        var enemyWithHighestAggro = _aggroByEnemy.OrderByDescending(x => x.Value).FirstOrDefault();
        // if enemy is already the highest in list, don't do anything
        if (enemyWithHighestAggro.Key == enemy)
            return AggroResults.Nop;
        // enemy will generate enough aggro to go before current highest aggro in table
        var aggroDelta = enemyWithHighestAggro.Value + 1;
        if (_aggroByEnemy.TryGetValue(enemy, out var aggro))
            aggroDelta = Math.Max(1, enemyWithHighestAggro.Value - aggro) + 1;
        //
        return UpdateAggro(enemy, aggroDelta);
    }

    public AggroResults OnStopFight(ICharacter enemy)
    {
        if (!_aggroByEnemy.ContainsKey(enemy))
            return AggroResults.NoAggro;

        _aggroByEnemy.Remove(enemy);
        return AggroResults.Removed;
    }

    public void Clear()
    {
        _aggroByEnemy.Clear();
    }

    private long ModifyAggroDeltaUsingAffects(ICharacter character, long aggroDelta)
    {
        // don't modify aggro loss
        if (aggroDelta <= 0)
            return aggroDelta;
        var result = aggroDelta;
        foreach (var aggroModifierAffect in character.Auras.Where(x => x.IsValid).SelectMany(x => x.Affects.OfType<ICharacterAggroModifierAffect>()))
        {
            result = result * aggroModifierAffect.MultiplierInPercent / 100;
        }
        return result;
    }

    // if aggro reaches 0, remove from aggro table
    private AggroResults UpdateAggro(ICharacter enemy, long delta)
    {
        if (_aggroByEnemy.TryGetValue(enemy, out long value))
        {
            var newAggro = Math.Max(0, value + delta);
            if (newAggro == 0)
            {
                _aggroByEnemy.Remove(enemy);
                return AggroResults.Removed;
            }
            else
            {
                _aggroByEnemy[enemy] = newAggro;
                return AggroResults.Updated;
            }
        }
        else
        {
            var aggro = Math.Max(0, delta);
            if (aggro != 0)
            {
                _aggroByEnemy[enemy] = aggro;
                return AggroResults.Added;
            }
            else
                return AggroResults.Removed;
        }
    }
}
