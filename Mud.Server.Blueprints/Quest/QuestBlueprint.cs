using System.Collections.Generic;

namespace Mud.Server.Blueprints.Quest
{
    public class QuestBlueprint
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int Level { get; set; }
        public int Experience { get; set; }
        public int Gold { get; set; }
        public bool ShouldQuestItemBeDestroyed { get; set; }

        public Dictionary<int, QuestKillLootTable<int>> KillLootTable { get; set; }
        public List<QuestItemObjectiveBlueprint> ItemObjectives { get; set; }
        public List<QuestKillObjectiveBlueprint> KillObjectives { get; set; }
        public List<QuestLocationObjectiveBlueprint> LocationObjectives { get; set; }

        // TODO: rewards: loot

        public QuestBlueprint()
        {
            KillLootTable = new Dictionary<int, QuestKillLootTable<int>>();
            ItemObjectives = new List<QuestItemObjectiveBlueprint>();
            KillObjectives = new List<QuestKillObjectiveBlueprint>();
            LocationObjectives = new List<QuestLocationObjectiveBlueprint>();
        }
    }
}
