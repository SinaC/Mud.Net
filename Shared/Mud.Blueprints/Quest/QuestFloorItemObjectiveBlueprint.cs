namespace Mud.Blueprints.Quest;

public class QuestFloorItemObjectiveBlueprint : QuestItemObjectiveBlueprintBase
{
    public required int SpawnCountOnRequest { get; set; } = 1; // initial instance when requesting the quest
    public required int MaxInWorld { get; set; } = 1; // max instance allow in whole world
    public required int MaxInRoom { get; set; } = 1; // max instance allowed in a room
    public required int[] RoomBlueprintIds { get; set; } = []; // various spawn location
}
