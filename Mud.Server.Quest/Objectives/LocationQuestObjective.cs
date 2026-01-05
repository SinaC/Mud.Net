using Mud.Blueprints.Room;
using Mud.Server.Interfaces.Quest;

namespace Mud.Server.Quest.Objectives;

public class LocationQuestObjective : IQuestObjective
{
    public required RoomBlueprint Blueprint { get; set; }
    public bool Explored { get; set; }

    #region IQuestObjective

    public int Id { get; set; }

    public bool IsCompleted => Explored;

    public string CompletionState => IsCompleted
        ? $"{Blueprint.Name,-20}: explored"
        : $"{Blueprint.Name,-20}: not explored";

    public void Reset()
    {
        Explored = false;
    }

    #endregion
}
