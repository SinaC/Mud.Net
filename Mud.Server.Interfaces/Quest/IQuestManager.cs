using Mud.Server.Blueprints.Quest;
using System.Collections.Generic;

namespace Mud.Server.Interfaces.Quest
{
    public interface IQuestManager
    {
        IReadOnlyCollection<QuestBlueprint> QuestBlueprints { get; }

        QuestBlueprint GetQuestBlueprint(int id);

        void AddQuestBlueprint(QuestBlueprint blueprint);
    }
}
