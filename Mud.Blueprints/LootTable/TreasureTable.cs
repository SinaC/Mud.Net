using Microsoft.Extensions.Logging;
using Mud.Server.Random;

namespace Mud.Blueprints.LootTable;

public class TreasureTable<T>
    where T:IEquatable<T>
{
    private ILogger<TreasureTable<T>> Logger { get; }
    private IRandomManager RandomManager { get; }

    public string Name { get; set; } = default!;
    public List<TreasureTableEntry<T>> Entries { get; set; } = default!;

    public TreasureTable(ILogger<TreasureTable<T>> logger, IRandomManager randomManager)
    {
        Logger = logger;
        RandomManager = randomManager;
    }

    public bool AddItem(T item, int occurancy, int maxOccurancy)
    {
        Entries ??= [];
        // TODO: check if already exists ?
        Entries.Add(new TreasureTableEntry<T>
        {
            Value = item,
            Occurancy = occurancy,
            MaxOccurancy = maxOccurancy
        });
        return true;
    }

    public T? GenerateLoot()
    {
        var randomId = RandomManager.RandomOccurancy(Entries);
        if (randomId?.Equals(default) == true)
            Logger.LogWarning("TreasureTable.GenerateLoot: no loot found");
        return randomId;
    }

    public T? GenerateLoot(IEnumerable<T> history)
    {
        if (Entries == null)
        {
            Logger.LogWarning("TreasureTable.GenerateLoot: No entries");
            return default; // max occurancy reached, no loot
        }
        var randomEntry = RandomManager.RandomOccurancy<TreasureTableEntry<T>, T>(Entries);
        if (randomEntry == default)
        {
            Logger.LogWarning("TreasureTable.GenerateLoot: no loot found");
            return default;
        }
        //Logger.LogDebug("Loot: {0}", randomEntry.Value);
        if (history.Count(x => x.Equals(randomEntry.Value)) >= randomEntry.MaxOccurancy)
        {
            //Logger.LogDebug("Loot rejected #>Max");
            return default; // max occurancy reached, no loot
        }
        return randomEntry.Value;
    }
}
