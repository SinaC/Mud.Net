using System;
using AutoBogus;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Mud.Server.Tests
{
    [Ignore] // TODO: totally wrong because a playable character should not be created without being impersonated immediately
    [TestClass]
    public class PlayerCommandTests : TestBase
    {
        [TestMethod]
        public void Impersonate_UnknownCharacter()
        {
            IPlayer player = new Player.Player(Guid.NewGuid(), AutoFaker.Generate<string>());
            IPlayableCharacter pc = new Character.PlayableCharacter.PlayableCharacter(Guid.NewGuid(), new Domain.PlayableCharacterData { Name = player.Name, Race = "dwarf", Class = "Warrior" }, player, new Mock<IRoom>().Object);

            player.ProcessCommand("impersonate mob1");

            Assert.IsNull(player.Impersonating);
            Assert.IsNotNull(pc.ImpersonatedBy);
        }

        [TestMethod]
        public void Impersonate_SameCharacter()
        {
            IPlayer player = new Player.Player(Guid.NewGuid(), AutoFaker.Generate<string>());
            IPlayableCharacter pc = new Character.PlayableCharacter.PlayableCharacter(Guid.NewGuid(), new Domain.PlayableCharacterData { Name = player.Name, Race = "dwarf", Class = "Warrior" }, player, new Mock<IRoom>().Object);

            player.ProcessCommand($"impersonate {player.Name}");

            Assert.IsNull(player.Impersonating);
            Assert.IsNotNull(pc.ImpersonatedBy);
        }
    }
}
