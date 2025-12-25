using Microsoft.Extensions.Logging;
using Moq;
using Mud.Blueprints.Item;
using Mud.Server.Interfaces.Item;
using Mud.Server.Interfaces.Room;
using Mud.Server.Item;
using Mud.Server.Options;

namespace Mud.Server.Tests.Items;

[TestClass]
public class FountainTests : TestBase
{
    [TestMethod]
    public void Fountain_Creation_Values()
    {
        var fountainBlueprint = new ItemFountainBlueprint
        {
            Id = 1,
            Name = "fountain",
            ShortDescription = "FountainShort",
            Description = "FountainDesc",
            LiquidType = "water"
        };
        var fountain = GenerateFountain(fountainBlueprint);

        Assert.AreEqual(fountainBlueprint.LiquidType, fountain.LiquidName);
        Assert.AreEqual(3, fountain.LiquidAmountMultiplier);
        Assert.AreEqual(int.MaxValue, fountain.LiquidLeft);
        Assert.IsFalse(fountain.IsEmpty);
    }

    [TestMethod]
    public void Fountain_Drink_Infinite()
    {
        var fountainBlueprint = new ItemFountainBlueprint
        {
            Id = 1,
            Name = "fountain",
            ShortDescription = "FountainShort",
            Description = "FountainDesc",
            LiquidType = "water"
        };
        var fountain = GenerateFountain(fountainBlueprint);

        fountain.Drink(10000000);

        Assert.AreEqual(fountainBlueprint.LiquidType, fountain.LiquidName);
        Assert.AreEqual(3, fountain.LiquidAmountMultiplier);
        Assert.AreEqual(int.MaxValue, fountain.LiquidLeft);
        Assert.IsFalse(fountain.IsEmpty);
    }

    private static IItemFountain GenerateFountain(ItemFountainBlueprint fountainBlueprint)
    {
        var loggerMock = new Mock<ILogger<ItemFountain>>();
        var messageForwardOptions = Microsoft.Extensions.Options.Options.Create(new MessageForwardOptions { ForwardSlaveMessages = false, PrefixForwardedMessages = false });
        var roomMock = new Mock<IRoom>();

        var fountain = new ItemFountain(loggerMock.Object, null!, null!, null!, messageForwardOptions, null!, null!);
        fountain.Initialize(Guid.NewGuid(), fountainBlueprint, roomMock.Object);

        return fountain;
    }
}
