using Microsoft.Extensions.Logging;
using Moq;
using Mud.Server.Blueprints.Item;
using Mud.Server.Flags.Interfaces;
using Mud.Server.Interfaces.Item;
using Mud.Server.Interfaces.Room;
using Mud.Server.Item;
using Mud.Server.Options;

namespace Mud.Server.Tests.Items
{
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
                ItemFlags = CreateItemFlags("AntiEvil"),
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
                ItemFlags = CreateItemFlags("AntiEvil"),
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
                ItemFlags = CreateItemFlags("AntiEvil"),
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
            var itemFlagsFactory = new Mock<IFlagFactory<IItemFlags, IItemFlagValues>>();
            var roomMock = new Mock<IRoom>();

            itemFlagsFactory.Setup(x => x.CreateInstance(It.IsAny<string[]>())).Returns<string[]>(flags => CreateItemFlags(flags));

            var staff = new ItemStaff(loggerMock.Object, null, null, null, messageForwardOptions, null, null, itemFlagsFactory.Object);
            staff.Initialize(Guid.NewGuid(), staffBlueprint, roomMock.Object);

            return staff;
        }
    }
}
