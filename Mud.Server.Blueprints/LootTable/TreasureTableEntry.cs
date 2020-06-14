﻿using Mud.Server.Random;

namespace Mud.Server.Blueprints.LootTable
{
    public class TreasureTableEntry<T> : IOccurancy<T>
    {
        public T Value { get; set; }
        public int Occurancy { get; set; }
        public int MaxOccurancy { get; set; } // maximum occurancy of this item in whole loot table

        public TreasureTableEntry()
        {
            MaxOccurancy = 1;
        }
    }
}
