namespace Mud.Domain.SerializationData;

public class CurrentQuestData
{
    public required int QuestId { get; set; }

    public required DateTime StartTime { get; set; }

    public required int PulseLeft { get; set; }

    public required DateTime? CompletionTime { get; set; }

    public required int GiverId { get; set; }

    public required int GiverRoomId { get; set; }

    public required CurrentQuestObjectiveData[] Objectives { get; set; }
}
