using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mud.Server.Tests.Mocking;

namespace Mud.Server.Tests
{
    [TestClass]
    public class PlayerCommandTests : TestBase
    {
        private WorldMock _world;
        private PlayerManagerMock playerManager;

        private Tuple<IPlayer,IRoom, IPlayableCharacter> CreatePlayerRoomCharacter(string playerName, string roomName, string characterName)
        {
            IPlayer player = playerManager.AddPlayer(new ClientMock(), playerName);
            IRoom room = _world.AddRoom(Guid.NewGuid(), new Blueprints.Room.RoomBlueprint { Id = 1, Name = "Room1" }, new Area.Area("area1", 1, 100, "builders", "credits"));
            IPlayableCharacter character = _world.AddPlayableCharacter(Guid.NewGuid(), new Domain.CharacterData { Name = characterName, Race = "dwarf", Class="Warrior"}, player, room);
            return new Tuple<IPlayer, IRoom, IPlayableCharacter>(player, room, character);
        }

        [TestMethod]
        public void TestImpersonateUnknownCharacter()
        {
            Tuple<IPlayer,IRoom, IPlayableCharacter> tuple = CreatePlayerRoomCharacter("player", "room", "character");
            
            tuple.Item1.ProcessCommand("impersonate mob1");

            Assert.IsNull(tuple.Item1.Impersonating);
            Assert.IsNotNull(tuple.Item3.ImpersonatedBy);
        }

        [TestInitialize]
        public void Initialize()
        {
            _world = new WorldMock();
            playerManager = new PlayerManagerMock();
        }
    }
}
