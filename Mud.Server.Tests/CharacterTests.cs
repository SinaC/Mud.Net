using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Mud.Container;
using Mud.Domain;
using Mud.Server.Character.NonPlayableCharacter;

namespace Mud.Server.Tests
{
    [TestClass]
    public class CharacterTests
    {
        private SimpleInjector.Container _originalContainer;

        [TestInitialize]
        public void TestInitialize()
        {
            _originalContainer = DependencyContainer.Current;
            DependencyContainer.SetManualContainer(new SimpleInjector.Container());
        }

        [TestCleanup]
        public void TestCleanup()
        {
            DependencyContainer.SetManualContainer(_originalContainer);
        }

        [TestMethod]
        public void BasicAttributes_Test()
        {
            INonPlayableCharacter npc = new NonPlayableCharacter(Guid.NewGuid(), new Blueprints.Character.CharacterNormalBlueprint { Id = 1, Name = "mob1", ActFlags = ActFlags.Noalign | ActFlags.Gain, OffensiveFlags = OffensiveFlags.AreaAttack | OffensiveFlags.Bash, CharacterFlags = CharacterFlags.Sanctuary | CharacterFlags.Regeneration, Level = 50, Sex = Sex.Neutral }, new Room.Room(Guid.NewGuid(), new Blueprints.Room.RoomBlueprint { Id = 1, Name = "room1" }, new Area.Area("Area1", 1, 100, "builders", "credits")));
            npc.Recompute();

            Assert.AreEqual(npc[CharacterAttributes.Strength], npc[BasicAttributes.Strength]);
            Assert.AreEqual(npc[CharacterAttributes.Intelligence], npc[BasicAttributes.Intelligence]);
            Assert.AreEqual(npc[CharacterAttributes.Wisdom], npc[BasicAttributes.Wisdom]);
            Assert.AreEqual(npc[CharacterAttributes.Dexterity], npc[BasicAttributes.Dexterity]);
            Assert.AreEqual(npc[CharacterAttributes.Constitution], npc[BasicAttributes.Constitution]);
        }

        [TestMethod]
        public void Armor_Test()
        {
            int defensiveBonus = 50;
            var attributeTablesMock = new Mock<ITableValues>();
            attributeTablesMock.Setup(x => x.DefensiveBonus(It.IsAny<ICharacter>())).Returns<ICharacter>(x => defensiveBonus);
            DependencyContainer.Current.RegisterInstance<ITableValues>(attributeTablesMock.Object);

            INonPlayableCharacter npc = new NonPlayableCharacter(Guid.NewGuid(), new Blueprints.Character.CharacterNormalBlueprint { Id = 1, Name = "mob1", ActFlags = ActFlags.Noalign | ActFlags.Gain, OffensiveFlags = OffensiveFlags.AreaAttack | OffensiveFlags.Bash, CharacterFlags = CharacterFlags.Sanctuary | CharacterFlags.Regeneration, Level = 50, Sex = Sex.Neutral }, new Room.Room(Guid.NewGuid(), new Blueprints.Room.RoomBlueprint { Id = 1, Name = "room1" }, new Area.Area("Area1", 1, 100, "builders", "credits")));
            npc.Recompute();

            Assert.AreEqual(-npc.Level+ defensiveBonus, npc[Armors.Bash]); // Armor is initialized with -Level
            Assert.AreEqual(-npc.Level + defensiveBonus, npc[Armors.Pierce]);
            Assert.AreEqual(-npc.Level + defensiveBonus, npc[Armors.Slash]);
            Assert.AreEqual(-npc.Level + defensiveBonus, npc[Armors.Magic]);
        }

        [TestMethod]
        public void HitDamRoll_Test()
        {
            int hitBonus = 45;
            int damBonus = 30;
            var attributeTablesMock = new Mock<ITableValues>();
            attributeTablesMock.Setup(x => x.HitBonus(It.IsAny<ICharacter>())).Returns<ICharacter>(x => hitBonus);
            attributeTablesMock.Setup(x => x.DamBonus(It.IsAny<ICharacter>())).Returns<ICharacter>(x => damBonus);
            DependencyContainer.Current.RegisterInstance<ITableValues>(attributeTablesMock.Object);

            INonPlayableCharacter npc = new NonPlayableCharacter(Guid.NewGuid(), new Blueprints.Character.CharacterNormalBlueprint { Id = 1, Name = "mob1", ActFlags = ActFlags.Noalign | ActFlags.Gain, OffensiveFlags = OffensiveFlags.AreaAttack | OffensiveFlags.Bash, CharacterFlags = CharacterFlags.Sanctuary | CharacterFlags.Regeneration, Level = 50, Sex = Sex.Neutral }, new Room.Room(Guid.NewGuid(), new Blueprints.Room.RoomBlueprint { Id = 1, Name = "room1" }, new Area.Area("Area1", 1, 100, "builders", "credits")));
            npc.Recompute();

            Assert.AreEqual(npc.Level + hitBonus, npc.HitRoll); // HitRoll is initialized with Level
            Assert.AreEqual(npc.Level + damBonus, npc.DamRoll); // DamRoll is initialized with Level
        }
    }
}
