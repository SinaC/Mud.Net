using System.Collections.Generic;
using Mud.Server.Blueprints.Character;
using Mud.Server.Blueprints.Item;
using Mud.Server.Blueprints.Quest;

namespace Mud.Server
{
    // TODO: function to get RemainingItem/RemainingKill
    public interface IQuest
    {
        QuestBlueprint Blueprint { get; }

        ICharacter Giver { get; }

        IEnumerable<QuestObjectiveBase> Objectives { get; }
        void Update(ICharacter victim);

        bool IsCompleted { get; }
        void Complete();
        void Abandon();
    }

    public abstract class QuestObjectiveBase
    {
        public abstract string TargetName { get; }
        public bool IsCompleted => Count >= Total;
        public int Count { get; set; }
        public int Total { get; set; }
    }

    public class ItemQuestObjective : QuestObjectiveBase
    {
        public override string TargetName => Blueprint.ShortDescription;

        public ItemBlueprintBase Blueprint { get; set; }
    }

    public class KillQuestObjective : QuestObjectiveBase
    {
        public override string TargetName => Blueprint.ShortDescription;

        public CharacterBlueprint Blueprint { get; set; }
    }
}
