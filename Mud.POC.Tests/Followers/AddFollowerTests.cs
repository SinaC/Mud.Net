using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Mud.POC.GroupsPetsFollowers;

namespace Mud.POC.Tests.Followers
{
    [TestClass]
    public class AddFollowerTests
    {
        [TestMethod]
        public void AddFollower()
        {
            ICharacter player1 = new PlayableCharacter("player1", new Mock<IRoom>().Object);
            ICharacter mob1 = new NonPlayableCharacter("mob1", new Mock<IRoom>().Object);

            player1.AddFollower(mob1);

            Assert.IsNotNull(mob1.Leader);
            Assert.IsNull(player1.Leader);
            Assert.AreSame(player1, mob1.Leader);
        }

        [TestMethod]
        public void AddFollower_Duplicate()
        {
            ICharacter player1 = new PlayableCharacter("player1", new Mock<IRoom>().Object);
            ICharacter mob1 = new NonPlayableCharacter("mob1", new Mock<IRoom>().Object);
            player1.AddFollower(mob1);

            player1.AddFollower(mob1);

            Assert.IsNotNull(mob1.Leader);
            Assert.IsNull(player1.Leader);
            Assert.AreSame(player1, mob1.Leader);
        }

        [TestMethod]
        public void AddFollower_WasFollowingAnother()
        {
            ICharacter player1 = new PlayableCharacter("player1", new Mock<IRoom>().Object);
            ICharacter mob1 = new NonPlayableCharacter("mob1", new Mock<IRoom>().Object);
            ICharacter mob2 = new NonPlayableCharacter("mob2", new Mock<IRoom>().Object);
            mob1.AddFollower(mob2);

            player1.AddFollower(mob2);

            Assert.IsNull(mob1.Leader);
            Assert.IsNull(mob1.Leader);
            Assert.IsNotNull(mob2.Leader);
            Assert.AreSame(player1, mob2.Leader);
        }

        [TestMethod]
        public void AddFollower_SimpleCycle()
        {
            ICharacter player1 = new PlayableCharacter("player1", new Mock<IRoom>().Object);
            ICharacter mob1 = new NonPlayableCharacter("mob1", new Mock<IRoom>().Object);
            player1.AddFollower(mob1);

            mob1.AddFollower(player1);

            Assert.IsNotNull(mob1.Leader);
            Assert.IsNull(player1.Leader);
            Assert.AreSame(player1, mob1.Leader);
        }

        [TestMethod]
        public void AddFollower_ComplexCycle() // mob3 -> mob2 -> mob1 -> player1
        {
            ICharacter player1 = new PlayableCharacter("player1", new Mock<IRoom>().Object);
            ICharacter mob1 = new NonPlayableCharacter("mob1", new Mock<IRoom>().Object);
            ICharacter mob2 = new NonPlayableCharacter("mob2", new Mock<IRoom>().Object);
            ICharacter mob3 = new NonPlayableCharacter("mob3", new Mock<IRoom>().Object);
            player1.AddFollower(mob1);
            mob1.AddFollower(mob2);
            mob2.AddFollower(mob3);

            mob3.AddFollower(player1);

            Assert.IsNull(player1.Leader);
            Assert.IsNotNull(mob1.Leader);
            Assert.IsNotNull(mob2.Leader);
            Assert.IsNotNull(mob3.Leader);
        }
    }
}
