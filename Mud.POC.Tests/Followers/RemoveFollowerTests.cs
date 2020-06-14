using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Mud.POC.GroupsPetsFollowers;

namespace Mud.POC.Tests.Followers
{
    [TestClass]
    public class RemoveFollowerTests : FollowerTestBase
    {
        [TestMethod]
        public void RemoveFollower()
        {
            ICharacter player1 = new PlayableCharacter("player1", new Mock<IRoom>().Object);
            ICharacter mob1 = new NonPlayableCharacter("mob1", new Mock<IRoom>().Object);
            player1.AddFollower(mob1);

            player1.RemoveFollower(mob1);

            Assert.IsNull(mob1.Leader);
            Assert.IsNull(player1.Leader);
        }

        [TestMethod]
        public void RemoveFollower_WasNotFollowing()
        {
            ICharacter player1 = new PlayableCharacter("player1", new Mock<IRoom>().Object);
            ICharacter mob1 = new NonPlayableCharacter("mob1", new Mock<IRoom>().Object);

            player1.RemoveFollower(mob1);

            Assert.IsNull(mob1.Leader);
            Assert.IsNull(player1.Leader);
        }
    }
}
