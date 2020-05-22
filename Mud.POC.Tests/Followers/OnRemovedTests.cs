using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Mud.POC.GroupsPetsFollowers;

namespace Mud.POC.Tests.Followers
{
    [TestClass]
    public class OnRemovedTests
    {
        [TestMethod]
        public void OnRemoved_Leader()
        {
            ICharacter player1 = new PlayableCharacter("player1", new Mock<IRoom>().Object);
            ICharacter mob1 = new NonPlayableCharacter("mob1", new Mock<IRoom>().Object);
            player1.AddFollower(mob1);

            player1.OnRemoved();

            Assert.IsNull(player1.Leader);
            Assert.IsNull(mob1.Leader);
        }

        [TestMethod]
        public void OnRemoved_NotTheLeader()
        {
            ICharacter player1 = new PlayableCharacter("player1", new Mock<IRoom>().Object);
            ICharacter mob1 = new NonPlayableCharacter("mob1", new Mock<IRoom>().Object);
            player1.AddFollower(mob1);

            mob1.OnRemoved();

            Assert.IsNull(player1.Leader);
            Assert.IsNull(mob1.Leader);
        }
    }
}
