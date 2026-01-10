using Mud.Blueprints.Quest;
using Mud.Domain.SerializationData.Avatar;

namespace Mud.Server.Interfaces.Quest;

public interface ICompletedQuest
{
    int QuestId { get; }
    QuestBlueprint? QuestBlueprint { get; }

    public DateTime StartTime { get; }
    public DateTime CompletionTime { get; }

    CompletedQuestData MapCompletedQuestData();
}
