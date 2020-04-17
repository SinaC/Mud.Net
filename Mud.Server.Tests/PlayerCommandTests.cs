﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mud.Container;
using Mud.Logger;
using Mud.Server.Tests.Mocking;

namespace Mud.Server.Tests
{
    [TestClass]
    public class PlayerCommandTests
    {
        private WorldMock _world;
        private PlayerManagerMock playerManager;

        [AssemblyInitialize]
        public static void AssemblyInitialize(TestContext context)
        {
            DependencyContainer.Current.Register<IRaceManager, Races.RaceManager>(SimpleInjector.Lifestyle.Singleton);
            DependencyContainer.Current.Register<IClassManager, Classes.ClassManager>(SimpleInjector.Lifestyle.Singleton);
            DependencyContainer.Current.Register<IAbilityManager, Abilities.AbilityManager>(SimpleInjector.Lifestyle.Singleton);
        }

        private Tuple<IPlayer,IRoom,ICharacter> CreatePlayerRoomCharacter(string playerName, string roomName, string characterName)
        {
            IPlayer player = playerManager.AddPlayer(new ClientMock(), playerName);
            IRoom room = _world.AddRoom(Guid.NewGuid(), new Blueprints.Room.RoomBlueprint { Id = 1, Name = "Room1" }, new Area.Area("area1", 1, 100, "builders", "credits"));
            ICharacter character = _world.AddCharacter(Guid.NewGuid(), new Domain.CharacterData { Name = characterName, Race = "dwarf", Class="Warrior"}, room);
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
            playerManager = new PlayerManagerMock();
        }
    }
}
