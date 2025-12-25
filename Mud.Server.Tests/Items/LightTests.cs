using Microsoft.Extensions.Logging;
using Moq;
using Mud.Blueprints.Item;
using Mud.Server.Flags;
using Mud.Server.Interfaces.Item;
using Mud.Server.Interfaces.Room;
using Mud.Server.Item;
using Mud.Server.Options;

namespace Mud.Server.Tests.Items;

[TestClass]
public class LightTests : TestBase
{
    [TestMethod]
    public void Light_Creation_Values()
    {
        var blueprint = new ItemLightBlueprint
        {
            Id = 1,
            Name = "Light",
            ShortDescription = "LightShort",
            Description = "LightDesc",
            ItemFlags = new ItemFlags("AntiEvil"),
            DurationHours = 60,
        };
        var light = GenerateLight(blueprint);

        Assert.AreEqual(blueprint.DurationHours * 60, light.TimeLeft);
        Assert.IsTrue(light.IsLighten);
        Assert.IsFalse(light.IsInfinite);
    }

    [TestMethod]
    public void Light_Infinite_Creation_Values()
    {
        var blueprint = new ItemLightBlueprint
        {
            Id = 1,
            Name = "Light",
            ShortDescription = "LightShort",
            Description = "LightDesc",
            ItemFlags = new ItemFlags("AntiEvil"),
            DurationHours = -1,
        };
        var light = GenerateLight(blueprint);

        Assert.AreEqual(-1, light.TimeLeft);
        Assert.IsTrue(light.IsLighten);
        Assert.IsTrue(light.IsInfinite);
    }

    [TestMethod]
    public void Light_DecreaseTimeLeft_Values()
    {
        var blueprint = new ItemLightBlueprint
        {
            Id = 1,
            Name = "Light",
            ShortDescription = "LightShort",
            Description = "LightDesc",
            ItemFlags = new ItemFlags("AntiEvil"),
            DurationHours = 30,
        };
        var light = GenerateLight(blueprint);

        light.DecreaseTimeLeft();

        Assert.AreEqual(29 * 60 + 59, light.TimeLeft);
        Assert.IsTrue(light.IsLighten);
        Assert.IsFalse(light.IsInfinite);
    }

    [TestMethod]
    public void Light_DecreaseTimeLeft_0Left_Values()
    {
        var blueprint = new ItemLightBlueprint
        {
            Id = 1,
            Name = "Light",
            ShortDescription = "LightShort",
            Description = "LightDesc",
            ItemFlags = new ItemFlags("AntiEvil"),
            DurationHours = 1,
        };
        var light = GenerateLight(blueprint);

        for (int i = 0; i < 60; i++)
            light.DecreaseTimeLeft();

        Assert.AreEqual(0, light.TimeLeft);
        Assert.IsFalse(light.IsLighten);
        Assert.IsFalse(light.IsInfinite);
    }

    private static IItemLight GenerateLight(ItemLightBlueprint lightBlueprint)
    {
        var loggerMock = new Mock<ILogger<ItemLight>>();
        var messageForwardOptions = Microsoft.Extensions.Options.Options.Create(new MessageForwardOptions { ForwardSlaveMessages = false, PrefixForwardedMessages = false });
        var roomMock = new Mock<IRoom>();

        var light = new ItemLight(loggerMock.Object, null!, null!, null!, messageForwardOptions, null!, null!);
        light.Initialize(Guid.NewGuid(), lightBlueprint, roomMock.Object);

        return light;
    }
}
