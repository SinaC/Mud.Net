using System;

namespace Mud.Server.Blueprints.Quest
{
    public class QuestKillLootTableEntry<T>
        where T:IEquatable<T>
    {
        public T Value { get; set; }
        public int Percentage { get; set; } // from 0 to 100
    }
}
