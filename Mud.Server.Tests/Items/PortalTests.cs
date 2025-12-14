using Microsoft.Extensions.Logging;
using Moq;
using Mud.Domain;
using Mud.Server.Blueprints.Item;
using Mud.Server.Blueprints.Room;
using Mud.Server.Flags.Interfaces;
using Mud.Server.Interfaces.Item;
using Mud.Server.Interfaces.Room;
using Mud.Server.Item;
using Mud.Server.Options;

namespace Mud.Server.Tests.Items
{
    [TestClass]
    public class PortalTests : TestBase
    {
        [TestMethod]
        public void Portal_Creation_Values()
        {
            var portalBlueprint = new ItemPortalBlueprint
            {
                Id = 1,
                Name = "portal",
                ShortDescription = "PortalShort",
                Description = "PortalDesc",
                Destination = 123,
                Key = 10,
                PortalFlags = PortalFlags.Closed | PortalFlags.PickProof,
                MaxChargeCount = 10,
                CurrentChargeCount = 7,
            };
            var portal = GeneratePortal(portalBlueprint);

            Assert.AreEqual(portalBlueprint.PortalFlags, portal.PortalFlags);
            Assert.AreEqual(portalBlueprint.MaxChargeCount, portal.MaxChargeCount);
            Assert.AreEqual(portalBlueprint.CurrentChargeCount, portal.CurrentChargeCount);
        }

        [TestMethod]
        public void Portal_Use()
        {
            var portalBlueprint = new ItemPortalBlueprint
            {
                Id = 1,
                Name = "portal",
                ShortDescription = "PortalShort",
                Description = "PortalDesc",
                Destination = 123,
                Key = 10,
                PortalFlags = PortalFlags.Closed | PortalFlags.PickProof,
                MaxChargeCount = 10,
                CurrentChargeCount = 7,
            };
            var portal = GeneratePortal(portalBlueprint);

            portal.Use();

            Assert.AreEqual(portalBlueprint.PortalFlags, portal.PortalFlags);
            Assert.AreEqual(portalBlueprint.MaxChargeCount, portal.MaxChargeCount);
            Assert.AreEqual(portalBlueprint.CurrentChargeCount - 1, portal.CurrentChargeCount);
        }

        [TestMethod]
        public void Portal_Use_InfiniteCharges()
        {
            var portalBlueprint = new ItemPortalBlueprint
            {
                Id = 1,
                Name = "portal",
                ShortDescription = "PortalShort",
                Description = "PortalDesc",
                Destination = 123,
                Key = 10,
                PortalFlags = PortalFlags.Closed | PortalFlags.PickProof,
                MaxChargeCount = -1,
                CurrentChargeCount = 7,
            };
            var portal = GeneratePortal(portalBlueprint);

            portal.Use();

            Assert.AreEqual(portalBlueprint.PortalFlags, portal.PortalFlags);
            Assert.AreEqual(portalBlueprint.MaxChargeCount, portal.MaxChargeCount);
            Assert.AreEqual(portalBlueprint.CurrentChargeCount, portal.CurrentChargeCount);
        }

        [TestMethod]
        public void Portal_Open_NoClosed()
        {
            var portalBlueprint = new ItemPortalBlueprint
            {
                Id = 1,
                Name = "portal",
                ShortDescription = "PortalShort",
                Description = "PortalDesc",
                Destination = 123,
                Key = 10,
                PortalFlags = PortalFlags.PickProof,
            };
            var portal = GeneratePortal(portalBlueprint);

            portal.Open();

            Assert.AreEqual(portalBlueprint.PortalFlags, portal.PortalFlags);
            Assert.AreEqual(portalBlueprint.MaxChargeCount, portal.MaxChargeCount);
            Assert.AreEqual(portalBlueprint.CurrentChargeCount, portal.CurrentChargeCount);
        }

        [TestMethod]
        public void Portal_Open()
        {
            var portalBlueprint = new ItemPortalBlueprint
            {
                Id = 1,
                Name = "portal",
                ShortDescription = "PortalShort",
                Description = "PortalDesc",
                Destination = 123,
                Key = 10,
                PortalFlags = PortalFlags.Closed | PortalFlags.PickProof,
            };
            var portal = GeneratePortal(portalBlueprint);

            portal.Open();

            Assert.AreEqual(portalBlueprint.PortalFlags & ~PortalFlags.Closed, portal.PortalFlags);
            Assert.AreEqual(portalBlueprint.MaxChargeCount, portal.MaxChargeCount);
            Assert.AreEqual(portalBlueprint.CurrentChargeCount, portal.CurrentChargeCount);
        }

        [TestMethod]
        public void Portal_Close_NoCloseable()
        {
            var portalBlueprint = new ItemPortalBlueprint
            {
                Id = 1,
                Name = "portal",
                ShortDescription = "PortalShort",
                Description = "PortalDesc",
                Destination = 123,
                Key = 10,
                PortalFlags = PortalFlags.PickProof | PortalFlags.NoClose,
            };
            var portal = GeneratePortal(portalBlueprint);

            portal.Close();

            Assert.AreEqual(portalBlueprint.PortalFlags, portal.PortalFlags);
            Assert.AreEqual(portalBlueprint.MaxChargeCount, portal.MaxChargeCount);
            Assert.AreEqual(portalBlueprint.CurrentChargeCount, portal.CurrentChargeCount);
        }

        [TestMethod]
        public void Portal_Close()
        {
            var portalBlueprint = new ItemPortalBlueprint
            {
                Id = 1,
                Name = "portal",
                ShortDescription = "PortalShort",
                Description = "PortalDesc",
                Destination = 123,
                Key = 10,
                PortalFlags = PortalFlags.PickProof,
            };
            var portal = GeneratePortal(portalBlueprint);

            portal.Close();

            Assert.AreEqual(portalBlueprint.PortalFlags | PortalFlags.Closed, portal.PortalFlags);
            Assert.AreEqual(portalBlueprint.MaxChargeCount, portal.MaxChargeCount);
            Assert.AreEqual(portalBlueprint.CurrentChargeCount, portal.CurrentChargeCount);
        }

        [TestMethod]
        public void Portal_Lock_NoLockable()
        {
            var portalBlueprint = new ItemPortalBlueprint
            {
                Id = 1,
                Name = "portal",
                ShortDescription = "PortalShort",
                Description = "PortalDesc",
                Destination = 123,
                Key = 10,
                PortalFlags = PortalFlags.Closed | PortalFlags.NoLock | PortalFlags.PickProof,
            };
            var portal = GeneratePortal(portalBlueprint);

            portal.Lock();

            Assert.AreEqual(portalBlueprint.PortalFlags, portal.PortalFlags);
            Assert.AreEqual(portalBlueprint.MaxChargeCount, portal.MaxChargeCount);
            Assert.AreEqual(portalBlueprint.CurrentChargeCount, portal.CurrentChargeCount);
        }

        [TestMethod]
        public void Portal_Lock()
        {
            var portalBlueprint = new ItemPortalBlueprint
            {
                Id = 1,
                Name = "portal",
                ShortDescription = "PortalShort",
                Description = "PortalDesc",
                Destination = 123,
                Key = 10,
                PortalFlags = PortalFlags.Closed | PortalFlags.PickProof,
            };
            var portal = GeneratePortal(portalBlueprint);

            portal.Lock();

            Assert.AreEqual(portalBlueprint.PortalFlags | PortalFlags.Locked, portal.PortalFlags);
            Assert.AreEqual(portalBlueprint.MaxChargeCount, portal.MaxChargeCount);
            Assert.AreEqual(portalBlueprint.CurrentChargeCount, portal.CurrentChargeCount);
        }

        [TestMethod]
        public void Portal_Unlock_NotLocked()
        {
            var portalBlueprint = new ItemPortalBlueprint
            {
                Id = 1,
                Name = "portal",
                ShortDescription = "PortalShort",
                Description = "PortalDesc",
                Destination = 123,
                Key = 10,
                PortalFlags = PortalFlags.Closed | PortalFlags.PickProof,
            };
            var portal = GeneratePortal(portalBlueprint);

            portal.Unlock();

            Assert.AreEqual(portalBlueprint.PortalFlags, portal.PortalFlags);
            Assert.AreEqual(portalBlueprint.MaxChargeCount, portal.MaxChargeCount);
            Assert.AreEqual(portalBlueprint.CurrentChargeCount, portal.CurrentChargeCount);
        }

        [TestMethod]
        public void Portal_Unlock()
        {
            var portalBlueprint = new ItemPortalBlueprint
            {
                Id = 1,
                Name = "portal",
                ShortDescription = "PortalShort",
                Description = "PortalDesc",
                Destination = 123,
                Key = 10,
                PortalFlags = PortalFlags.Closed | PortalFlags.PickProof | PortalFlags.Locked,
            };
            var portal = GeneratePortal(portalBlueprint);

            portal.Unlock();

            Assert.AreEqual(portalBlueprint.PortalFlags & ~PortalFlags.Locked, portal.PortalFlags);
            Assert.AreEqual(portalBlueprint.MaxChargeCount, portal.MaxChargeCount);
            Assert.AreEqual(portalBlueprint.CurrentChargeCount, portal.CurrentChargeCount);
        }

        private static IItemPortal GeneratePortal(ItemPortalBlueprint portalBlueprint)
        {
            var loggerMock = new Mock<ILogger<ItemPortal>>();
            var messageForwardOptions = Microsoft.Extensions.Options.Options.Create(new MessageForwardOptions { ForwardSlaveMessages = false, PrefixForwardedMessages = false });
            var roomManagerMock = new Mock<IRoomManager>();
            var roomMock = new Mock<IRoom>();

            roomMock.SetupGet(x => x.Blueprint).Returns(new RoomBlueprint { Id = 123 });
            roomManagerMock.SetupGet(x => x.Rooms).Returns([roomMock.Object]);

            var portal = new ItemPortal(loggerMock.Object, null!, null!, null!, messageForwardOptions, roomManagerMock.Object, null!);
            portal.Initialize(Guid.NewGuid(), portalBlueprint, roomMock.Object);

            return portal;
        }
    }
}
