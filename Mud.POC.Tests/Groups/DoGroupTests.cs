using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mud.POC.GroupsPetsFollowers;
using Mud.Server.Input;
using System.Linq;
// ReSharper disable UnusedVariable

namespace Mud.POC.Tests.Groups
{
    [TestClass]
    public class DoGroupTests : GroupTestBase
    {
        // add member
        [TestMethod]
        public void DoGroup_AddMember_InSameRoom()
        {
            IRoom room1 = new Room("room1");
            IPlayableCharacter player1 = new PlayableCharacter("player1", room1);
            IPlayableCharacter player2 = new PlayableCharacter("player2", room1);

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

        // no parameter
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
    }
}
