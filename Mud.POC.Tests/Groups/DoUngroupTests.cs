using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mud.POC.GroupsPetsFollowers;
using Mud.Server.Input;
// ReSharper disable UnusedVariable

namespace Mud.POC.Tests.Groups
{
    [TestClass]
    public class DoUngroupTests : GroupTestBase
    {
        [TestMethod]
        public void DoUngroup_NoParam()
        {
            IRoom room1 = new Room("room1");
            IPlayableCharacter player1 = new PlayableCharacter("player1", room1);
            IPlayableCharacter player2 = new PlayableCharacter("player2", room1);
            CreateGroup(player1, player2);

            var args = BuildParameters("");
            CommandExecutionResults result = player1.DoUngroup(args.rawParameters, args.parameters);

            Assert.AreEqual(CommandExecutionResults.SyntaxErrorNoDisplay, result);
        }

        [TestMethod]
        public void DoUngroup_NotInAGroup()
        {
            IRoom room1 = new Room("room1");
            IPlayableCharacter player1 = new PlayableCharacter("player1", room1);

            var args = BuildParameters("player1");
            CommandExecutionResults result = player1.DoUngroup(args.rawParameters, args.parameters);

            Assert.AreEqual(CommandExecutionResults.NoExecution, result);
        }

        [TestMethod]
        public void DoUngroup_Self()
        {
            IRoom room1 = new Room("room1");
            IPlayableCharacter player1 = new PlayableCharacter("player1", room1);
            IPlayableCharacter player2 = new PlayableCharacter("player2", room1);
            CreateGroup(player1, player2);

            var args = BuildParameters("player1");
            CommandExecutionResults result = player1.DoUngroup(args.rawParameters, args.parameters);

            Assert.AreEqual(CommandExecutionResults.InvalidTarget, result);
        }

        [TestMethod]
        public void DoUngroup_TargetNotInGroup()
        {
            IRoom room1 = new Room("room1");
            IPlayableCharacter player1 = new PlayableCharacter("player1", room1);
            IPlayableCharacter player2 = new PlayableCharacter("player2", room1);
            CreateGroup(player1, player2);
            IPlayableCharacter player3 = new PlayableCharacter("player3", room1);

            var args = BuildParameters("player3");
            CommandExecutionResults result = player1.DoUngroup(args.rawParameters, args.parameters);

            Assert.AreEqual(CommandExecutionResults.TargetNotFound, result);
        }

        [TestMethod]
        public void DoUngroup_NotTheLeader()
        {
            IRoom room1 = new Room("room1");
            IPlayableCharacter player1 = new PlayableCharacter("player1", room1);
            IPlayableCharacter player2 = new PlayableCharacter("player2", room1);
            IPlayableCharacter player3 = new PlayableCharacter("player3", room1);
            CreateGroup(player1, player2, player3);

            var args = BuildParameters("player3");
            CommandExecutionResults result = player2.DoUngroup(args.rawParameters, args.parameters);

            Assert.AreEqual(CommandExecutionResults.NoExecution, result);
            Assert.IsNotNull(player3.Group);
        }


        [TestMethod]
        public void DoUngroup_NotLastMember()
        {
            IRoom room1 = new Room("room1");
            IPlayableCharacter player1 = new PlayableCharacter("player1", room1);
            IPlayableCharacter player2 = new PlayableCharacter("player2", room1);
            IPlayableCharacter player3 = new PlayableCharacter("player3", room1);
            CreateGroup(player1, player2, player3);

            var args = BuildParameters("player2");
            CommandExecutionResults result = player1.DoUngroup(args.rawParameters, args.parameters);

            Assert.AreEqual(CommandExecutionResults.Ok, result);
            Assert.IsNotNull(player1.Group);
            Assert.IsNull(player2.Group);
            Assert.IsNotNull(player3.Group);
            Assert.AreSame(player1.Group, player3.Group);
            Assert.AreSame(player1, player1.Group?.Leader);
        }

        [TestMethod]
        public void DoUngroup_LastMember()
        {
            IRoom room1 = new Room("room1");
            IPlayableCharacter player1 = new PlayableCharacter("player1", room1);
            IPlayableCharacter player2 = new PlayableCharacter("player2", room1);
            CreateGroup(player1, player2);

            var args = BuildParameters("player2");
            CommandExecutionResults result = player1.DoUngroup(args.rawParameters, args.parameters);

            Assert.AreEqual(CommandExecutionResults.Ok, result);
            Assert.IsNull(player1.Group);
            Assert.IsNull(player2.Group);
        }
    }
}
