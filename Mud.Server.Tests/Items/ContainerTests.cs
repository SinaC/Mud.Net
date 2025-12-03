using Microsoft.Extensions.Logging;
using Moq;
using Mud.Domain;
using Mud.Server.Blueprints.Item;
using Mud.Server.Flags.Interfaces;
using Mud.Server.Interfaces.Item;
using Mud.Server.Interfaces.Room;
using Mud.Server.Item;
using Mud.Server.Options;

namespace Mud.Server.Tests.Items
{
    [TestClass]
    public class ContainerTests : TestBase
    {
        [TestMethod]
        public void Container_Open_NoClosed()
        {
            var containerBlueprint = new ItemContainerBlueprint
            {
                Id = 1,
                Name = "container",
                ShortDescription = "PortalShort",
                Description = "PortalDesc",
                Key = 10,
                ContainerFlags = ContainerFlags.PickProof,
            };
            var container = GenerateContainer(containerBlueprint);

            container.Open();

            Assert.AreEqual(containerBlueprint.ContainerFlags, container.ContainerFlags);
        }

        [TestMethod]
        public void Container_Open()
        {
            var containerBlueprint = new ItemContainerBlueprint
            {
                Id = 1,
                Name = "container",
                ShortDescription = "PortalShort",
                Description = "PortalDesc",
                Key = 10,
                ContainerFlags = ContainerFlags.PickProof | ContainerFlags.Closed,
            };
            var container = GenerateContainer(containerBlueprint);

            container.Open();

            Assert.AreEqual(containerBlueprint.ContainerFlags & ~ContainerFlags.Closed, container.ContainerFlags);
        }

        [TestMethod]
        public void Container_Close_NoCloseable()
        {
            var containerBlueprint = new ItemContainerBlueprint
            {
                Id = 1,
                Name = "container",
                ShortDescription = "PortalShort",
                Description = "PortalDesc",
                Key = 10,
                ContainerFlags = ContainerFlags.PickProof | ContainerFlags.NoClose,
            };
            var container = GenerateContainer(containerBlueprint);

            container.Close();

            Assert.AreEqual(containerBlueprint.ContainerFlags, container.ContainerFlags);
        }

        [TestMethod]
        public void Container_Close()
        {
            var containerBlueprint = new ItemContainerBlueprint
            {
                Id = 1,
                Name = "container",
                ShortDescription = "PortalShort",
                Description = "PortalDesc",
                Key = 10,
                ContainerFlags = ContainerFlags.PickProof,
            };
            var container = GenerateContainer(containerBlueprint);

            container.Close();

            Assert.AreEqual(containerBlueprint.ContainerFlags | ContainerFlags.Closed, container.ContainerFlags);
        }

        [TestMethod]
        public void Container_Lock_NoLockable()
        {
            var containerBlueprint = new ItemContainerBlueprint
            {
                Id = 1,
                Name = "container",
                ShortDescription = "PortalShort",
                Description = "PortalDesc",
                Key = 10,
                ContainerFlags = ContainerFlags.PickProof | ContainerFlags.Closed | ContainerFlags.NoLock,
            };
            var container = GenerateContainer(containerBlueprint);

            container.Lock();

            Assert.AreEqual(containerBlueprint.ContainerFlags, container.ContainerFlags);
        }

        [TestMethod]
        public void Container_Lock()
        {
            var containerBlueprint = new ItemContainerBlueprint
            {
                Id = 1,
                Name = "container",
                ShortDescription = "PortalShort",
                Description = "PortalDesc",
                Key = 10,
                ContainerFlags = ContainerFlags.PickProof | ContainerFlags.Closed,
            };
            var container = GenerateContainer(containerBlueprint);

            container.Lock();

            Assert.AreEqual(containerBlueprint.ContainerFlags | ContainerFlags.Locked, container.ContainerFlags);
        }

        [TestMethod]
        public void Container_Unlock_NotLocked()
        {
            var containerBlueprint = new ItemContainerBlueprint
            {
                Id = 1,
                Name = "container",
                ShortDescription = "PortalShort",
                Description = "PortalDesc",
                Key = 10,
                ContainerFlags = ContainerFlags.PickProof | ContainerFlags.Closed,
            };
            var container = GenerateContainer(containerBlueprint);

            container.Unlock();

            Assert.AreEqual(containerBlueprint.ContainerFlags, container.ContainerFlags);
        }

        [TestMethod]
        public void Container_Unlock()
        {
            var containerBlueprint = new ItemContainerBlueprint
            {
                Id = 1,
                Name = "container",
                ShortDescription = "PortalShort",
                Description = "PortalDesc",
                Key = 10,
                ContainerFlags = ContainerFlags.PickProof | ContainerFlags.Closed | ContainerFlags.Locked,
            };
            var container = GenerateContainer(containerBlueprint);

            container.Unlock();

            Assert.AreEqual(containerBlueprint.ContainerFlags & ~ContainerFlags.Locked, container.ContainerFlags);
        }

        private static IItemContainer GenerateContainer(ItemContainerBlueprint containerBlueprint)
        {
            var loggerMock = new Mock<ILogger<ItemContainer>>();
            var messageForwardOptions = Microsoft.Extensions.Options.Options.Create(new MessageForwardOptions { ForwardSlaveMessages = false, PrefixForwardedMessages = false });
            var itemFlagsFactory = new Mock<IFlagFactory<IItemFlags, IItemFlagValues>>();
            var roomMock = new Mock<IRoom>();

            itemFlagsFactory.Setup(x => x.CreateInstance(It.IsAny<string[]>())).Returns<string[]>(flags => CreateItemFlags(flags));

            var container = new ItemContainer(loggerMock.Object, null, null, null, messageForwardOptions, null, null, null, itemFlagsFactory.Object);
            container.Initialize(Guid.NewGuid(), containerBlueprint, roomMock.Object);

            return container;
        }
    }
}
