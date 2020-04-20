using Mud.Server.Blueprints.Character;
using Mud.Server.Blueprints.Item;
using Mud.Server.Blueprints.Room;

namespace Mud.Server.Quest
{
    public abstract class QuestObjectiveCountBase : IQuestObjective
    {
        public abstract string TargetName { get; }
        public int Count { get; set; }
        public int Total { get; set; }

        #region IQuestObjective

        public int Id { get; set; }

        public bool IsCompleted => Count >= Total;

        public string CompletionState => IsCompleted
            ? $"{TargetName,-20}: complete"
            : $"{TargetName,-20}: {Count,3} / {Total,3} ({((Count * 100) / Total):D}%)";

        #endregion
    }

    public class ItemQuestObjective : QuestObjectiveCountBase
    {
        public ItemQuestBlueprint Blueprint { get; set; }

        #region QuestObjectiveCountBase

        public override string TargetName => Blueprint.ShortDescription;

        #endregion
    }

    public class KillQuestObjective : QuestObjectiveCountBase
    {
        public CharacterBlueprintBase Blueprint { get; set; }

        #region QuestObjectiveCountBase

        public override string TargetName => Blueprint.ShortDescription;

        #endregion
    }

    public class LocationQuestObjective : IQuestObjective
    {
        #region IQuestObjective

        public int Id { get; set; }

        public bool IsCompleted => Explored;

        public string CompletionState => IsCompleted
            ? $"{Blueprint.Name,-20}: explored"
            : $"{Blueprint.Name,-20}: not explored";

        #endregion

        public RoomBlueprint Blueprint { get; set; }
        public bool Explored { get; set; }
    }
}
