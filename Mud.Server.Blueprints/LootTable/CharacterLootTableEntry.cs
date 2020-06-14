﻿using System;
using Mud.Server.Random;

namespace Mud.Server.Blueprints.LootTable
{
    public class CharacterLootTableEntry<T> : IOccurancy<TreasureTable<T>>
        where T:IEquatable<T>
    {
        public TreasureTable<T> Value { get; set; }
        public int Occurancy { get; set; }
        public int Max { get; set; } // Maximum number of loot from this list allowed for whole loot

        public CharacterLootTableEntry()
        {
            Max = 1;
        }
    }
}
