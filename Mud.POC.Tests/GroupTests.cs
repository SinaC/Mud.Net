using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Mud.POC.Groups;
using Mud.Server.Input;
using System.Collections.Generic;
using System.Linq;

namespace Mud.POC.Tests
{
    [TestClass]
    public class GroupTests
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

            Assert.IsFalse(memberAdded);
            Assert.IsNull(group.Leader);
            Assert.AreEqual(0, group.Members.Count());
            Assert.IsFalse(group.Members.Contains(player1Mock.Object));
            Assert.IsFalse(group.Members.Contains(player2Mock.Object));
            Assert.IsNull(player1Mock.Object.Group);
            Assert.IsNull(player2Mock.Object.Group);
        }

        // DoGroup add member
        [TestMethod]
        public void DoGroup_AddMember_InSameRoom()
        {
            IRoom room1 = new Room("room1");
            var player1 = new PlayableCharacter("player1", room1);
            var player2 = new PlayableCharacter("player2", room1);

            CommandExecutionResults result = player1.DoGroup("player2", new CommandParameter("player2", false));

            Assert.AreEqual(CommandExecutionResults.Ok, result);
            Assert.IsNotNull(player1.Group);
            Assert.IsNotNull(player2.Group);
            Assert.AreSame(player1.Group, player2.Group);
        }

        [TestMethod]
        public void DoGroup_AddMember_NotInSameRoom()
        {
            IRoom room1 = new Room("room1");
            IPlayableCharacter player1 = new PlayableCharacter("player1", room1);
            IRoom room2 = new Room("room2");
            IPlayableCharacter player2 = new PlayableCharacter("player2", room2);

            CommandExecutionResults result = player1.DoGroup("player2", new CommandParameter("player2", false));

            Assert.AreEqual(CommandExecutionResults.TargetNotFound, result);
            Assert.IsNull(player1.Group);
            Assert.IsNull(player2.Group);
        }

        [TestMethod]
        public void DoGroup_AddMember_AdditionalInSameRoom()
        {
            IRoom room1 = new Room("room1");
            IPlayableCharacter player1 = new PlayableCharacter("player1", room1);
            IPlayableCharacter player2 = new PlayableCharacter("player2", room1);
            IPlayableCharacter player3 = new PlayableCharacter("player3", room1);
            player1.DoGroup("player2", new CommandParameter("player2", false));

            CommandExecutionResults result = player1.DoGroup("player3", new CommandParameter("player3", false));

            Assert.AreEqual(CommandExecutionResults.Ok, result);
            Assert.IsNotNull(player1.Group);
            Assert.IsNotNull(player2.Group);
            Assert.IsNotNull(player3.Group);
            Assert.AreSame(player1.Group, player2.Group);
            Assert.AreSame(player1.Group, player3.Group);
        }

        [TestMethod]
        public void DoGroup_AddMember_AdditionalNoInSameRoom()
        {
            IRoom room1 = new Room("room1");
            IPlayableCharacter player1 = new PlayableCharacter("player1", room1);
            IPlayableCharacter player2 = new PlayableCharacter("player2", room1);
            IRoom room2 = new Room("room2");
            IPlayableCharacter player3 = new PlayableCharacter("player3", room2);
            player1.DoGroup("player2", new CommandParameter("player2", false));

            CommandExecutionResults result = player1.DoGroup("player3", new CommandParameter("player3", false));

            Assert.AreEqual(CommandExecutionResults.TargetNotFound, result);
            Assert.IsNotNull(player1.Group);
            Assert.IsNotNull(player2.Group);
            Assert.IsNull(player3.Group);
            Assert.AreSame(player1.Group, player2.Group);
        }

        [TestMethod]
        public void DoGroup_AddMember_InAnotherGroup()
        {
            IRoom room1 = new Room("room1");
            IPlayableCharacter player1 = new PlayableCharacter("player1", room1);
            IPlayableCharacter player2 = new PlayableCharacter("player2", room1);
            IPlayableCharacter player3 = new PlayableCharacter("player3", room1);
            player2.DoGroup("player3", new CommandParameter("player3", false));

            CommandExecutionResults result = player1.DoGroup("player2", new CommandParameter("player2", false));

            Assert.AreEqual(CommandExecutionResults.InvalidTarget, result);
            Assert.IsNull(player1.Group);
            Assert.IsNotNull(player2.Group);
            Assert.IsNotNull(player3.Group);
            Assert.AreSame(player2.Group, player3.Group);
        }

        [TestMethod]
        public void DoGroup_AddMember_NotTheLeader()
        {
            IRoom room1 = new Room("room1");
            IPlayableCharacter player1 = new PlayableCharacter("player1", room1);
            IPlayableCharacter player2 = new PlayableCharacter("player2", room1);
            IPlayableCharacter player3 = new PlayableCharacter("player3", room1);
            player1.DoGroup("player2", new CommandParameter("player2", false));

            CommandExecutionResults result = player2.DoGroup("player3", new CommandParameter("player3", false));

            Assert.AreEqual(CommandExecutionResults.NoExecution, result);
            Assert.IsNotNull(player1.Group);
            Assert.IsNotNull(player2.Group);
            Assert.IsNull(player3.Group);
            Assert.AreSame(player1.Group, player2.Group);
        }

        // DoGroup: no parameter
        [TestMethod]
        public void DoGroup_NoGroup_NoPet()
        {
            IRoom room1 = new Room("room1");
            IPlayableCharacter player1 = new PlayableCharacter("player1", room1);

            CommandExecutionResults result = player1.DoGroup(string.Empty, Enumerable.Empty<CommandParameter>().ToArray());

            Assert.AreEqual(CommandExecutionResults.NoExecution, result);
        }

        [TestMethod]
        public void DoGroup_NoGroup_OnePet()
        {
            IRoom room1 = new Room("room1");
            IPlayableCharacter player1 = new PlayableCharacter("player1", room1);
            INonPlayableCharacter mob1 = new NonPlayableCharacter("mob1", room1);
            player1.AddPet(mob1);

            CommandExecutionResults result = player1.DoGroup(string.Empty, Enumerable.Empty<CommandParameter>().ToArray());

            Assert.AreEqual(CommandExecutionResults.Ok, result);
        }

        [TestMethod]
        public void DoGroup_InGroup_NoPet()
        {
            IRoom room1 = new Room("room1");
            IPlayableCharacter player1 = new PlayableCharacter("player1", room1);
            IPlayableCharacter player2 = new PlayableCharacter("player2", room1);
            player1.DoGroup("player2", new CommandParameter("player2", false));

            CommandExecutionResults result = player1.DoGroup(string.Empty, Enumerable.Empty<CommandParameter>().ToArray());

            Assert.AreEqual(CommandExecutionResults.Ok, result);
        }

        // TODO: DoUngroup
        // TODO: DoLeave
        // TODO: OnRemoved

        //
        private Mock<IPlayableCharacter> CreatePlayerMock()
        {
            var playerMock = new Mock<IPlayableCharacter>();
            playerMock.Setup(x => x.ChangeGroup(It.IsAny<IGroup>())).Callback<IGroup>(x => playerMock.SetupGet(y => y.Group).Returns(x));
            return playerMock;
        }
    }
}
