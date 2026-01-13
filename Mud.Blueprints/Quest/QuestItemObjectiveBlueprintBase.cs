namespace Mud.Blueprints.Quest;

public abstract class QuestItemObjectiveBlueprintBase
{
    public required int Id { get; set; }
    public required int ItemBlueprintId { get; set; }
    public required int Count { get; set; }
}
