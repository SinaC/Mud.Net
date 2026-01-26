using Mud.Random;

namespace Mud.Blueprints.LootTable;

public class CharacterLootTableEntry<T> : IOccurancy<TreasureTable<T>>
    where T:IEquatable<T>
{
    public TreasureTable<T> Value { get; set; } = default!;
    public int Occurancy { get; set; }
    public int MaxLootCount { get; set; } // Maximum number of loot from this list allowed for whole loot

    public CharacterLootTableEntry()
    {
        MaxLootCount = 1;
    }
}
