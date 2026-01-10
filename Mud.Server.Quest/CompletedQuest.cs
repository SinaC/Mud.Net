using Mud.Blueprints.Quest;
using Mud.Domain.SerializationData.Avatar;
using Mud.Server.Interfaces.Quest;

namespace Mud.Server.Quest;

public class CompletedQuest : ICompletedQuest
{
    public int QuestId { get; set; }

    public QuestBlueprint? QuestBlueprint { get; set; }

    public DateTime StartTime { get; set; }

    public DateTime CompletionTime { get; set; }

    public CompletedQuestData MapCompletedQuestData()
        => new()
        {
            QuestId = QuestId,
            StartTime = StartTime,
            CompletionTime = CompletionTime,
        };
}
