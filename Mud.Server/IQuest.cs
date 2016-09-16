using System.Collections.Generic;
using Mud.Server.Blueprints.Character;
using Mud.Server.Blueprints.Item;
using Mud.Server.Blueprints.Quest;
using Mud.Server.Blueprints.Room;
using Mud.Server.Item;

namespace Mud.Server
{
    public interface IQuest
    {
        QuestBlueprint Blueprint { get; }

        ICharacter Giver { get; } // TODO: quest may be ended with a different NPC

        IEnumerable<QuestObjectiveBase> Objectives { get; }
        void GenerateKillLoot(ICharacter victim, IContainer container);
        void Update(ICharacter victim);
        void Update(IItemQuest item);
        void Update(IRoom room);

        bool IsCompleted { get; }
        void Complete();
        void Abandon();
    }

    public abstract class QuestObjectiveBase
    {
        public abstract bool IsCompleted { get; }

        public abstract string CompletionState { get; }
    }

    public abstract class QuestObjectiveCountBase : QuestObjectiveBase
    {
        public abstract string TargetName { get; }
        public int Count { get; set; }
        public int Total { get; set; }

        #region QuestObjectiveBase

        public override bool IsCompleted => Count >= Total;

        public override string CompletionState => IsCompleted 
            ? $"{TargetName,-20}: complete" 
            : $"{TargetName,-20}: {Count,3} / {Total,3} ({((Count*100)/Total):D}%)";

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
        public CharacterBlueprint Blueprint { get; set; }

        #region QuestObjectiveCountBase

        public override string TargetName => Blueprint.ShortDescription;

        #endregion
    }

    public class LocationQuestObjective : QuestObjectiveBase
    {
        #region QuestObjectiveBase

        public override bool IsCompleted => Explored;

        public override string CompletionState => IsCompleted
            ? $"{Blueprint.Name,-20}: explored"
            : $"{Blueprint.Name,-20}: not explored";

        #endregion

        public RoomBlueprint Blueprint { get; set; }
        public bool Explored { get; set; }
    }
}
