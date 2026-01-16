namespace Mud.Domain.SerializationData.Avatar;

public class CompletedQuestData
{
    public required int QuestId { get; set; }
    public required DateTime StartTime { get; set; }
    public required DateTime CompletionTime { get; set; }
}