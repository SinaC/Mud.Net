using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Mud.POC.GroupsPetsFollowers;
using Mud.Server.Input;

namespace Mud.POC.Tests.Followers
{
    [TestClass]
    public class DoFollowTests : FollowerTestBase
    {
        [TestMethod]
        public void Follow_NoParameter_NotFollowing()
        {
            ICharacter player1 = new PlayableCharacter("player1", new Mock<IRoom>().Object);

            var args = BuildParameters("");
            CommandExecutionResults result = player1.DoFollow(args.rawParameters, args.parameters);

            Assert.AreEqual(CommandExecutionResults.NoExecution, result);
        }

        [TestMethod]
        public void Follow_NoParameter_Following()
        {
            ICharacter player1 = new PlayableCharacter("player1", new Mock<IRoom>().Object);
            ICharacter mob1 = new NonPlayableCharacter("mob1", new Mock<IRoom>().Object);
            mob1.AddFollower(player1);

            var args = BuildParameters("");
            CommandExecutionResults result = player1.DoFollow(args.rawParameters, args.parameters);

            Assert.AreEqual(CommandExecutionResults.Ok, result);
        }

        [TestMethod]
        public void Follow_NotExisting()
        {
            IRoom room1 = new Room("room1");
            ICharacter player1 = new PlayableCharacter("player1", room1);

            var args = BuildParameters("mob1");
            CommandExecutionResults result = player1.DoFollow(args.rawParameters, args.parameters);

            Assert.AreEqual(CommandExecutionResults.TargetNotFound, result);
        }

        [TestMethod]
        public void Follow_NotInSameRoom()
        {
            IRoom room1 = new Room("room1");
            ICharacter player1 = new PlayableCharacter("player1", room1);
            IRoom room2 = new Room("room2");
            ICharacter mob1 = new NonPlayableCharacter("mob1", room2);

            var args = BuildParameters("mob1");
            CommandExecutionResults result = player1.DoFollow(args.rawParameters, args.parameters);

            Assert.AreEqual(CommandExecutionResults.TargetNotFound, result);
        }

        [TestMethod]
        public void Follow_TargetInSameRoom()
        {
            IRoom room1 = new Room("room1");
            ICharacter player1 = new PlayableCharacter("player1", room1);
            ICharacter mob1 = new NonPlayableCharacter("mob1", room1);

            var args = BuildParameters("mob1");
            CommandExecutionResults result = player1.DoFollow(args.rawParameters, args.parameters);

            Assert.AreEqual(CommandExecutionResults.Ok, result);
            Assert.AreSame(mob1, player1.Leader);
        }

        [TestMethod]
        public void Follow_Self_NotFollowing()
        {
            IRoom room1 = new Room("room1");
            ICharacter player1 = new PlayableCharacter("player1", room1);

            var args = BuildParameters("player1");
            CommandExecutionResults result = player1.DoFollow(args.rawParameters, args.parameters);

            Assert.AreEqual(CommandExecutionResults.InvalidTarget, result);
        }

        [TestMethod]
        public void Follow_Self_Following()
        {
            IRoom room1 = new Room("room1");
            ICharacter player1 = new PlayableCharacter("player1", room1);
            ICharacter mob1 = new NonPlayableCharacter("mob1", room1);
            mob1.AddFollower(player1);

            var args = BuildParameters("player1");
            CommandExecutionResults result = player1.DoFollow(args.rawParameters, args.parameters);

            Assert.AreEqual(CommandExecutionResults.Ok, result);
        }
    }
}
