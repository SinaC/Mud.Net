using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Mud.Server.Blueprints.Character;
using Mud.Server.Blueprints.Room;
using Mud.Server.Character.NonPlayableCharacter;
using Mud.Server.Interfaces.Area;
using Mud.Server.Interfaces.Character;
using Mud.Server.Random;
using System;
using Mud.Server.Flags;

namespace Mud.Server.Tests
{
    /*
    // TODO: will fail until RandomManager can be injected in ICharacter
    [Ignore]
    [TestClass]
    public class SavesSpellTests
    {
        [TestMethod]
        public void SavesSpell_Npc_AlwaysSuccess_Test()
        {
            ICharacter victim = new NonPlayableCharacter(Guid.NewGuid(), new CharacterNormalBlueprint { Id = 1, Name = "Mob1", Level = 1 }, new Room.Room(Guid.NewGuid(), new RoomBlueprint { Id = 1, Name = "Room1" }, new Mock<IArea>().Object));
            var randomManagerMock = new Mock<IRandomManager>();
            randomManagerMock
                .Setup(x => x.Chance(It.IsAny<int>()))
                .Returns(true);

            bool savesSpell = victim.SavesSpell(10, Domain.SchoolTypes.Acid);

            Assert.IsTrue(savesSpell);
        }

        [TestMethod]
        public void SavesSpell_Npc_AlwaysFail_Test()
        {
            ICharacter victim = new NonPlayableCharacter(Guid.NewGuid(), new CharacterNormalBlueprint { Id = 1, Name = "Mob1", Level = 1 }, new Room.Room(Guid.NewGuid(), new RoomBlueprint { Id = 1, Name = "Room1" }, new Mock<IArea>().Object));
            var randomManagerMock = new Mock<IRandomManager>();
            randomManagerMock
                .Setup(x => x.Chance(It.IsAny<int>()))
                .Returns(false);

            bool savesSpell = victim.SavesSpell(10, Domain.SchoolTypes.Acid);

            Assert.IsFalse(savesSpell);
        }

        [TestMethod]
        public void SavesSpell_Npc_ImmuneAcid_DamageAcid_Test()
        {
            ICharacter victim = new NonPlayableCharacter(Guid.NewGuid(), new CharacterNormalBlueprint { Id = 1, Name = "Mob1", Level = 1, Immunities = new IRVFlags("Acid") }, new Room.Room(Guid.NewGuid(), new RoomBlueprint { Id = 1, Name = "Room1" }, new Mock<IArea>().Object));
            var randomManagerMock = new Mock<IRandomManager>();
            randomManagerMock
                .Setup(x => x.Chance(It.IsAny<int>()))
                .Returns(false); // always fail, because of immunity random manager should not be used

            bool savesSpell = victim.SavesSpell(10, Domain.SchoolTypes.Acid);

            Assert.IsTrue(savesSpell);
        }

        [TestMethod]
        public void SavesSpell_Npc_ImmunePoison_DamageAcid_Test()
        {
            ICharacter victim = new NonPlayableCharacter(Guid.NewGuid(), new CharacterNormalBlueprint { Id = 1, Name = "Mob1", Level = 1, Immunities = new IRVFlags("Poison") }, new Room.Room(Guid.NewGuid(), new RoomBlueprint { Id = 1, Name = "Room1" }, new Mock<IArea>().Object));
            var randomManagerMock = new Mock<IRandomManager>();
            randomManagerMock
                .Setup(x => x.Chance(It.IsAny<int>()))
                .Returns(false); // always fail, because of immunity random manager should not be used

            bool savesSpell = victim.SavesSpell(10, Domain.SchoolTypes.Acid);

            Assert.IsFalse(savesSpell);
        }

        // TODO: more tests
    }
    */
}
