using System.Collections.Generic;

namespace Mud.Server.Blueprints.Quest
{
    public class QuestBlueprint
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int Level { get; set; }
        public bool ShouldQuestItemBeDestroyed { get; set; }

        public Dictionary<int, QuestKillLootTable<int>> KillLootTable { get; set; }
        public List<QuestItemObjective> ItemObjectives { get; set; }
        public List<QuestKillObjective> KillObjectives { get; set; }

        // TODO: rewards: loot/xp/gold

        public QuestBlueprint()
        {
            KillLootTable = new Dictionary<int, QuestKillLootTable<int>>();
            ItemObjectives = new List<QuestItemObjective>();
            KillObjectives = new List<QuestKillObjective>();
        }

        public List<int> GenerateKillLoot(int victimBlueprintId)
        {
            QuestKillLootTable<int> table;
            if (!KillLootTable.TryGetValue(victimBlueprintId, out table))
                return null;
            return table.GenerateLoots();
        }
    }
}
