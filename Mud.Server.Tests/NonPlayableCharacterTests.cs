using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Mud.Container;
using Mud.Domain;
using Mud.Server.Character.NonPlayableCharacter;
using Mud.Server.Interfaces.Area;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Class;
using Mud.Server.Interfaces.Race;
using Mud.Server.Interfaces.Table;
using Mud.Server.Random;

namespace Mud.Server.Tests
{
    [TestClass]
    public class NonPlayableCharacterTests
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
            var randomManagerMock = new Mock<IRandomManager>();
            randomManagerMock.Setup(x => x.Dice(It.IsAny<int>(), It.IsAny<int>())).Returns<int, int>((x, y) => x + y);
            DependencyContainer.Current.RegisterInstance<IRandomManager>(randomManagerMock.Object);
            var raceManagerMock = new Mock<IRaceManager>();
            DependencyContainer.Current.RegisterInstance<IRaceManager>(raceManagerMock.Object);
            var classManagerMock = new Mock<IClassManager>();
            DependencyContainer.Current.RegisterInstance<IClassManager>(classManagerMock.Object);

            INonPlayableCharacter npc = new NonPlayableCharacter(Guid.NewGuid(), new Blueprints.Character.CharacterNormalBlueprint { Id = 1, Name = "mob1", ActFlags = ActFlags.NoAlign | ActFlags.Gain, OffensiveFlags = OffensiveFlags.AreaAttack | OffensiveFlags.Bash, CharacterFlags = CharacterFlags.Sanctuary | CharacterFlags.Regeneration, Level = 50, Sex = Sex.Neutral }, new Room.Room(Guid.NewGuid(), new Blueprints.Room.RoomBlueprint { Id = 1, Name = "room1" }, new Mock<IArea>().Object));
            npc.Recompute();

            Assert.AreEqual(npc[CharacterAttributes.Strength], npc[BasicAttributes.Strength]);
            Assert.AreEqual(npc[CharacterAttributes.Intelligence], npc[BasicAttributes.Intelligence]);
            Assert.AreEqual(npc[CharacterAttributes.Wisdom], npc[BasicAttributes.Wisdom]);
            Assert.AreEqual(npc[CharacterAttributes.Dexterity], npc[BasicAttributes.Dexterity]);
            Assert.AreEqual(npc[CharacterAttributes.Constitution], npc[BasicAttributes.Constitution]);
        }

        [TestMethod]
        public void HitPoints_Test()
        {
            var attributeTablesMock = new Mock<ITableValues>();
            DependencyContainer.Current.RegisterInstance<ITableValues>(attributeTablesMock.Object);
            var randomManagerMock = new Mock<IRandomManager>();
            randomManagerMock.Setup(x => x.Dice(It.IsAny<int>(), It.IsAny<int>())).Returns<int, int>((x, y) => x * y);
            DependencyContainer.Current.RegisterInstance<IRandomManager>(randomManagerMock.Object);
            var raceManagerMock = new Mock<IRaceManager>();
            DependencyContainer.Current.RegisterInstance<IRaceManager>(raceManagerMock.Object);
            var classManagerMock = new Mock<IClassManager>();
            DependencyContainer.Current.RegisterInstance<IClassManager>(classManagerMock.Object);

            var characterBlueprint = new Blueprints.Character.CharacterNormalBlueprint 
            { 
                Id = 1, Name = "mob1", ActFlags = ActFlags.NoAlign | ActFlags.Gain, OffensiveFlags = OffensiveFlags.AreaAttack | OffensiveFlags.Bash, CharacterFlags = CharacterFlags.Sanctuary | CharacterFlags.Regeneration, Level = 50, Sex = Sex.Neutral,
                HitPointDiceCount = 5, HitPointDiceValue = 10, HitPointDiceBonus = 30,
            };
            INonPlayableCharacter npc = new NonPlayableCharacter(Guid.NewGuid(), characterBlueprint, new Room.Room(Guid.NewGuid(), new Blueprints.Room.RoomBlueprint { Id = 1, Name = "room1" }, new Mock<IArea>().Object));
            npc.Recompute();

            Assert.AreEqual(characterBlueprint.HitPointDiceCount * characterBlueprint.HitPointDiceValue + characterBlueprint.HitPointDiceBonus, npc.MaxHitPoints);
            Assert.AreEqual(characterBlueprint.HitPointDiceCount * characterBlueprint.HitPointDiceValue + characterBlueprint.HitPointDiceBonus, npc[CharacterAttributes.MaxHitPoints]);
            Assert.AreEqual(characterBlueprint.HitPointDiceCount * characterBlueprint.HitPointDiceValue + characterBlueprint.HitPointDiceBonus, npc.HitPoints);
        }

        [TestMethod]
        public void Mana_Test()
        {
            var attributeTablesMock = new Mock<ITableValues>();
            DependencyContainer.Current.RegisterInstance<ITableValues>(attributeTablesMock.Object);
            var randomManagerMock = new Mock<IRandomManager>();
            randomManagerMock.Setup(x => x.Dice(It.IsAny<int>(), It.IsAny<int>())).Returns<int, int>((x, y) => x * y);
            DependencyContainer.Current.RegisterInstance<IRandomManager>(randomManagerMock.Object);
            var raceManagerMock = new Mock<IRaceManager>();
            DependencyContainer.Current.RegisterInstance<IRaceManager>(raceManagerMock.Object);
            var classManagerMock = new Mock<IClassManager>();
            DependencyContainer.Current.RegisterInstance<IClassManager>(classManagerMock.Object);

            var characterBlueprint = new Blueprints.Character.CharacterNormalBlueprint
            {
                Id = 1, Name = "mob1", ActFlags = ActFlags.NoAlign | ActFlags.Gain, OffensiveFlags = OffensiveFlags.AreaAttack | OffensiveFlags.Bash, CharacterFlags = CharacterFlags.Sanctuary | CharacterFlags.Regeneration, Level = 50, Sex = Sex.Neutral,
                ManaDiceCount = 5,
                ManaDiceValue = 10,
                ManaDiceBonus = 30,
            };
            INonPlayableCharacter npc = new NonPlayableCharacter(Guid.NewGuid(), characterBlueprint, new Room.Room(Guid.NewGuid(), new Blueprints.Room.RoomBlueprint { Id = 1, Name = "room1" }, new Mock<IArea>().Object));
            npc.Recompute();

            Assert.AreEqual(characterBlueprint.ManaDiceCount * characterBlueprint.ManaDiceValue + characterBlueprint.ManaDiceBonus, npc.MaxResource(ResourceKinds.Mana));
            Assert.AreEqual(characterBlueprint.ManaDiceCount * characterBlueprint.ManaDiceValue + characterBlueprint.ManaDiceBonus, npc[ResourceKinds.Mana]);
        }

        [TestMethod]
        public void Armor_Test()
        {
            int defensiveBonus = 50;
            var attributeTablesMock = new Mock<ITableValues>();
            attributeTablesMock.Setup(x => x.DefensiveBonus(It.IsAny<ICharacter>())).Returns<ICharacter>(x => defensiveBonus);
            DependencyContainer.Current.RegisterInstance<ITableValues>(attributeTablesMock.Object);
            var randomManagerMock = new Mock<IRandomManager>();
            randomManagerMock.Setup(x => x.Dice(It.IsAny<int>(), It.IsAny<int>())).Returns<int, int>((x, y) => x + y);
            DependencyContainer.Current.RegisterInstance<IRandomManager>(randomManagerMock.Object);
            var raceManagerMock = new Mock<IRaceManager>();
            DependencyContainer.Current.RegisterInstance<IRaceManager>(raceManagerMock.Object);
            var classManagerMock = new Mock<IClassManager>();
            DependencyContainer.Current.RegisterInstance<IClassManager>(classManagerMock.Object);

            var characterBlueprint = new Blueprints.Character.CharacterNormalBlueprint 
            { 
                Id = 1, Name = "mob1", ActFlags = ActFlags.NoAlign | ActFlags.Gain, OffensiveFlags = OffensiveFlags.AreaAttack | OffensiveFlags.Bash, CharacterFlags = CharacterFlags.Sanctuary | CharacterFlags.Regeneration, Level = 50, Sex = Sex.Neutral,
                ArmorBash = 125, ArmorPierce = 130, ArmorSlash = 135, ArmorExotic = 140,
            };
            INonPlayableCharacter npc = new NonPlayableCharacter(Guid.NewGuid(), characterBlueprint, new Room.Room(Guid.NewGuid(), new Blueprints.Room.RoomBlueprint { Id = 1, Name = "room1" }, new Mock<IArea>().Object));
            npc.Recompute();

            Assert.AreEqual(characterBlueprint.ArmorBash + defensiveBonus, npc[Armors.Bash]); // Armor is initialized with -Level
            Assert.AreEqual(characterBlueprint.ArmorPierce + defensiveBonus, npc[Armors.Pierce]);
            Assert.AreEqual(characterBlueprint.ArmorSlash + defensiveBonus, npc[Armors.Slash]);
            Assert.AreEqual(characterBlueprint.ArmorExotic + defensiveBonus, npc[Armors.Exotic]);
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
            var randomManagerMock = new Mock<IRandomManager>();
            randomManagerMock.Setup(x => x.Dice(It.IsAny<int>(), It.IsAny<int>())).Returns<int, int>((x, y) => x + y);
            DependencyContainer.Current.RegisterInstance<IRandomManager>(randomManagerMock.Object);
            var raceManagerMock = new Mock<IRaceManager>();
            DependencyContainer.Current.RegisterInstance<IRaceManager>(raceManagerMock.Object);
            var classManagerMock = new Mock<IClassManager>();
            DependencyContainer.Current.RegisterInstance<IClassManager>(classManagerMock.Object);

            var characterBlueprint = new Blueprints.Character.CharacterNormalBlueprint 
            { 
                Id = 1, Name = "mob1", ActFlags = ActFlags.NoAlign | ActFlags.Gain, OffensiveFlags = OffensiveFlags.AreaAttack | OffensiveFlags.Bash, CharacterFlags = CharacterFlags.Sanctuary | CharacterFlags.Regeneration, Level = 50, Sex = Sex.Neutral,
                HitRollBonus = -10,
            };
            INonPlayableCharacter npc = new NonPlayableCharacter(Guid.NewGuid(), characterBlueprint, new Room.Room(Guid.NewGuid(), new Blueprints.Room.RoomBlueprint { Id = 1, Name = "room1" }, new Mock<IArea>().Object));
            npc.Recompute();

            Assert.AreEqual(characterBlueprint.HitRollBonus + hitBonus, npc.HitRoll); // HitRoll is initialized with Level
            Assert.AreEqual(npc.Level + damBonus, npc.DamRoll); // DamRoll is initialized with Level
        }
    }
}
