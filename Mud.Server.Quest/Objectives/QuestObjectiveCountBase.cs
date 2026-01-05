using Mud.Server.Interfaces.Quest;

namespace Mud.Server.Quest.Objectives;

public abstract class QuestObjectiveCountBase : IQuestObjective
{
    public abstract string TargetName { get; }
    public int Count { get; set; }
    public int Total { get; set; }

    #region IQuestObjective

    public int Id { get; set; }

    public bool IsCompleted => Count >= Total;

    public string CompletionState => IsCompleted
        ? $"{TargetName,-20}: complete ({Count,3} / {Total,3})"
        : $"{TargetName,-20}: {Count,3} / {Total,3} ({Count * 100 / Total:D}%)";

    public void Reset()
    {
        Count = 0;
    }

    #endregion
}
