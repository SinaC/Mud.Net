using Mud.Random;

namespace Mud.Blueprints.LootTable;

public class TreasureTableEntry<T> : IOccurancy<T>
    where T: notnull
{
    public T Value { get; set; } = default!;
    public int Occurancy { get; set; }
    public int MaxInstance { get; set; } // maximum instance count of this item in whole loot table

    public TreasureTableEntry()
    {
        MaxInstance = 1;
    }
}
