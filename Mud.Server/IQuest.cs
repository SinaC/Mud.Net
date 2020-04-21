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

        ICharacter Character { get; }
        ICharacter Giver { get; } // TODO: quest may be ended with a different NPC

        IEnumerable<IQuestObjective> Objectives { get; }
        void GenerateKillLoot(ICharacter victim, IContainer container);
        void Update(ICharacter victim);
        void Update(IItemQuest item, bool force);
        void Update(IRoom room);
        void Reset();
        void Timeout();
        bool UpdateSecondsLeft(int seconds); // true if timed out

        bool IsCompleted { get; }
        DateTime StartTime { get; }
        int SecondsLeft { get; }
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
