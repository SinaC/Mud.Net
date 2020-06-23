using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mud.POC.GroupsPetsFollowers;

namespace Mud.POC.Tests.Followers
{
    [TestClass]
    public class DoNofollowTests : FollowerTestBase
    {
        [TestMethod]
        public void NoFollow_NoFollower()
        {
            IRoom room1 = new Room("room1");
            ICharacter player1 = new PlayableCharacter("player1", room1);

            var args = BuildParameters("");
            CommandExecutionResults result = player1.DoNofollow(args.rawParameters, args.parameters);

            Assert.AreEqual(CommandExecutionResults.Ok, result);
        }

        [TestMethod]
        public void NoFollow_Followers()
        {
            IRoom room1 = new Room("room1");
            ICharacter player1 = new PlayableCharacter("player1", room1);
            ICharacter mob1 = new NonPlayableCharacter("mob1", room1);
            ICharacter player2 = new PlayableCharacter("player2", room1);
            player1.AddFollower(player2);
            player1.AddFollower(mob1);

            var args = BuildParameters("");
            CommandExecutionResults result = player1.DoNofollow(args.rawParameters, args.parameters);

            Assert.AreEqual(CommandExecutionResults.Ok, result);
            Assert.IsNull(player1.Leader);
            Assert.IsNull(mob1.Leader);
            Assert.IsNull(player2.Leader);
        }
    }
}
