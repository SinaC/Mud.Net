using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mud.Server.Tests.Mocking;
using Mud.Server.Interfaces.Player;

namespace Mud.Server.Tests
{
    [TestClass]
    public class CommandParsingTests : TestBase
    {
        private WorldMock _world;
        private PlayerManagerMock _playerManagerMock;

        private IPlayer CreatePlayer(string playerName)
        {
            IPlayer player = _playerManagerMock.AddPlayer(new ClientMock(), playerName);
            return player;
        }

        /*
         * player.ProcessCommand("test");
            player.ProcessCommand("test arg1");
            player.ProcessCommand("test 'arg1' 'arg2' 'arg3' 'arg4'");
            player.ProcessCommand("test 'arg1 arg2' 'arg3 arg4'");
            player.ProcessCommand("test 'arg1 arg2\" arg3 arg4");
            player.ProcessCommand("test 3.arg1");
            player.ProcessCommand("test 2.'arg1'");
            player.ProcessCommand("test 2'.arg1'");
            player.ProcessCommand("test 2.'arg1 arg2' 3.arg3 5.arg4");
            player.ProcessCommand("test 2."); // INVALID
            player.ProcessCommand("test ."); // INVALID
            player.ProcessCommand("test '2.arg1'");
            player.ProcessCommand("unknown"); // INVALID
            player.ProcessCommand("/test");

            ICharacter character = _world.AddCharacter(Guid.NewGuid(), "Character", room);
            character.ProcessCommand("look");
            character.ProcessCommand("tell"); // INVALID because Player commands are not accessible by Character
            character.ProcessCommand("unknown"); // INVALID

            player.ProcessCommand("impersonate"); // INVALID to un-impersonate, player already must be impersonated
            player.ProcessCommand("impersonate character");
            player.ProcessCommand("/tell");
            player.ProcessCommand("tell"); // INVALID because OOG is not accessible while impersonating unless if command starts with /
            player.ProcessCommand("look");

            player.ProcessCommand("impersonate"); // INVALID because OOG is not accessible while impersonating unless if command starts with /
            player.ProcessCommand("/impersonate");
            player.ProcessCommand("/tell");
            player.ProcessCommand("tell");
            player.ProcessCommand("look"); // INVALID because Character commands are not accessible by Player unless if impersonating

            IAdmin admin = _world.AddAdmin(Guid.NewGuid(), "Admin");
            admin.ProcessCommand("incarnate");
            admin.ProcessCommand("unknown"); // INVALID
         * */

        [TestMethod]
        public void TestExistingCommandNoArgument()
        {
            IPlayer player = CreatePlayer("player");

            bool processed = player.ProcessInput("test");

            Assert.IsTrue(processed);
        }

        [TestMethod]
        public void TestExistingCommandSingleArgument()
        {
            IPlayer player = CreatePlayer("player");

            bool processed = player.ProcessInput("test arg1");

            Assert.IsTrue(processed);
        }

        [TestMethod]
        public void TestExistingCommandMultipleArgument()
        {
            IPlayer player = CreatePlayer("player");

            bool processed = player.ProcessInput("test 'arg1' 'arg2' 'arg3' 'arg4'");

            Assert.IsTrue(processed);
        }

        [TestInitialize]
        public void Initialize()
        {
            _playerManagerMock = new PlayerManagerMock();
            _world = new WorldMock();
        }
    }
}
