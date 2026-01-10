using Mud.Blueprints.Room;
using Mud.Server.Interfaces.Quest;

namespace Mud.Server.Quest.Objectives;

public class LocationQuestObjective : IQuestObjective
{

    public required RoomBlueprint RoomBlueprint { get; set; }

    public bool Explored { get; set; }

    #region IQuestObjective

    public int Id { get; set; }

    public bool IsCompleted => Explored;

    public string CompletionState => IsCompleted
        ? $"{RoomBlueprint.Name,-20}: explored"
        : $"{RoomBlueprint.Name,-20}: not explored";

    public void Reset()
    {
        Explored = false;
    }

    #endregion
}
