using Mud.Server.Blueprints.LootTable;

namespace Mud.Server.Interfaces.World;

public interface IWorld
{
    IReadOnlyCollection<TreasureTable<int>> TreasureTables { get; }

    void AddTreasureTable(TreasureTable<int> table);

    void FixWorld(); // should be called before the first ResetWorld
    void ResetWorld();

    void Cleanup(); // called once outputs has been processed (before next loop)
}
