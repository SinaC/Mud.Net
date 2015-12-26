using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mud.Logger;
using Mud.Server.Tests.Mocking;

namespace Mud.Server.Tests
{
    [TestClass]
    public class PlayerCommandTests
    {
        private LogMock _log;
        private WorldMock _world;

        private Tuple<IPlayer,IRoom,ICharacter> CreatePlayerRoomCharacter(string playerName, string roomName, string characterName)
        {
            IPlayer player = _world.AddPlayer(Guid.NewGuid(), playerName);
            IRoom room = _world.AddRoom(Guid.NewGuid(), roomName);
            ICharacter character = _world.AddCharacter(Guid.NewGuid(), characterName, room);
            return new Tuple<IPlayer, IRoom, ICharacter>(player, room, character);
        }

        [TestMethod]
        public void TestImpersonateUnknownCharacter()
        {
            Tuple<IPlayer,IRoom,ICharacter> tuple = CreatePlayerRoomCharacter("player", "room", "character");
            
            tuple.Item1.ProcessCommand("impersonate mob1");

            Assert.IsNull(tuple.Item1.Impersonating);
            Assert.IsNull(tuple.Item3.ImpersonatedBy);
        }

        [TestInitialize]
        public void Initialize()
        {
            _world = new WorldMock();
            _log = new LogMock();
            Log.SetLogger(_log);
        }
    }
}
