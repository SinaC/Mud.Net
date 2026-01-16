using Microsoft.Extensions.Logging;
using Moq;
using Mud.Blueprints.Item;
using Mud.Flags;
using Mud.Server.Interfaces.Item;
using Mud.Server.Interfaces.Room;
using Mud.Server.Item;
using Mud.Server.Options;

namespace Mud.Server.Tests.Items;

[TestClass]
public class StaffTests : TestBase
{
    [TestMethod]
    public void Staff_Creation_Values()
    {
        var blueprint = new ItemStaffBlueprint
        {
            Id = 1,
            Name = "Staff",
            ShortDescription = "StaffShort",
            Description = "StaffDesc",
            ItemFlags = new ItemFlags("AntiEvil"),
            MaxChargeCount = 10,
            CurrentChargeCount = 7,
            AlreadyRecharged = false
        };
        var staff = GenerateStaff(blueprint);

        Assert.AreEqual(blueprint.MaxChargeCount, staff.MaxChargeCount);
        Assert.AreEqual(blueprint.CurrentChargeCount, staff.CurrentChargeCount);
        Assert.AreEqual(blueprint.AlreadyRecharged, staff.AlreadyRecharged);
    }

    [TestMethod]
    public void Staff_Use()
    {
        var blueprint = new ItemStaffBlueprint
        {
            Id = 1,
            Name = "Staff",
            ShortDescription = "StaffShort",
            Description = "StaffDesc",
            ItemFlags = new ItemFlags("AntiEvil"),
            MaxChargeCount = 10,
            CurrentChargeCount = 7,
            AlreadyRecharged = false
        };
        var staff = GenerateStaff(blueprint);

        staff.Use();

        Assert.AreEqual(10, staff.MaxChargeCount);
        Assert.AreEqual(6, staff.CurrentChargeCount);
        Assert.IsFalse(staff.AlreadyRecharged);
    }

    [TestMethod]
    public void Staff_Recharge()
    {
        var blueprint = new ItemStaffBlueprint
        {
            Id = 1,
            Name = "Staff",
            ShortDescription = "StaffShort",
            Description = "StaffDesc",
            ItemFlags = new ItemFlags("AntiEvil"),
            MaxChargeCount = 10,
            CurrentChargeCount = 7,
            AlreadyRecharged = false
        };
        var staff = GenerateStaff(blueprint);

        staff.Recharge(7, 9);

        Assert.AreEqual(9, staff.MaxChargeCount);
        Assert.AreEqual(7, staff.CurrentChargeCount);
        Assert.IsTrue(staff.AlreadyRecharged);
    }

    private static IItemStaff GenerateStaff(ItemStaffBlueprint staffBlueprint)
    {
        var loggerMock = new Mock<ILogger<ItemStaff>>();
        var messageForwardOptions = Microsoft.Extensions.Options.Options.Create(new MessageForwardOptions { ForwardSlaveMessages = false, PrefixForwardedMessages = false });
        var worldOptions = Microsoft.Extensions.Options.Options.Create(new WorldOptions { MaxLevel = 60, BlueprintIds = null! });
        var roomMock = new Mock<IRoom>();

        var staff = new ItemStaff(loggerMock.Object, null!, null!, messageForwardOptions, worldOptions, null!, null!, null!);
        staff.Initialize(Guid.NewGuid(), staffBlueprint, roomMock.Object);

        return staff;
    }
}
