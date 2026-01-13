using Mud.Blueprints.Quest;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Entity;
using Mud.Server.Interfaces.Item;
using Mud.Server.Interfaces.Room;

namespace Mud.Server.Interfaces.Quest;

public interface IQuest
{
    IPlayableCharacter Character { get; }
    INonPlayableCharacter Giver { get; } // TODO: quest may be ended with a different NPC

    string DebugName { get; }

    string Title { get; }
    string? Description { get; }
    int Level { get; }

    bool AreObjectivesFulfilled { get; }
    int TimeLimit { get; }
    DateTime StartTime { get; }
    int PulseLeft { get; }
    DateTime? CompletionTime { get; }

    IReadOnlyDictionary<int, QuestKillLootTable<int>> KillLootTable { get; } // key: mob blueprint id
    IEnumerable<IQuestObjective> Objectives { get; }

    void GenerateKillLoot(INonPlayableCharacter victim, IContainer container);
    void Update(INonPlayableCharacter victim);
    void Update(IItemQuest item, bool force);
    void Update(IRoom room);
    void Reset();
    void Timeout();
    bool DecreasePulseLeft(int pulseCount); // true if timed out
    void Complete();
    void Abandon();
}