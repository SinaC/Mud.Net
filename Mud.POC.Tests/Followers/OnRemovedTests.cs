using System.Linq;
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

            Assert.IsNull(player1.Follows);
            Assert.IsNull(mob1.Follows);
            Assert.AreEqual(0, player1.Followers.Count());
            Assert.AreEqual(0, mob1.Followers.Count());
        }

        [TestMethod]
        public void OnRemoved_NotTheLeader()
        {
            ICharacter player1 = new PlayableCharacter("player1", new Mock<IRoom>().Object);
            ICharacter mob1 = new NonPlayableCharacter("mob1", new Mock<IRoom>().Object);
            player1.AddFollower(mob1);

            mob1.OnRemoved();

            Assert.IsNull(player1.Follows);
            Assert.IsNull(mob1.Follows);
            Assert.AreEqual(0, player1.Followers.Count());
            Assert.AreEqual(0, mob1.Followers.Count());
        }
    }
}
