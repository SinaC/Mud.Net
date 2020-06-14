using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mud.POC.GroupsPetsFollowers;

namespace Mud.POC.Tests.Groups
{
    [TestClass]
    public class OnRemovedTests : GroupTestBase
    {
        [TestMethod]
        public void OnRemoved_MoreThan2Members_NotLeaderOfGroup()
        {
            IRoom room1 = new Room("room1");
            IPlayableCharacter player1 = new PlayableCharacter("player1", room1);
            IPlayableCharacter player2 = new PlayableCharacter("player2", room1);
            IPlayableCharacter player3 = new PlayableCharacter("player3", room1);
            CreateGroup(player1, player2, player3);

            player2.OnRemoved();

            Assert.IsFalse(player2.IsValid);
            Assert.IsTrue(player1.IsValid);
            Assert.IsTrue(player3.IsValid);
            Assert.IsNotNull(player1.Group);
            Assert.IsNotNull(player3.Group);
            Assert.AreSame(player1, player1.Group.Leader);
            Assert.AreSame(player1.Group, player3.Group);
        }

        [TestMethod]
        public void OnRemoved_MoreThan2Members_LeaderOfGroup()
        {
            IRoom room1 = new Room("room1");
            IPlayableCharacter player1 = new PlayableCharacter("player1", room1);
            IPlayableCharacter player2 = new PlayableCharacter("player2", room1);
            IPlayableCharacter player3 = new PlayableCharacter("player3", room1);
            CreateGroup(player1, player2, player3);

            player1.OnRemoved();

            Assert.IsFalse(player1.IsValid);
            Assert.IsTrue(player2.IsValid);
            Assert.IsTrue(player3.IsValid);
            Assert.IsNotNull(player2.Group);
            Assert.IsNotNull(player3.Group);
            Assert.AreSame(player2, player2.Group.Leader);
            Assert.AreSame(player2.Group, player3.Group);
        }

        [TestMethod]
        public void OnRemoved_2Members_NotLeaderOfGroup()
        {
            IRoom room1 = new Room("room1");
            IPlayableCharacter player1 = new PlayableCharacter("player1", room1);
            IPlayableCharacter player2 = new PlayableCharacter("player2", room1);
            CreateGroup(player1, player2);

            player2.OnRemoved();

            Assert.IsTrue(player1.IsValid);
            Assert.IsFalse(player2.IsValid);
            Assert.IsNull(player1.Group);
        }

        [TestMethod]
        public void OnRemoved_Members_LeaderOfGroup()
        {
            IRoom room1 = new Room("room1");
            IPlayableCharacter player1 = new PlayableCharacter("player1", room1);
            IPlayableCharacter player2 = new PlayableCharacter("player2", room1);
            CreateGroup(player1, player2);

            player1.OnRemoved();

            Assert.IsFalse(player1.IsValid);
            Assert.IsTrue(player2.IsValid);
            Assert.IsNull(player2.Group);
        }
    }
}
