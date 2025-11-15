using Mud.Logger;
using Mud.Server.Random;

namespace Mud.Server.Blueprints.LootTable;

public class TreasureTable<T>
    where T:IEquatable<T>
{
    private IRandomManager RandomManager { get; }

    public string Name { get; set; } = default!;
    public List<TreasureTableEntry<T>> Entries { get; set; } = default!;

    public TreasureTable(IRandomManager randomManager)
    {
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
            Log.Default.WriteLine(LogLevels.Warning, "TreasureTable.GenerateLoot: no loot found");
        return randomId;
    }

    public T? GenerateLoot(IEnumerable<T> history)
    {
        if (Entries == null)
        {
            Log.Default.WriteLine(LogLevels.Warning, "TreasureTable.GenerateLoot: No entries");
            return default; // max occurancy reached, no loot
        }
        var randomEntry = RandomManager.RandomOccurancy<TreasureTableEntry<T>, T>(Entries);
        if (randomEntry == default)
        {
            Log.Default.WriteLine(LogLevels.Warning, "TreasureTable.GenerateLoot: no loot found");
            return default;
        }
        //Log.Default.WriteLine(LogLevels.Debug, "Loot: {0}", randomEntry.Value);
        if (history.Count(x => x.Equals(randomEntry.Value)) >= randomEntry.MaxOccurancy)
        {
            //Log.Default.WriteLine(LogLevels.Debug, "Loot rejected #>Max");
            return default; // max occurancy reached, no loot
        }
        return randomEntry.Value;
    }
}
