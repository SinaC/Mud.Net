using System;
using System.Collections.Generic;
using Mud.Domain;
using Mud.Server.Blueprints.Quest;
using Mud.Server.Item;

namespace Mud.Server
{
    public interface IQuest
    {
        QuestBlueprint Blueprint { get; }

        IPlayableCharacter Character { get; }
        INonPlayableCharacter Giver { get; } // TODO: quest may be ended with a different NPC

        IEnumerable<IQuestObjective> Objectives { get; }
        void GenerateKillLoot(INonPlayableCharacter victim, IContainer container);
        void Update(INonPlayableCharacter victim);
        void Update(IItemQuest item, bool force);
        void Update(IRoom room);
        void Reset();
        void Timeout();
        bool DecreasePulseLeft(int pulseCount); // true if timed out

        bool IsCompleted { get; }
        DateTime StartTime { get; }
        int PulseLeft { get; }
        DateTime? CompletionTime { get; }
        void Complete();
        void Abandon();

        CurrentQuestData GenerateQuestData();
    }

    public interface IQuestObjective
    {
        int Id { get; }
        bool IsCompleted { get; }
        string CompletionState { get; }

        void Reset();
    }
}
