namespace Mud.Domain.SerializationData.Avatar;

public class ActiveQuestData
{
    public required int QuestId { get; set; }

    public required DateTime StartTime { get; set; }

    public required int PulseLeft { get; set; }

    public required DateTime? CompletionTime { get; set; }

    public required int GiverId { get; set; }

    public required int GiverRoomId { get; set; }

    public required ActiveQuestObjectiveData[] Objectives { get; set; }
}
