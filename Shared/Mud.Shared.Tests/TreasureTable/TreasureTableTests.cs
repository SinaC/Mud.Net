using Microsoft.Extensions.Logging;
using Moq;
using Mud.Blueprints.LootTable;
using Mud.Random;

namespace Mud.Shared.Tests.TreasureTable;

[TestClass]
public class TreasureTableTests
{
    [TestMethod]
    public void ThreeTables()
    {
        var treasureTableLoggerMock = new Mock<ILogger<TreasureTable<int>>>();
        var characterLootTableMock = new Mock<ILogger<CharacterLootTable<int>>>();

        var randomManager = new RandomManager(0);
        //
        var tableNormalLoot = new TreasureTable<int>(treasureTableLoggerMock.Object, randomManager)
        {
            Name = "TreasureList_NormalLoot",
            Entries =
            [
                new()
                {
                    Value = 3360, // standard sleeves
                    Occurancy = 25, // 25/(25+65+10) -> 25%
                    MaxInstance = 1
                },
                new()
                {
                    Value = 3011, // bread
                    Occurancy = 65, // 65/(25+65+10) -> 65%
                    MaxInstance = 1
                },
                new()
                {
                    Value = 3365, // war banner
                    Occurancy = 10, // 10/(25+65+10) -> 10%
                    MaxInstance = 1
                }
            ]
        };
        var tableRareLoot = new TreasureTable<int>(treasureTableLoggerMock.Object, randomManager)
        {
            Name = "TreasureList_RareLoot",
            Entries =
            [
                new()
                {
                    Value = 3133, // city key
                    Occurancy = 1, // 1/1 -> 100% because no other entries
                    MaxInstance = 1,
                }
            ]
        };
        var tableEmpty = new TreasureTable<int>(treasureTableLoggerMock.Object, randomManager)
        {
            Name = "TreasureList_Empty"
        };
        var fidoTable = new CharacterLootTable<int>(characterLootTableMock.Object, randomManager)
        {
            MinLoot = 1,
            MaxLoot = 3,
            Entries =
            [
                new()
                {
                    Value = tableNormalLoot,
                    Occurancy = 45, // 45/(45+5+50) -> 45%
                    MaxLootCount = 2
                },
                new()
                {
                    Value = tableRareLoot,
                    Occurancy = 5, // 5/(45+5+50) -> 5%
                    MaxLootCount = 1
                },
            new() {
                Value = tableEmpty,
                Occurancy = 50, // 50/(45+5+50) -> 50%
                MaxLootCount = 1
            }
            ]
        };

        var loots = fidoTable.GenerateLoots();

        Assert.IsNotNull(loots);
    }
}
