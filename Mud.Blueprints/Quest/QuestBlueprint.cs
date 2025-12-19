namespace Mud.Blueprints.Quest;

public class QuestBlueprint
{
    public int Id { get; set; }
    public string Title { get; set; } = default!;
    public string Description { get; set; } = default!;
    public int Level { get; set; }
    public int Experience { get; set; }
    public int Gold { get; set; }
    public bool ShouldQuestItemBeDestroyed { get; set; }
    public int TimeLimit { get; set; } // 0 means no limit, in minutes

    public Dictionary<int, QuestKillLootTable<int>> KillLootTable { get; set; } = default!;
    public QuestItemObjectiveBlueprint[] ItemObjectives { get; set; } = default!;
    public QuestKillObjectiveBlueprint[] KillObjectives { get; set; } = default!;
    public QuestLocationObjectiveBlueprint[] LocationObjectives { get; set; } = default!;

    // TODO: rewards: loot

    public QuestBlueprint()
    {
        KillLootTable = [];
    }
}
