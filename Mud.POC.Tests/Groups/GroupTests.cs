using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Mud.POC.GroupsPetsFollowers;

namespace Mud.POC.Tests.Groups
{
    [TestClass]
    public class GroupTests : GroupTestBase
    {
        // Group tests
        [TestMethod]
        public void Group_CreateGroup() 
        {
            var playerMock = CreatePlayerMock();

            IGroup group = new Group(playerMock.Object);

            Assert.AreSame(playerMock.Object.Group, group);
            Assert.IsNotNull(group.Leader);
            Assert.AreSame(playerMock.Object, group.Leader);
            Assert.AreEqual(1, group.Members.Count());
            Assert.AreSame(playerMock.Object, group.Members.Single());
            playerMock.Verify(x => x.ChangeGroup(It.IsAny<IGroup>()), Times.Once);
        }

        [TestMethod]
        public void Group_AddMember()
        {
            var player1Mock = CreatePlayerMock();
            var player2Mock = CreatePlayerMock();
            IGroup group = new Group(player1Mock.Object);

            bool memberAdded = group.AddMember(player2Mock.Object);

            Assert.AreSame(player1Mock.Object.Group, group);
            Assert.AreSame(player2Mock.Object.Group, group);
            Assert.IsTrue(memberAdded);
            Assert.IsNotNull(group.Leader);
            Assert.AreSame(player1Mock.Object, group.Leader);
            Assert.AreEqual(2, group.Members.Count());
            player1Mock.Verify(x => x.ChangeGroup(It.IsAny<IGroup>()), Times.Once);
            player2Mock.Verify(x => x.ChangeGroup(It.IsAny<IGroup>()), Times.Once);
        }

        [TestMethod]
        public void Group_AddMember_DuplicateMember()
        {
            var player1Mock = CreatePlayerMock();
            IGroup group = new Group(player1Mock.Object);

            bool memberAdded = group.AddMember(player1Mock.Object);

            Assert.AreSame(player1Mock.Object.Group, group);
            Assert.IsFalse(memberAdded);
            Assert.IsNotNull(group.Leader);
            Assert.AreSame(player1Mock.Object, group.Leader);
            Assert.AreEqual(1, group.Members.Count());
            Assert.AreSame(player1Mock.Object, group.Members.Single());
            player1Mock.Verify(x => x.ChangeGroup(It.IsAny<IGroup>()), Times.Once);
        }

        [TestMethod]
        public void Group_AddMember_TooManyMember()
        {
            var player1Mock = CreatePlayerMock();
            var player2Mock = CreatePlayerMock();
            var player3Mock = CreatePlayerMock();
            var player4Mock = CreatePlayerMock();
            var player5Mock = CreatePlayerMock();
            var player6Mock = CreatePlayerMock();
            IGroup group = new Group(player1Mock.Object);
            group.AddMember(player2Mock.Object);
            group.AddMember(player3Mock.Object);
            group.AddMember(player4Mock.Object);
            group.AddMember(player5Mock.Object);

            bool memberAdded = group.AddMember(player6Mock.Object);

            Assert.IsFalse(memberAdded);
            Assert.AreSame(player1Mock.Object.Group, group);
            Assert.AreSame(player2Mock.Object.Group, group);
            Assert.AreSame(player3Mock.Object.Group, group);
            Assert.AreSame(player4Mock.Object.Group, group);
            Assert.AreSame(player5Mock.Object.Group, group);
            Assert.AreSame(player5Mock.Object.Group, group);
            Assert.IsNull(player6Mock.Object.Group);
            Assert.AreSame(player1Mock.Object, group.Leader);
            Assert.AreEqual(5, group.Members.Count());
            player1Mock.Verify(x => x.ChangeGroup(It.IsAny<IGroup>()), Times.Once);
            player2Mock.Verify(x => x.ChangeGroup(It.IsAny<IGroup>()), Times.Once);
            player3Mock.Verify(x => x.ChangeGroup(It.IsAny<IGroup>()), Times.Once);
            player4Mock.Verify(x => x.ChangeGroup(It.IsAny<IGroup>()), Times.Once);
            player5Mock.Verify(x => x.ChangeGroup(It.IsAny<IGroup>()), Times.Once);
        }

        [TestMethod]
        public void Group_AddMember_InAnotherGroup()
        {
            var player1Mock = CreatePlayerMock();
            var player2Mock = CreatePlayerMock();
            IGroup group1 = new Group(player1Mock.Object);
            group1.AddMember(player2Mock.Object);
            var player3Mock = CreatePlayerMock();
            IGroup group2 = new Group(player3Mock.Object);

            bool memberAdded = group1.AddMember(player3Mock.Object);

            Assert.IsFalse(memberAdded);
            Assert.AreSame(player1Mock.Object, group1.Leader);
            Assert.AreEqual(2, group1.Members.Count());
            Assert.AreSame(player3Mock.Object, group2.Leader);
            Assert.AreEqual(1, group2.Members.Count());
            Assert.AreSame(group1, player1Mock.Object.Group);
            Assert.AreSame(group1, player2Mock.Object.Group);
            Assert.AreSame(group2, player3Mock.Object.Group);
            player1Mock.Verify(x => x.ChangeGroup(It.IsAny<IGroup>()), Times.Once);
            player2Mock.Verify(x => x.ChangeGroup(It.IsAny<IGroup>()), Times.Once);
            player3Mock.Verify(x => x.ChangeGroup(It.IsAny<IGroup>()), Times.Once);
        }

        [TestMethod]
        public void Group_ChangeLeader()
        {
            var player1Mock = CreatePlayerMock();
            var player2Mock = CreatePlayerMock();
            var player3Mock = CreatePlayerMock();
            IGroup group = new Group(player1Mock.Object);
            group.AddMember(player2Mock.Object);
            group.AddMember(player3Mock.Object);

            bool leaderChanged = group.ChangeLeader(player3Mock.Object);

            Assert.IsTrue(leaderChanged);
            Assert.IsNotNull(group.Leader);
            Assert.AreSame(player3Mock.Object, group.Leader);
            Assert.AreEqual(3, group.Members.Count());
            Assert.AreSame(group, player1Mock.Object.Group);
            Assert.AreSame(group, player2Mock.Object.Group);
            Assert.AreSame(group, player3Mock.Object.Group);
            player1Mock.Verify(x => x.ChangeGroup(It.IsAny<IGroup>()), Times.Once);
            player2Mock.Verify(x => x.ChangeGroup(It.IsAny<IGroup>()), Times.Once);
            player3Mock.Verify(x => x.ChangeGroup(It.IsAny<IGroup>()), Times.Once);
        }

        [TestMethod]
        public void Group_ChangeLeader_AlreadyLeader()
        {
            var player1Mock = CreatePlayerMock();
            var player2Mock = CreatePlayerMock();
            var player3Mock = CreatePlayerMock();
            IGroup group = new Group(player1Mock.Object);
            group.AddMember(player2Mock.Object);
            group.AddMember(player3Mock.Object);

            bool leaderChanged = group.ChangeLeader(player1Mock.Object);

            Assert.IsFalse(leaderChanged);
            Assert.IsNotNull(group.Leader);
            Assert.AreSame(player1Mock.Object, group.Leader);
            Assert.AreEqual(3, group.Members.Count());
            Assert.AreSame(group, player1Mock.Object.Group);
            Assert.AreSame(group, player2Mock.Object.Group);
            Assert.AreSame(group, player3Mock.Object.Group);
            player1Mock.Verify(x => x.ChangeGroup(It.IsAny<IGroup>()), Times.Once);
            player2Mock.Verify(x => x.ChangeGroup(It.IsAny<IGroup>()), Times.Once);
            player3Mock.Verify(x => x.ChangeGroup(It.IsAny<IGroup>()), Times.Once);
        }

        [TestMethod]
        public void Group_ChangeLeader_NotInGroup()
        {
            var player1Mock = CreatePlayerMock();
            var player2Mock = CreatePlayerMock();
            var player3Mock = CreatePlayerMock();
            IGroup group = new Group(player1Mock.Object);
            group.AddMember(player2Mock.Object);

            bool leaderChanged = group.ChangeLeader(player3Mock.Object);

            Assert.IsFalse(leaderChanged);
            Assert.IsNotNull(group.Leader);
            Assert.AreSame(player1Mock.Object, group.Leader);
            Assert.AreEqual(2, group.Members.Count());
            Assert.AreSame(group, player1Mock.Object.Group);
            Assert.AreSame(group, player2Mock.Object.Group);
            Assert.IsNull(player3Mock.Object.Group);
            player1Mock.Verify(x => x.ChangeGroup(It.IsAny<IGroup>()), Times.Once);
            player2Mock.Verify(x => x.ChangeGroup(It.IsAny<IGroup>()), Times.Once);
            player3Mock.Verify(x => x.ChangeGroup(It.IsAny<IGroup>()), Times.Never);
        }

        [TestMethod]
        public void Group_RemoveMember_NotTheLeader()
        {
            var player1Mock = CreatePlayerMock();
            var player2Mock = CreatePlayerMock();
            var player3Mock = CreatePlayerMock();
            IGroup group = new Group(player1Mock.Object);
            group.AddMember(player2Mock.Object);
            group.AddMember(player3Mock.Object);

            bool memberRemoved = group.RemoveMember(player2Mock.Object);

            Assert.IsTrue(memberRemoved);
            Assert.IsNotNull(group.Leader);
            Assert.AreSame(player1Mock.Object, group.Leader);
            Assert.AreEqual(2, group.Members.Count());
            Assert.IsFalse(group.Members.Contains(player2Mock.Object));
            Assert.IsTrue(group.Members.Contains(player1Mock.Object));
            Assert.IsTrue(group.Members.Contains(player3Mock.Object));
            Assert.IsNull(player2Mock.Object.Group);
            Assert.AreSame(group, player1Mock.Object.Group);
            Assert.AreSame(group, player3Mock.Object.Group);
            player1Mock.Verify(x => x.ChangeGroup(It.IsAny<IGroup>()), Times.Once);
            player2Mock.Verify(x => x.ChangeGroup(It.IsAny<IGroup>()), Times.Exactly(2));
            player3Mock.Verify(x => x.ChangeGroup(It.IsAny<IGroup>()), Times.Once);
        }

        [TestMethod]
        public void Group_RemoveMember_TheLeader()
        {
            var player1Mock = CreatePlayerMock();
            var player2Mock = CreatePlayerMock();
            var player3Mock = CreatePlayerMock();
            IGroup group = new Group(player1Mock.Object);
            group.AddMember(player2Mock.Object);
            group.AddMember(player3Mock.Object);

            bool memberRemoved = group.RemoveMember(player1Mock.Object);

            Assert.IsTrue(memberRemoved);
            Assert.IsNotNull(group.Leader);
            Assert.AreSame(player2Mock.Object, group.Leader);
            Assert.AreEqual(2, group.Members.Count());
            Assert.IsFalse(group.Members.Contains(player1Mock.Object));
            Assert.IsTrue(group.Members.Contains(player2Mock.Object));
            Assert.IsTrue(group.Members.Contains(player3Mock.Object));
            Assert.IsNull(player1Mock.Object.Group);
            Assert.AreSame(group, player2Mock.Object.Group);
            Assert.AreSame(group, player3Mock.Object.Group);
            player1Mock.Verify(x => x.ChangeGroup(It.IsAny<IGroup>()), Times.Exactly(2));
            player2Mock.Verify(x => x.ChangeGroup(It.IsAny<IGroup>()), Times.Once);
            player3Mock.Verify(x => x.ChangeGroup(It.IsAny<IGroup>()), Times.Once);
        }

        [TestMethod]
        public void Group_RemoveMember_NotInGroup()
        {
            var player1Mock = CreatePlayerMock();
            var player2Mock = CreatePlayerMock();
            var player3Mock = CreatePlayerMock();
            IGroup group = new Group(player1Mock.Object);
            group.AddMember(player2Mock.Object);

            bool memberRemoved = group.RemoveMember(player3Mock.Object);

            Assert.IsFalse(memberRemoved);
            Assert.IsNotNull(group.Leader);
            Assert.AreSame(player1Mock.Object, group.Leader);
            Assert.AreEqual(2, group.Members.Count());
            Assert.IsTrue(group.Members.Contains(player1Mock.Object));
            Assert.IsTrue(group.Members.Contains(player2Mock.Object));
            Assert.IsFalse(group.Members.Contains(player3Mock.Object));
            Assert.IsNull(player3Mock.Object.Group);
            Assert.AreSame(group, player1Mock.Object.Group);
            Assert.AreSame(group, player2Mock.Object.Group);
            player1Mock.Verify(x => x.ChangeGroup(It.IsAny<IGroup>()), Times.Once);
            player2Mock.Verify(x => x.ChangeGroup(It.IsAny<IGroup>()), Times.Once);
            player3Mock.Verify(x => x.ChangeGroup(It.IsAny<IGroup>()), Times.Never);
        }

        [TestMethod]
        public void Group_RemoveMember_InAnotherGroup()
        {
            var player1Mock = CreatePlayerMock();
            var player2Mock = CreatePlayerMock();
            var player3Mock = CreatePlayerMock();
            IGroup group1 = new Group(player1Mock.Object);
            group1.AddMember(player2Mock.Object);
            IGroup group2 = new Group(player3Mock.Object);

            bool memberRemoved = group1.RemoveMember(player3Mock.Object);

            Assert.IsFalse(memberRemoved);
            Assert.IsNotNull(group1.Leader);
            Assert.AreSame(player1Mock.Object, group1.Leader);
            Assert.AreEqual(2, group1.Members.Count());
            Assert.IsTrue(group1.Members.Contains(player1Mock.Object));
            Assert.IsTrue(group1.Members.Contains(player2Mock.Object));
            Assert.IsFalse(group1.Members.Contains(player3Mock.Object));
            Assert.IsTrue(group2.Members.Contains(player3Mock.Object));
            Assert.IsNotNull(player3Mock.Object.Group);
            Assert.AreSame(group1, player1Mock.Object.Group);
            Assert.AreSame(group1, player2Mock.Object.Group);
            Assert.AreSame(group2, player3Mock.Object.Group);
            player1Mock.Verify(x => x.ChangeGroup(It.IsAny<IGroup>()), Times.Once);
            player2Mock.Verify(x => x.ChangeGroup(It.IsAny<IGroup>()), Times.Once);
            player3Mock.Verify(x => x.ChangeGroup(It.IsAny<IGroup>()), Times.Once);
        }

        [TestMethod]
        public void Group_RemoveMember_RemoveLastMember()
        {
            var player1Mock = CreatePlayerMock();
            var player2Mock = CreatePlayerMock();
            IGroup group = new Group(player1Mock.Object);
            group.AddMember(player2Mock.Object);
            group.RemoveMember(player2Mock.Object);

            bool memberRemoved = group.RemoveMember(player1Mock.Object);

            Assert.IsTrue(memberRemoved);
            Assert.IsNull(group.Leader);
            Assert.AreEqual(0, group.Members.Count());
            Assert.IsFalse(group.Members.Contains(player1Mock.Object));
            Assert.IsFalse(group.Members.Contains(player2Mock.Object));
            Assert.IsNull(player1Mock.Object.Group);
            Assert.IsNull(player2Mock.Object.Group);
            player1Mock.Verify(x => x.ChangeGroup(It.IsAny<IGroup>()), Times.Exactly(2));
            player2Mock.Verify(x => x.ChangeGroup(It.IsAny<IGroup>()), Times.Exactly(2));
        }

        [TestMethod]
        public void Group_Disband()
        {
            var player1Mock = CreatePlayerMock();
            var player2Mock = CreatePlayerMock();
            var player3Mock = CreatePlayerMock();
            var player4Mock = CreatePlayerMock();
            IGroup group = new Group(player1Mock.Object);
            group.AddMember(player2Mock.Object);
            group.AddMember(player3Mock.Object);
            group.AddMember(player4Mock.Object);

            group.Disband();

            Assert.IsFalse(group.IsValid);
            Assert.IsNull(player1Mock.Object.Group);
            Assert.IsNull(player2Mock.Object.Group);
            Assert.IsNull(player3Mock.Object.Group);
            Assert.IsNull(player4Mock.Object.Group);
            player1Mock.Verify(x => x.ChangeGroup(It.IsAny<IGroup>()), Times.Exactly(2));
            player2Mock.Verify(x => x.ChangeGroup(It.IsAny<IGroup>()), Times.Exactly(2));
            player3Mock.Verify(x => x.ChangeGroup(It.IsAny<IGroup>()), Times.Exactly(2));
            player4Mock.Verify(x => x.ChangeGroup(It.IsAny<IGroup>()), Times.Exactly(2));
        }

        [TestMethod]
        public void Group_AddMember_AfterRemoveLastMember()
        {
            var player1Mock = CreatePlayerMock();
            var player2Mock = CreatePlayerMock();
            IGroup group = new Group(player1Mock.Object);
            group.AddMember(player2Mock.Object);
            group.RemoveMember(player2Mock.Object);
            group.RemoveMember(player1Mock.Object);

            bool memberAdded = group.AddMember(player2Mock.Object);

            Assert.IsFalse(group.IsValid);
            Assert.IsFalse(memberAdded);
            Assert.IsNull(group.Leader);
            Assert.AreEqual(0, group.Members.Count());
            Assert.IsFalse(group.Members.Contains(player1Mock.Object));
            Assert.IsFalse(group.Members.Contains(player2Mock.Object));
            Assert.IsNull(player1Mock.Object.Group);
            Assert.IsNull(player2Mock.Object.Group);
            player1Mock.Verify(x => x.ChangeGroup(It.IsAny<IGroup>()), Times.Exactly(2));
            player2Mock.Verify(x => x.ChangeGroup(It.IsAny<IGroup>()), Times.Exactly(2));
        }


        //
        private Mock<IPlayableCharacter> CreatePlayerMock()
        {
            var playerMock = new Mock<IPlayableCharacter>();
            playerMock.Setup(x => x.ChangeGroup(It.IsAny<IGroup>())).Callback<IGroup>(x => playerMock.SetupGet(y => y.Group).Returns(x));
            return playerMock;
        }
    }
}
