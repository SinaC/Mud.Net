using Mud.Blueprints.Room;

namespace Mud.Server.Quest.Objectives;

public class FloorItemQuestObjective : ItemQuestObjectiveBase
{
    public required int[] RoomBlueprintIds { get; set; }
}
