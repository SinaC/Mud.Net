using Microsoft.Extensions.Logging;
using Mud.Server.Random;

namespace Mud.Server.Blueprints.Quest;

// Quest loot table stores global drop percentage instead of a relative percentage
public class QuestKillLootTable<T> // this represents additional provided by kill mob while working on this quest
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

    public List<T> GenerateLoots()
    {
        List<T> loots = [];
        if (Entries != null)
        {
            foreach (QuestKillLootTableEntry<T> entry in Entries)
            {
                int percentage = RandomManager.Range(1,100);
                if (percentage <= entry.Percentage)
                    loots.Add(entry.Value);
            }
        }
        else
            Logger.LogWarning("QuestLootTable.GenerateLoots: No entries");
        return loots;
    }
}
