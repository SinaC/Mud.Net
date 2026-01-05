using Microsoft.Extensions.Logging;
using Mud.Common.Attributes;
using Mud.Server.Random;

namespace Mud.Blueprints.Quest;

// Quest loot table stores global drop percentage instead of a relative percentage
[Export(typeof(QuestKillLootTable<>))]
public class QuestKillLootTable<T> // generated loot when killing mob while on a quest
    where T:IEquatable<T>
{
    private ILogger<QuestKillLootTable<T>> Logger { get; }
    private IRandomManager RandomManager { get; }

    public string Name { get; set; } = default!;
    public List<QuestKillLootTableEntry<T>> Entries { get; set; } = default!;

    public QuestKillLootTable(ILogger<QuestKillLootTable<T>> logger, IRandomManager randomManager)
    {
        Logger = logger;
        RandomManager = randomManager;
    }

    public bool AddItem(T item, int percentage)
    {
        Entries ??= [];
        // TODO: check if already exists ?
        Entries.Add(new QuestKillLootTableEntry<T>
        {
            Value = item,
            Percentage = percentage
        });
        return true;
    }

    public List<T> GenerateLoots(IEnumerable<T> forbiddenIds)
    {
        List<T> loots = [];
        if (Entries != null)
        {
            foreach (var entry in Entries.Where(x => forbiddenIds == null || forbiddenIds.All(y => !y.Equals(x.Value))))
            {
                var percentage = RandomManager.Range(1, 100);
                if (percentage <= entry.Percentage)
                    loots.Add(entry.Value);
            }
        }
        else
            Logger.LogWarning("QuestLootTable.GenerateLoots: No entries");
        return loots;
    }
}
