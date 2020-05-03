using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Mud.Server.Abilities.Rom24;
using Mud.Server.Blueprints.Character;
using Mud.Server.Blueprints.Room;
using Mud.Server.Character.NonPlayableCharacter;
using Mud.Server.Common;
using System;

namespace Mud.Server.Tests.Rom24
{
    [TestClass]
    public class Rom24CommonTests
    {
        [TestMethod]
        public void SavesSpell_Npc_AlwaysSuccess_Test()
        {
            ICharacter victim = new NonPlayableCharacter(Guid.NewGuid(), new CharacterNormalBlueprint { Id = 1, Name = "Mob1", Level = 1 }, new Room.Room(Guid.NewGuid(), new RoomBlueprint { Id = 1, Name = "Room1" }, new Area.Area("Area1", 0, 100, "Builders", "Credits")));
            var randomManagerMock = new Mock<IRandomManager>();
            randomManagerMock
                .Setup(x => x.Chance(It.IsAny<int>()))
                .Returns(true);
            Rom24Common rom24Common = new Rom24Common(randomManagerMock.Object);

            bool savesSpell = rom24Common.SavesSpell(10, victim, Domain.SchoolTypes.Acid);

            Assert.IsTrue(savesSpell);
        }

        [TestMethod]
        public void SavesSpell_Npc_AlwaysFail_Test()
        {
            ICharacter victim = new NonPlayableCharacter(Guid.NewGuid(), new CharacterNormalBlueprint { Id = 1, Name = "Mob1", Level = 1 }, new Room.Room(Guid.NewGuid(), new RoomBlueprint { Id = 1, Name = "Room1" }, new Area.Area("Area1", 0, 100, "Builders", "Credits")));
            var randomManagerMock = new Mock<IRandomManager>();
            randomManagerMock
                .Setup(x => x.Chance(It.IsAny<int>()))
                .Returns(false);
            Rom24Common rom24Common = new Rom24Common(randomManagerMock.Object);

            bool savesSpell = rom24Common.SavesSpell(10, victim, Domain.SchoolTypes.Acid);

            Assert.IsFalse(savesSpell);
        }

        [TestMethod]
        public void SavesSpell_Npc_ImmuneAcid_DamageAcid_Test()
        {
            ICharacter victim = new NonPlayableCharacter(Guid.NewGuid(), new CharacterNormalBlueprint { Id = 1, Name = "Mob1", Level = 1, Immunities = Domain.IRVFlags.Acid }, new Room.Room(Guid.NewGuid(), new RoomBlueprint { Id = 1, Name = "Room1" }, new Area.Area("Area1", 0, 100, "Builders", "Credits")));
            var randomManagerMock = new Mock<IRandomManager>();
            randomManagerMock
                .Setup(x => x.Chance(It.IsAny<int>()))
                .Returns(false); // always fail, because of immunity random manager should not be used
            Rom24Common rom24Common = new Rom24Common(randomManagerMock.Object);

            bool savesSpell = rom24Common.SavesSpell(10, victim, Domain.SchoolTypes.Acid);

            Assert.IsTrue(savesSpell);
        }

        [TestMethod]
        public void SavesSpell_Npc_ImmunePoison_DamageAcid_Test()
        {
            ICharacter victim = new NonPlayableCharacter(Guid.NewGuid(), new CharacterNormalBlueprint { Id = 1, Name = "Mob1", Level = 1, Immunities = Domain.IRVFlags.Poison }, new Room.Room(Guid.NewGuid(), new RoomBlueprint { Id = 1, Name = "Room1" }, new Area.Area("Area1", 0, 100, "Builders", "Credits")));
            var randomManagerMock = new Mock<IRandomManager>();
            randomManagerMock
                .Setup(x => x.Chance(It.IsAny<int>()))
                .Returns(false); // always fail, because of immunity random manager should not be used
            Rom24Common rom24Common = new Rom24Common(randomManagerMock.Object);

            bool savesSpell = rom24Common.SavesSpell(10, victim, Domain.SchoolTypes.Acid);

            Assert.IsFalse(savesSpell);
        }

        // TODO: more tests
    }
}
