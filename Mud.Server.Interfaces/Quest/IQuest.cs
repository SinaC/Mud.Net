using Mud.Domain;
using Mud.Server.Blueprints.Quest;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Entity;
using Mud.Server.Interfaces.Item;
using Mud.Server.Interfaces.Room;

namespace Mud.Server.Interfaces.Quest;

public interface IQuest
{
    void Initialize(QuestBlueprint blueprint, IPlayableCharacter character, INonPlayableCharacter giver); // TODO: giver should be ICharacterQuestor
    void Initialize(CurrentQuestData questData, IPlayableCharacter character);

    QuestBlueprint Blueprint { get; }

    IPlayableCharacter Character { get; }
    INonPlayableCharacter Giver { get; } // TODO: quest may be ended with a different NPC

    bool IsCompleted { get; }
    DateTime StartTime { get; }
    int PulseLeft { get; }
    DateTime? CompletionTime { get; }

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

    CurrentQuestData MapQuestData();
}

public interface IQuestObjective
{
    int Id { get; }
    bool IsCompleted { get; }
    string CompletionState { get; }

    void Reset();
}
