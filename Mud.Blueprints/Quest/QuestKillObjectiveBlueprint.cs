namespace Mud.Blueprints.Quest;

public class QuestKillObjectiveBlueprint
{
    public required int Id { get; set; }
    public required int CharacterBlueprintId { get; set; }
    public required int Count { get; set; }
}
