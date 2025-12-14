using Microsoft.Extensions.Logging;
using Moq;
using Mud.DataStructures.Flags;
using Mud.POC.Flags;
using Mud.POC.Tests.Flags.Stub;
using System.Text;

namespace Mud.POC.Tests.Flags
{
    [TestClass]
    public class FlagsManagerTests
    {
        [TestMethod]
        public void MissingAttribute()
        {
            var loggerMock = new Mock<ILogger<FlagsManager>>();
            var flagValues = new List<IFlagValues>
            {
                new Flags1Definition1(),
                new Flags1Definition2(),
                new Flags2Definition1(),
                new Flags2Definition2() // missing attribute
            };

            var ex = Assert.Throws<Exception>(() => new FlagsManager(loggerMock.Object, flagValues));
            Assert.IsNotNull(ex);
            Assert.AreEqual($"FlagManager: no DefineFlagValuesAttribute found for DefineFlagValues {typeof(Flags2Definition2).FullName}", ex.Message);
        }

        [TestMethod]
        public void InvalidFlagInterfaceTypeOnAttribute()
        {
            var loggerMock = new Mock<ILogger<FlagsManager>>();
            var flagValues = new List<IFlagValues>
            {
                new Flags1Definition1(),
                new Flags1Definition2(),
                new Flags2Definition1(),
                new Flags2Definition3() // invalid flag interface type in attribute
            };

            var ex = Assert.Throws<Exception>(() => new FlagsManager(loggerMock.Object, flagValues));
            Assert.IsNotNull(ex);
            Assert.AreEqual($"FlagManager: FlagInterfaceType {typeof(string)} in DefineFlagValues {typeof(Flags2Definition3).FullName} doesn't inherit from {typeof(IFlags<string>).FullName}", ex.Message);
        }

        [TestMethod]
        public void DuplicateDefinition()
        {
            var loggerMock = new Mock<ILogger<FlagsManager>>();
            var flagValues = new List<IFlagValues>
            {
                new Flags1Definition1(),
                new Flags1Definition2()
            };
            var manager = new FlagsManager(loggerMock.Object, flagValues);

            loggerMock.Verify(x => x.Log(
                It.Is<LogLevel>(logLevel => logLevel == LogLevel.Error),
                It.Is<EventId>(eventId => eventId.Id == 0),
                It.Is<It.IsAnyType>((@object, @type) => @object.ToString() == $"FlagManager: Flag flag1_1 found in DefineFlagValues {typeof(Flags1Definition2).FullName} is already defined"),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [TestMethod]
        public void CheckFlags_UnknownFlags()
        {
            var loggerMock = new Mock<ILogger<FlagsManager>>();
            var flagValues = new List<IFlagValues>
            {
                new Flags1Definition1(),
                new Flags1Definition2(),
                new Flags2Definition1()
            };
            var manager = new FlagsManager(loggerMock.Object, flagValues);
            var flags1 = new Flags1();
            flags1.Set("flag1_2", "unknown", "flag1_6");

            var valid = manager.CheckFlags<IFlags1>(flags1);
            Assert.IsFalse(valid);
            loggerMock.Verify(x => x.Log(
                It.Is<LogLevel>(logLevel => logLevel == LogLevel.Error),
                It.Is<EventId>(eventId => eventId.Id == 0),
                It.Is<It.IsAnyType>((@object, @type) => @object.ToString() == $"FlagManager: 'unknown' not found in {typeof(IFlags1).FullName}"),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [TestMethod]
        public void CheckFlags_ValidFlags()
        {
            var loggerMock = new Mock<ILogger<FlagsManager>>();
            var flagValues = new List<IFlagValues>
            {
                new Flags1Definition1(),
                new Flags1Definition2(),
                new Flags2Definition1()
            };
            var manager = new FlagsManager(loggerMock.Object, flagValues);
            var flags1 = new Flags1();
            flags1.Set("flag1_2", "flag1_6");

            var valid = manager.CheckFlags<IFlags1>(flags1);
            Assert.IsTrue(valid);
        }

        [TestMethod]
        public void Append()
        {
            var loggerMock = new Mock<ILogger<FlagsManager>>();
            var flagValues = new List<IFlagValues>
            {
                new Flags1Definition1(),
                new Flags1Definition2(),
                new Flags2Definition1()
            };
            var manager = new FlagsManager(loggerMock.Object, flagValues);
            var flags1 = new Flags1();
            flags1.Set("flag1_2", "unknown", "flag1_6");

            var sb = new StringBuilder();
            manager.Append<IFlags1>(sb, flags1, false);

            Assert.AreEqual("(Flag12)(Flag1_6)", sb.ToString());
            loggerMock.Verify(x => x.Log(
                It.Is<LogLevel>(logLevel => logLevel == LogLevel.Error),
                It.Is<EventId>(eventId => eventId.Id == 0),
                It.Is<It.IsAnyType>((@object, @type) => @object.ToString() == $"FlagManager: flag unknown not found in {typeof(IFlags1).FullName}"),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
