namespace Mud.Blueprints.Quest;

public class QuestFloorItemObjectiveBlueprint : QuestItemObjectiveBlueprintBase
{
    public int SpawnCountOnRequest { get; set; } = 1; // initial instance when requesting the quest
    public int MaxInWorld { get; set; } = 1; // max instance allow in whole world
    public int MaxInRoom { get; set; } = 1; // max instance allowed in a room
    public int[] RoomBlueprintIds { get; set; } = []; // various spawn location
}
