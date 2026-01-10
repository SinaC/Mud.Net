namespace Mud.Server.Interfaces.Quest;

public interface IQuestObjective
{
    int Id { get; }
    bool IsCompleted { get; }
    string CompletionState { get; }

    void Reset();
}
