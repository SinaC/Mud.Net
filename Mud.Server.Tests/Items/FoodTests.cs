using Microsoft.Extensions.Logging;
using Moq;
using Mud.Blueprints.Item;
using Mud.Server.Interfaces.Item;
using Mud.Server.Interfaces.Room;
using Mud.Server.Item;
using Mud.Server.Options;

namespace Mud.Server.Tests.Items;

[TestClass]
public class FoodTests : TestBase
{
    [TestMethod]
    public void Food_Creation_Values()
    {
        var foodBlueprint = new ItemFoodBlueprint
        {
            Id = 1,
            Name = "Food",
            ShortDescription = "FoodShort",
            Description = "FoodDesc",
            Cost = 20,
            HungerHours = 10,
            FullHours = 20,
            IsPoisoned = true
        };
        var food = GenerateFood(foodBlueprint);

        Assert.AreEqual(20, food.FullHours);
        Assert.AreEqual(10, food.HungerHours);
        Assert.IsTrue(food.IsPoisoned);
    }

    [TestMethod]
    public void Food_Poison()
    {
        var foodBlueprint = new ItemFoodBlueprint
        {
            Id = 1,
            Name = "Food",
            ShortDescription = "FoodShort",
            Description = "FoodDesc",
            Cost = 20,
            HungerHours = 10,
            FullHours = 20,
            IsPoisoned = false
        };
        var food = GenerateFood(foodBlueprint);

        food.Poison();

        Assert.IsTrue(food.IsPoisoned);
    }

    [TestMethod]
    public void Food_Cure()
    {
        var foodBlueprint = new ItemFoodBlueprint
        {
            Id = 1,
            Name = "Food",
            ShortDescription = "FoodShort",
            Description = "FoodDesc",
            Cost = 20,
            HungerHours = 10,
            FullHours = 20,
            IsPoisoned = true
        };
        var food = GenerateFood(foodBlueprint);

        food.Cure();

        Assert.IsFalse(food.IsPoisoned);
    }

    [TestMethod]
    public void Food_SetHours()
    {
        var foodBlueprint = new ItemFoodBlueprint
        {
            Id = 1,
            Name = "Food",
            ShortDescription = "FoodShort",
            Description = "FoodDesc",
            Cost = 20,
            HungerHours = 10,
            FullHours = 20,
            IsPoisoned = true
        };
        var food = GenerateFood(foodBlueprint);

        food.SetHours(10, 15);

        Assert.AreEqual(10, food.FullHours);
        Assert.AreEqual(15, food.HungerHours);
    }

    private static IItemFood GenerateFood(ItemFoodBlueprint foodBlueprint)
    {
        var loggerMock = new Mock<ILogger<ItemFood>>();
        var messageForwardOptions = Microsoft.Extensions.Options.Options.Create(new MessageForwardOptions { ForwardSlaveMessages = false, PrefixForwardedMessages = false });
        var roomMock = new Mock<IRoom>();

        var food = new ItemFood(loggerMock.Object, null!, null!, null!, messageForwardOptions, null!, null!);
        food.Initialize(Guid.NewGuid(), foodBlueprint, roomMock.Object);

        return food;
    }
}
