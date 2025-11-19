using Microsoft.Extensions.Logging;
using Mud.Server.Random;

namespace Mud.Server.Blueprints.LootTable;

// http://www.gammon.com.au/forum/bbshowpost.php?bbsubject_id=9715
public class CharacterLootTable<T>
    where T:IEquatable<T>
{
    private ILogger<CharacterLootTable<T>> Logger { get; }
    private IRandomManager RandomManager { get; }

    public int MinLoot { get; set; }
    public int MaxLoot { get; set; }
    public List<CharacterLootTableEntry<T>> Entries { get; set; } = default!;
    //public List<T> AlwaysDrop { get; set; }

    public CharacterLootTable(ILogger<CharacterLootTable<T>> logger, IRandomManager randomManager)
    {
        Logger = logger;
        RandomManager = randomManager;

        MinLoot = 1;
        MaxLoot = 1;
    }

    public bool AddTreasureList(TreasureTable<T> table, int occurancy, int maxLoot)
    {
        Entries ??= [];
        // TODO: check if already exists ?
        Entries.Add(new CharacterLootTableEntry<T>
        {
            Value = table,
            Occurancy = occurancy,
            Max = maxLoot
        });
        return true;
    }

    public List<T> GenerateLoots()
    {
        List<CharacterLootTableEntry<T>> history = [];
        List<T> lootList = [];
        int lootCount = RandomManager.Next(MinLoot, MaxLoot + 1);
        //Logger.LogDebug("#Loot: {0}", lootCount);
        if (Entries != null)
        {
            for (int loop = 1; loop <= lootCount; loop++)
            {
                var randomEntry = RandomManager.RandomOccurancy<CharacterLootTableEntry<T>, TreasureTable<T>>(Entries);
                if (randomEntry != null)
                {
                    //Logger.LogDebug("Table: {0}", randomEntry.Value.Name);
                    if (randomEntry.Value.Entries != null) // shortcut for empty table
                    {
                        if (history.Count(x => x.Value == randomEntry.Value) < randomEntry.Max) // check if max loot in this entry reached
                        {
                            history.Add(randomEntry);
                            var loot = randomEntry.Value.GenerateLoot(lootList);
                            if (loot?.Equals(default) == false)
                                lootList.Add(loot);
                        }
                        //else
                        //    Logger.LogDebug("Table rejected #>Max");
                    }
                    //else
                    //    Logger.LogDebug("Empty table");
                }
                else
                    Logger.LogWarning("CharacterLootTable.GenerateLoots: no treasure table found");
            }
        }
        else
            Logger.LogWarning("CharacterLootTable.GenerateLoots: No entries");
        //if (AlwaysDrop != null)
        //    lootList.AddRange(AlwaysDrop);
        return lootList;
    }
}
