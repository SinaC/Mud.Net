using Mud.Domain;

namespace Mud.POC.Combat;

// see Generating Threat at https://www.wowhead.com/classic/guide/threat-overview-classic-wow
public class AggroTable : IAggroTable
{
    private readonly Dictionary<ICharacter, long> _aggroByEnemy = [];

    public IReadOnlyDictionary<ICharacter, long> AggroByEnemy => _aggroByEnemy;

    public ICharacter? GetEnemyInRoom(IRoom room)
        => _aggroByEnemy.Where(x => x.Key.Room == room).OrderByDescending(x => x.Value).FirstOrDefault().Key; // TODO: find data structure enabling to quickly retrieve by ICharacter and also get top aggro ICharacter

    public AggroResults OnAggroGeneratedByEnemy(ICharacter aggroGenerator, long delta)
    {
        return UpdateAggro(aggroGenerator, delta);
    }

    public AggroResults OnDamageDealtByEnemy(ICharacter damageDealer, int amount)
    {
        var aggroDelta = amount;
        return UpdateAggro(damageDealer, aggroDelta);
    }

    public AggroResults OnHealByEnemy(ICharacter healer, int amount)
    {
        var aggroDelta = Math.Max(1, amount / 4);
        return UpdateAggro(healer, aggroDelta);
    }

    public AggroResults OnEnemyKilled(ICharacter enemy)
    {
        _aggroByEnemy.Remove(enemy);
        return AggroResults.Removed;
    }

    public void Clear()
    {
        _aggroByEnemy.Clear();
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


public interface IAggroTable : IAggroTableReadOnly
{
    AggroResults OnAggroGeneratedByEnemy(ICharacter aggroGenerator, long delta);
    AggroResults OnDamageDealtByEnemy(ICharacter damageDealer, int amount);
    AggroResults OnHealByEnemy(ICharacter healer, int amount);
    AggroResults OnEnemyKilled(ICharacter enemy);

    void Clear();
}

public interface IAggroTableReadOnly
{
    IReadOnlyDictionary<ICharacter, long> AggroByEnemy { get; }
    ICharacter? GetEnemyInRoom(IRoom room);
}

public enum AggroResults
{
    Updated = 1,
    Added = 2,
    Removed = 3
}