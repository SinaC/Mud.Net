using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mud.POC.GroupsPetsFollowers;

namespace Mud.POC.Tests.Groups
{
    [TestClass]
    public class DoLeaveTests : GroupTestBase
    {
        [TestMethod]
        public void DoLeave_NoInGroup()
        {
            IRoom room1 = new Room("room1");
            IPlayableCharacter player1 = new PlayableCharacter("player1", room1);

            var args = BuildParameters("");
            CommandExecutionResults result = player1.DoLeave(args.rawParameters, args.parameters);

            Assert.AreEqual(CommandExecutionResults.SyntaxErrorNoDisplay, result);
        }

        [TestMethod]
        public void DoLeave_NotLastMemberNotLeader()
        {
            IRoom room1 = new Room("room1");
            IPlayableCharacter player1 = new PlayableCharacter("player1", room1);
            IPlayableCharacter player2 = new PlayableCharacter("player2", room1);
            IPlayableCharacter player3 = new PlayableCharacter("player3", room1);
            CreateGroup(player1, player2, player3);

            var args = BuildParameters("");
            CommandExecutionResults result = player2.DoLeave(args.rawParameters, args.parameters);

            Assert.AreEqual(CommandExecutionResults.Ok, result);
            Assert.IsNotNull(player1.Group);
            Assert.IsNull(player2.Group);
            Assert.IsNotNull(player3.Group);
            Assert.AreSame(player1, player1.Group?.Leader);
            Assert.AreSame(player1.Group, player3.Group);
        }

        [TestMethod]
        public void DoLeave_NotLastMemberButLeader()
        {
            IRoom room1 = new Room("room1");
            IPlayableCharacter player1 = new PlayableCharacter("player1", room1);
            IPlayableCharacter player2 = new PlayableCharacter("player2", room1);
            IPlayableCharacter player3 = new PlayableCharacter("player3", room1);
            CreateGroup(player1, player2, player3);

            var args = BuildParameters("");
            CommandExecutionResults result = player1.DoLeave(args.rawParameters, args.parameters);

            Assert.AreEqual(CommandExecutionResults.Ok, result);
            Assert.IsNull(player1.Group);
            Assert.IsNotNull(player2.Group);
            Assert.IsNotNull(player3.Group);
            Assert.AreSame(player2, player2.Group?.Leader);
            Assert.AreSame(player2.Group, player3.Group);
        }

        [TestMethod]
        public void DoLeave_LastMember()
        {
            IRoom room1 = new Room("room1");
            IPlayableCharacter player1 = new PlayableCharacter("player1", room1);
            IPlayableCharacter player2 = new PlayableCharacter("player2", room1);
            IGroup group = CreateGroup(player1, player2);

            var args = BuildParameters("");
            CommandExecutionResults result = player2.DoLeave(args.rawParameters, args.parameters);

            Assert.AreEqual(CommandExecutionResults.Ok, result);
            Assert.IsNull(player1.Group);
            Assert.IsNull(player2.Group);
            Assert.IsFalse(group.IsValid);
        }
    }
}
