using Microsoft.Extensions.Logging;
using Moq;
using Mud.Blueprints.Item;
using Mud.Server.Interfaces.Item;
using Mud.Server.Interfaces.Room;
using Mud.Server.Item;
using Mud.Server.Options;

namespace Mud.Server.Tests.Items;

[TestClass]
public class DrinkContainerTests : TestBase
{
    // DrinkContainer
    [TestMethod]
    public void DrinkContainer_Creation_Values()
    {
        var drinkContainerBlueprint = new ItemDrinkContainerBlueprint
        {
            Id = 1, Name = "drinkcontainer", ShortDescription = "DrinkContainerShort", Description = "DrinkContainerDesc",
            MaxLiquidAmount = 500,
            CurrentLiquidAmount = 350,
            LiquidType = "water",
            IsPoisoned = true
        };
        var drinkContainer = GenerateDrinkContainer(drinkContainerBlueprint);

        Assert.AreEqual(drinkContainerBlueprint.LiquidType, drinkContainer.LiquidName);
        Assert.AreEqual(1, drinkContainer.LiquidAmountMultiplier);
        Assert.AreEqual(drinkContainerBlueprint.MaxLiquidAmount, drinkContainer.MaxLiquid);
        Assert.AreEqual(drinkContainerBlueprint.CurrentLiquidAmount, drinkContainer.LiquidLeft);
        Assert.IsFalse(drinkContainer.IsEmpty);
        Assert.IsTrue(drinkContainer.IsPoisoned);
    }

    [TestMethod]
    public void DrinkContainer_Drink_LessThanLiquidLeft()
    {
        var drinkContainerBlueprint = new ItemDrinkContainerBlueprint
        {
            Id = 1, Name = "drinkcontainer", ShortDescription = "DrinkContainerShort", Description = "DrinkContainerDesc",
            MaxLiquidAmount = 500,
            CurrentLiquidAmount = 350,
            LiquidType = "water",
            IsPoisoned = true
        };
        var drinkContainer = GenerateDrinkContainer(drinkContainerBlueprint);
        drinkContainer.Drink(100);

        Assert.IsFalse(drinkContainer.IsEmpty);
        Assert.AreEqual(250, drinkContainer.LiquidLeft);
        Assert.AreEqual(500, drinkContainer.MaxLiquid);
    }

    [TestMethod]
    public void DrinkContainer_Drink_MoreThanLiquidLeft()
    {
        var drinkContainerBlueprint = new ItemDrinkContainerBlueprint
        {
            Id = 1, Name = "drinkcontainer", ShortDescription = "DrinkContainerShort", Description = "DrinkContainerDesc",
            MaxLiquidAmount = 500,
            CurrentLiquidAmount = 350,
            LiquidType = "water",
            IsPoisoned = true
        };
        var drinkContainer = GenerateDrinkContainer(drinkContainerBlueprint);
        drinkContainer.Drink(1000);

        Assert.IsTrue(drinkContainer.IsEmpty);
        Assert.AreEqual(0, drinkContainer.LiquidLeft);
        Assert.AreEqual(500, drinkContainer.MaxLiquid);
    }

    [TestMethod]
    public void DrinkContainer_Drink_Poison()
    {
        var drinkContainerBlueprint = new ItemDrinkContainerBlueprint
        {
            Id = 1, Name = "drinkcontainer", ShortDescription = "DrinkContainerShort", Description = "DrinkContainerDesc",
            MaxLiquidAmount = 500,
            CurrentLiquidAmount = 350,
            LiquidType = "water",
            IsPoisoned = false
        };
        var drinkContainer = GenerateDrinkContainer(drinkContainerBlueprint);
        drinkContainer.Poison();

        Assert.IsTrue(drinkContainer.IsPoisoned);
        Assert.AreEqual(350, drinkContainer.LiquidLeft);
        Assert.AreEqual(500, drinkContainer.MaxLiquid);
    }

    [TestMethod]
    public void DrinkContainer_Drink_Cure()
    {
        var drinkContainerBlueprint = new ItemDrinkContainerBlueprint
        {
            Id = 1, Name = "drinkcontainer", ShortDescription = "DrinkContainerShort", Description = "DrinkContainerDesc",
            MaxLiquidAmount = 500,
            CurrentLiquidAmount = 350,
            LiquidType = "water",
            IsPoisoned = true
        };
        var drinkContainer = GenerateDrinkContainer(drinkContainerBlueprint);
        drinkContainer.Cure();

        Assert.IsFalse(drinkContainer.IsPoisoned);
        Assert.AreEqual(350, drinkContainer.LiquidLeft);
        Assert.AreEqual(500, drinkContainer.MaxLiquid);
    }

    [TestMethod]
    public void DrinkContainer_Drink_Filld()
    {
        var drinkContainerBlueprint = new ItemDrinkContainerBlueprint
        {
            Id = 1, Name = "drinkcontainer", ShortDescription = "DrinkContainerShort", Description = "DrinkContainerDesc",
            MaxLiquidAmount = 500,
            CurrentLiquidAmount = 350,
            LiquidType = "water",
            IsPoisoned = true
        };
        var drinkContainer = GenerateDrinkContainer(drinkContainerBlueprint);
        drinkContainer.Fill(500);

        Assert.IsTrue(drinkContainer.IsPoisoned);
        Assert.AreEqual("water", drinkContainer.LiquidName);
        Assert.AreEqual(500, drinkContainer.LiquidLeft);
        Assert.AreEqual(500, drinkContainer.MaxLiquid);
    }

    [TestMethod]
    public void DrinkContainer_Drink_Fill_ChangeLiquid()
    {
        var drinkContainerBlueprint = new ItemDrinkContainerBlueprint
        {
            Id = 1, Name = "drinkcontainer", ShortDescription = "DrinkContainerShort", Description = "DrinkContainerDesc",
            MaxLiquidAmount = 500,
            CurrentLiquidAmount = 350,
            LiquidType = "water",
            IsPoisoned = true
        };
        var drinkContainer = GenerateDrinkContainer(drinkContainerBlueprint);
        drinkContainer.Fill("wine", 500);

        Assert.IsTrue(drinkContainer.IsPoisoned);
        Assert.AreEqual("wine", drinkContainer.LiquidName);
        Assert.AreEqual(500, drinkContainer.LiquidLeft);
        Assert.AreEqual(500, drinkContainer.MaxLiquid);
    }

    [TestMethod]
    public void DrinkContainer_Drink_Pour()
    {
        var drinkContainerBlueprint = new ItemDrinkContainerBlueprint
        {
            Id = 1, Name = "drinkcontainer", ShortDescription = "DrinkContainerShort", Description = "DrinkContainerDesc",
            MaxLiquidAmount = 500,
            CurrentLiquidAmount = 350,
            LiquidType = "water",
            IsPoisoned = true
        };
        var drinkContainer = GenerateDrinkContainer(drinkContainerBlueprint);
        drinkContainer.Pour();

        Assert.IsFalse(drinkContainer.IsPoisoned);
        Assert.AreEqual(0, drinkContainer.LiquidLeft);
        Assert.AreEqual(500, drinkContainer.MaxLiquid);
    }


    private static IItemDrinkContainer GenerateDrinkContainer(ItemDrinkContainerBlueprint drinkContainerBlueprint)
    {
        var loggerMock = new Mock<ILogger<ItemDrinkContainer>>();
        var messageForwardOptions = Microsoft.Extensions.Options.Options.Create(new MessageForwardOptions { ForwardSlaveMessages = false, PrefixForwardedMessages = false });
        var worldOptions = Microsoft.Extensions.Options.Options.Create(new WorldOptions { MaxLevel = 60, UseAggro = false, BlueprintIds = null! });
        var roomMock = new Mock<IRoom>();

        var drinkContainer = new ItemDrinkContainer(loggerMock.Object, null!, null!, messageForwardOptions, worldOptions, null!, null!);
        drinkContainer.Initialize(Guid.NewGuid(), drinkContainerBlueprint, string.Empty, roomMock.Object);

        return drinkContainer;
    }
}
