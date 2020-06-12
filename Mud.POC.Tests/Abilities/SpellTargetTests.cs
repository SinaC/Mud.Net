using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Mud.POC.Abilities;
using Mud.Server.Common;
using Mud.Server.Input;
using Mud.Server.Random;
using System.Linq;

namespace Mud.POC.Tests.Abilities
{
    [TestClass]
    public class SpellTargetTests : AbilityTestBase
    {
        // AbilityTargets.None
        [TestMethod]
        public void TargetNone_NoTargetSpecified_Test()
        {
            var randomManagerMock = new Mock<IRandomManager>();
            randomManagerMock
                .Setup(x => x.Chance(It.IsAny<int>()))
                .Returns(true); // always succeed
            IAbilityManager abilityManager = new AbilityManager(randomManagerMock.Object);
            var characterMock = new Mock<ICharacter>();
            characterMock.SetupGet(x => x.Level).Returns(100);
            characterMock.SetupGet(x => x.KnownAbilities).Returns(new[] { new KnownAbility { Ability = abilityManager["Mass invis"], Learned = 1, Level = 1 } });

            (string rawParameters, CommandParameter[] parameters) args = BuildParametersSkipFirst("'Mass invis'");
            AbilityTargetResults result = abilityManager.GetAbilityTarget(abilityManager["Mass invis"], characterMock.Object, out IEntity target, args.rawParameters, args.parameters);

            Assert.AreEqual(AbilityTargetResults.Ok, result);
            Assert.IsNull(target);
        }

        [TestMethod]
        public void TargetNone_AdditionalParameter_Test()
        {
            var randomManagerMock = new Mock<IRandomManager>();
            randomManagerMock
                .Setup(x => x.Chance(It.IsAny<int>()))
                .Returns(true); // always succeed
            IAbilityManager abilityManager = new AbilityManager(randomManagerMock.Object);
            var characterMock = new Mock<ICharacter>();
            characterMock.SetupGet(x => x.Level).Returns(100);
            characterMock.SetupGet(x => x.KnownAbilities).Returns(new[] { new KnownAbility { Ability = abilityManager["Mass invis"], Learned = 1, Level = 1 } });

            (string rawParameters, CommandParameter[] parameters) args = BuildParametersSkipFirst("'Mass invis' should not be specified");
            AbilityTargetResults result = abilityManager.GetAbilityTarget(abilityManager["Mass invis"], characterMock.Object, out IEntity target, args.rawParameters, args.parameters);

            Assert.AreEqual(AbilityTargetResults.Ok, result);
            Assert.IsNull(target);
        }

        // AbilityTargets.CharacterOffensive
        [TestMethod]
        public void TargetCharacterOffensive_NoTargetSpecified_NoFighting_Test()
        {
            var randomManagerMock = new Mock<IRandomManager>();
            randomManagerMock
                .Setup(x => x.Chance(It.IsAny<int>()))
                .Returns(true); // always succeed
            IAbilityManager abilityManager = new AbilityManager(randomManagerMock.Object);
            var characterMock = new Mock<ICharacter>();
            characterMock.SetupGet(x => x.Level).Returns(100);
            characterMock.SetupGet(x => x.KnownAbilities).Returns(new[] { new KnownAbility { Ability = abilityManager["Acid Blast"], Learned = 1, Level = 1 } });

            (string rawParameters, CommandParameter[] parameters) args = BuildParametersSkipFirst("'Acid Blast'");
            AbilityTargetResults result = abilityManager.GetAbilityTarget(abilityManager["Acid Blast"], characterMock.Object, out IEntity target, args.rawParameters, args.parameters);

            Assert.AreEqual(AbilityTargetResults.MissingParameter, result);
            Assert.IsNull(target);
        }

        [TestMethod]
        public void TargetCharacterOffensive_NoTargetSpecified_Fighting_Test()
        {
            var randomManagerMock = new Mock<IRandomManager>();
            randomManagerMock
                .Setup(x => x.Chance(It.IsAny<int>()))
                .Returns(true); // always succeed
            IAbilityManager abilityManager = new AbilityManager(randomManagerMock.Object);
            var mob1Mock = new Mock<ICharacter>();
            mob1Mock.SetupGet(x => x.Name).Returns("mob1");
            mob1Mock.SetupGet(x => x.Keywords).Returns(new[] { "mob1" });
            mob1Mock.SetupGet(x => x.Level).Returns(100);
            mob1Mock.SetupGet(x => x.KnownAbilities).Returns(new[] { new KnownAbility { Ability = abilityManager["Acid Blast"], Learned = 1, Level = 1 } });
            var mob2Mock = new Mock<ICharacter>();
            mob2Mock.SetupGet(x => x.Name).Returns("mob2");
            mob2Mock.SetupGet(x => x.Keywords).Returns(new[] { "mob2" });
            var roomMock = new Mock<IRoom>();
            roomMock.SetupGet(x => x.People).Returns(new[] { mob1Mock.Object, mob2Mock.Object });
            mob1Mock.SetupGet(x => x.Room).Returns(roomMock.Object);
            mob1Mock.SetupGet(x => x.Fighting).Returns(mob2Mock.Object);
            mob2Mock.SetupGet(x => x.Room).Returns(roomMock.Object);
            mob2Mock.SetupGet(x => x.Fighting).Returns(mob1Mock.Object);

            (string rawParameters, CommandParameter[] parameters) args = BuildParametersSkipFirst("'Acid Blast'");
            AbilityTargetResults result = abilityManager.GetAbilityTarget(abilityManager["Acid Blast"], mob1Mock.Object, out IEntity target, args.rawParameters, args.parameters);

            Assert.AreEqual(AbilityTargetResults.Ok, result);
            Assert.AreSame(mob2Mock.Object, target);
        }

        [TestMethod]
        public void TargetCharacterOffensive_InvalidTargetSpecified_Test()
        {
            var randomManagerMock = new Mock<IRandomManager>();
            randomManagerMock
                .Setup(x => x.Chance(It.IsAny<int>()))
                .Returns(true); // always succeed
            IAbilityManager abilityManager = new AbilityManager(randomManagerMock.Object);
            var characterMock = new Mock<ICharacter>();
            characterMock.SetupGet(x => x.Name).Returns("mob1");
            characterMock.SetupGet(x => x.Keywords).Returns(new[] { "mob1" });
            characterMock.SetupGet(x => x.Level).Returns(100);
            characterMock.SetupGet(x => x.KnownAbilities).Returns(new[] { new KnownAbility { Ability = abilityManager["Acid Blast"], Learned = 1, Level = 1 } });
            var roomMock = new Mock<IRoom>();
            roomMock.SetupGet(x => x.People).Returns(new[] { characterMock.Object });
            characterMock.SetupGet(x => x.Room).Returns(roomMock.Object);

            (string rawParameters, CommandParameter[] parameters) args = BuildParametersSkipFirst("'Acid Blast' mob2");
            AbilityTargetResults result = abilityManager.GetAbilityTarget(abilityManager["Acid Blast"], characterMock.Object, out IEntity target, args.rawParameters, args.parameters);

            Assert.AreEqual(AbilityTargetResults.TargetNotFound, result);
            Assert.IsNull(target);
        }

        [TestMethod]
        public void TargetCharacterOffensive_CorrectTarget_Test()
        {
            var randomManagerMock = new Mock<IRandomManager>();
            randomManagerMock
                .Setup(x => x.Chance(It.IsAny<int>()))
                .Returns(true); // always succeed
            IAbilityManager abilityManager = new AbilityManager(randomManagerMock.Object);
            var mob1Mock = new Mock<ICharacter>();
            mob1Mock.SetupGet(x => x.Name).Returns("mob1");
            mob1Mock.SetupGet(x => x.Keywords).Returns(new[] { "mob1" });
            mob1Mock.SetupGet(x => x.Level).Returns(100);
            mob1Mock.SetupGet(x => x.KnownAbilities).Returns(new[] { new KnownAbility { Ability = abilityManager["Acid Blast"], Learned = 1, Level = 1 } });
            var mob2Mock = new Mock<ICharacter>();
            mob2Mock.SetupGet(x => x.Name).Returns("mob2");
            mob2Mock.SetupGet(x => x.Keywords).Returns(new[] { "mob2" });
            var roomMock = new Mock<IRoom>();
            roomMock.SetupGet(x => x.People).Returns(new[] { mob1Mock.Object, mob2Mock.Object });
            mob1Mock.SetupGet(x => x.Room).Returns(roomMock.Object);
            mob2Mock.SetupGet(x => x.Room).Returns(roomMock.Object);

            (string rawParameters, CommandParameter[] parameters) args = BuildParametersSkipFirst("'Acid Blast' mob2");
            AbilityTargetResults result = abilityManager.GetAbilityTarget(abilityManager["Acid Blast"], mob1Mock.Object, out IEntity target, args.rawParameters, args.parameters);

            Assert.AreEqual(AbilityTargetResults.Ok, result);
            Assert.AreSame(mob2Mock.Object, target);
        }

        // AbilityTargets.CharacterDefensive
        [TestMethod]
        public void TargetCharacterDefensive_NoTargetSpecified_Test()
        {
            var randomManagerMock = new Mock<IRandomManager>();
            randomManagerMock
                .Setup(x => x.Chance(It.IsAny<int>()))
                .Returns(true); // always succeed
            IAbilityManager abilityManager = new AbilityManager(randomManagerMock.Object);
            var characterMock = new Mock<ICharacter>();
            characterMock.SetupGet(x => x.Level).Returns(100);
            characterMock.SetupGet(x => x.KnownAbilities).Returns(new[] { new KnownAbility { Ability = abilityManager["Armor"], Learned = 1, Level = 1 } });

            (string rawParameters, CommandParameter[] parameters) args = BuildParametersSkipFirst("Armor");
            AbilityTargetResults result = abilityManager.GetAbilityTarget(abilityManager["Armor"], characterMock.Object, out IEntity target, args.rawParameters, args.parameters);

            Assert.AreEqual(AbilityTargetResults.Ok, result);
            Assert.AreSame(characterMock.Object, target);
        }

        [TestMethod]
        public void TargetCharacterDefensive_TargetSelfSpecified_Test()
        {
            var randomManagerMock = new Mock<IRandomManager>();
            randomManagerMock
                .Setup(x => x.Chance(It.IsAny<int>()))
                .Returns(true); // always succeed
            IAbilityManager abilityManager = new AbilityManager(randomManagerMock.Object);
            var characterMock = new Mock<ICharacter>();
            characterMock.SetupGet(x => x.Name).Returns("mob1");
            characterMock.SetupGet(x => x.Keywords).Returns(new[] { "mob1" });
            characterMock.SetupGet(x => x.Level).Returns(100);
            characterMock.SetupGet(x => x.KnownAbilities).Returns(new[] { new KnownAbility { Ability = abilityManager["Armor"], Learned = 1, Level = 1 } });
            var roomMock = new Mock<IRoom>();
            roomMock.SetupGet(x => x.People).Returns(new[] { characterMock.Object });
            characterMock.SetupGet(x => x.Room).Returns(roomMock.Object);

            (string rawParameters, CommandParameter[] parameters) args = BuildParametersSkipFirst("Armor mob1");
            AbilityTargetResults result = abilityManager.GetAbilityTarget(abilityManager["Armor"], characterMock.Object, out IEntity target, args.rawParameters, args.parameters);

            Assert.AreEqual(AbilityTargetResults.Ok, result);
            Assert.AreSame(characterMock.Object, target);
        }

        [TestMethod]
        public void TargetCharacterDefensive_TargetSpecified_Test()
        {
            var randomManagerMock = new Mock<IRandomManager>();
            randomManagerMock
                .Setup(x => x.Chance(It.IsAny<int>()))
                .Returns(true); // always succeed
            IAbilityManager abilityManager = new AbilityManager(randomManagerMock.Object);
            var mob1Mock = new Mock<ICharacter>();
            mob1Mock.SetupGet(x => x.Name).Returns("mob1");
            mob1Mock.SetupGet(x => x.Keywords).Returns(new[] { "mob1" });
            mob1Mock.SetupGet(x => x.Level).Returns(100);
            mob1Mock.SetupGet(x => x.KnownAbilities).Returns(new[] { new KnownAbility { Ability = abilityManager["Armor"], Learned = 1, Level = 1 } });
            var mob2Mock = new Mock<ICharacter>();
            mob2Mock.SetupGet(x => x.Name).Returns("mob2");
            mob2Mock.SetupGet(x => x.Keywords).Returns(new[] { "mob2" });
            var roomMock = new Mock<IRoom>();
            roomMock.SetupGet(x => x.People).Returns(new[] { mob1Mock.Object, mob2Mock.Object });
            mob1Mock.SetupGet(x => x.Room).Returns(roomMock.Object);
            mob2Mock.SetupGet(x => x.Room).Returns(roomMock.Object);

            (string rawParameters, CommandParameter[] parameters) args = BuildParametersSkipFirst("Armor mob2");
            AbilityTargetResults result = abilityManager.GetAbilityTarget(abilityManager["Armor"], mob1Mock.Object, out IEntity target, args.rawParameters, args.parameters);

            Assert.AreEqual(AbilityTargetResults.Ok, result);
            Assert.AreSame(mob2Mock.Object, target);
        }

        // AbilityTargets.CharacterSelf
        [TestMethod]
        public void TargetCharacterSelf_NoTargetSpecified_Test()
        {
            var randomManagerMock = new Mock<IRandomManager>();
            randomManagerMock
                .Setup(x => x.Chance(It.IsAny<int>()))
                .Returns(true); // always succeed
            IAbilityManager abilityManager = new AbilityManager(randomManagerMock.Object);
            var characterMock = new Mock<ICharacter>();
            characterMock.SetupGet(x => x.Level).Returns(100);
            characterMock.SetupGet(x => x.KnownAbilities).Returns(new[] { new KnownAbility { Ability = abilityManager["Detect Evil"], Learned = 1, Level = 1 } });

            (string rawParameters, CommandParameter[] parameters) args = BuildParametersSkipFirst("'detect evil'");
            AbilityTargetResults result = abilityManager.GetAbilityTarget(abilityManager["detect evil"], characterMock.Object, out IEntity target, args.rawParameters, args.parameters);

            Assert.AreEqual(AbilityTargetResults.Ok, result);
            Assert.AreSame(characterMock.Object, target);
        }

        [TestMethod]
        public void TargetCharacterSelf_TargetSelfSpecified_Test()
        {
            var randomManagerMock = new Mock<IRandomManager>();
            randomManagerMock
                .Setup(x => x.Chance(It.IsAny<int>()))
                .Returns(true); // always succeed
            IAbilityManager abilityManager = new AbilityManager(randomManagerMock.Object);
            var characterMock = new Mock<ICharacter>();
            characterMock.SetupGet(x => x.Name).Returns("mob1");
            characterMock.SetupGet(x => x.Keywords).Returns(new[] { "mob1" });
            characterMock.SetupGet(x => x.Level).Returns(100);
            characterMock.SetupGet(x => x.KnownAbilities).Returns(new[] { new KnownAbility { Ability = abilityManager["detect evil"], Learned = 1, Level = 1 } });
            var roomMock = new Mock<IRoom>();
            roomMock.SetupGet(x => x.People).Returns(new[] { characterMock.Object });
            characterMock.SetupGet(x => x.Room).Returns(roomMock.Object);

            (string rawParameters, CommandParameter[] parameters) args = BuildParametersSkipFirst("'detect evil' mob1");
            AbilityTargetResults result = abilityManager.GetAbilityTarget(abilityManager["detect evil"], characterMock.Object, out IEntity target, args.rawParameters, args.parameters);

            Assert.AreEqual(AbilityTargetResults.Ok, result);
            Assert.AreSame(characterMock.Object, target);
        }

        [TestMethod]
        public void TargetCharacterSelf_TargetNotFoundSpecified_Test()
        {
            var randomManagerMock = new Mock<IRandomManager>();
            randomManagerMock
                .Setup(x => x.Chance(It.IsAny<int>()))
                .Returns(true); // always succeed
            IAbilityManager abilityManager = new AbilityManager(randomManagerMock.Object);
            var mob1Mock = new Mock<ICharacter>();
            mob1Mock.SetupGet(x => x.Name).Returns("mob1");
            mob1Mock.SetupGet(x => x.Keywords).Returns(new[] { "mob1" });
            mob1Mock.SetupGet(x => x.Level).Returns(100);
            mob1Mock.SetupGet(x => x.KnownAbilities).Returns(new[] { new KnownAbility { Ability = abilityManager["Detect Evil"], Learned = 1, Level = 1 } });
            var mob2Mock = new Mock<ICharacter>();
            mob2Mock.SetupGet(x => x.Name).Returns("mob2");
            mob2Mock.SetupGet(x => x.Keywords).Returns(new[] { "mob2" });
            var roomMock = new Mock<IRoom>();
            roomMock.SetupGet(x => x.People).Returns(new[] { mob1Mock.Object, mob2Mock.Object });
            mob1Mock.SetupGet(x => x.Room).Returns(roomMock.Object);
            mob2Mock.SetupGet(x => x.Room).Returns(roomMock.Object);

            (string rawParameters, CommandParameter[] parameters) args = BuildParametersSkipFirst("'detect evil' mob3");
            AbilityTargetResults result = abilityManager.GetAbilityTarget(abilityManager["detect evil"], mob1Mock.Object, out IEntity target, args.rawParameters, args.parameters);

            Assert.AreEqual(AbilityTargetResults.InvalidTarget, result);
            Assert.IsNull(target);
        }

        [TestMethod]
        public void TargetCharacterSelf_TargetSpecified_Test()
        {
            var randomManagerMock = new Mock<IRandomManager>();
            randomManagerMock
                .Setup(x => x.Chance(It.IsAny<int>()))
                .Returns(true); // always succeed
            IAbilityManager abilityManager = new AbilityManager(randomManagerMock.Object);
            var mob1Mock = new Mock<ICharacter>();
            mob1Mock.SetupGet(x => x.Name).Returns("mob1");
            mob1Mock.SetupGet(x => x.Keywords).Returns(new[] { "mob1" });
            mob1Mock.SetupGet(x => x.Level).Returns(100);
            mob1Mock.SetupGet(x => x.KnownAbilities).Returns(new[] { new KnownAbility { Ability = abilityManager["Detect Evil"], Learned = 1, Level = 1 } });
            var mob2Mock = new Mock<ICharacter>();
            mob2Mock.SetupGet(x => x.Name).Returns("mob2");
            mob2Mock.SetupGet(x => x.Keywords).Returns(new[] { "mob2" });
            var roomMock = new Mock<IRoom>();
            roomMock.SetupGet(x => x.People).Returns(new[] { mob1Mock.Object, mob2Mock.Object });
            mob1Mock.SetupGet(x => x.Room).Returns(roomMock.Object);
            mob2Mock.SetupGet(x => x.Room).Returns(roomMock.Object);

            (string rawParameters, CommandParameter[] parameters) args = BuildParametersSkipFirst("'detect evil' mob2");
            AbilityTargetResults result = abilityManager.GetAbilityTarget(abilityManager["detect evil"], mob1Mock.Object, out IEntity target, args.rawParameters, args.parameters);

            Assert.AreEqual(AbilityTargetResults.InvalidTarget, result);
            Assert.IsNull(target);
        }

        // AbilityTargets.ItemInventory
        [TestMethod]
        public void TargetItemInventory_NoTargetSpecified_Test()
        {
            var randomManagerMock = new Mock<IRandomManager>();
            randomManagerMock
                .Setup(x => x.Chance(It.IsAny<int>()))
                .Returns(true); // always succeed
            IAbilityManager abilityManager = new AbilityManager(randomManagerMock.Object);
            var itemMock = new Mock<IItem>();
            itemMock.SetupGet(x => x.Name).Returns("item1");
            itemMock.SetupGet(x => x.Keywords).Returns(new[] { "item1" });
            var characterMock = new Mock<ICharacter>();
            characterMock.SetupGet(x => x.Level).Returns(100);
            characterMock.SetupGet(x => x.Inventory).Returns(new[] { itemMock.Object });
            characterMock.SetupGet(x => x.KnownAbilities).Returns(new[] { new KnownAbility { Ability = abilityManager["Identify"], Learned = 1, Level = 1 } });

            (string rawParameters, CommandParameter[] parameters) args = BuildParametersSkipFirst("Identify");
            AbilityTargetResults result = abilityManager.GetAbilityTarget(abilityManager["Identify"], characterMock.Object, out IEntity target, args.rawParameters, args.parameters);

            Assert.AreEqual(AbilityTargetResults.MissingParameter, result);
            Assert.IsNull(target);
        }

        [TestMethod]
        public void TargetItemInventory_TargetNotFoundSpecified_Test()
        {
            var randomManagerMock = new Mock<IRandomManager>();
            randomManagerMock
                .Setup(x => x.Chance(It.IsAny<int>()))
                .Returns(true); // always succeed
            IAbilityManager abilityManager = new AbilityManager(randomManagerMock.Object);
            var itemMock = new Mock<IItem>();
            itemMock.SetupGet(x => x.Name).Returns("item1");
            itemMock.SetupGet(x => x.Keywords).Returns(new[] { "item1" });
            var characterMock = new Mock<ICharacter>();
            characterMock.SetupGet(x => x.Name).Returns("mob1");
            characterMock.SetupGet(x => x.Keywords).Returns(new[] { "mob1" });
            characterMock.SetupGet(x => x.Level).Returns(100);
            characterMock.SetupGet(x => x.Inventory).Returns(new[] { itemMock.Object });
            characterMock.SetupGet(x => x.KnownAbilities).Returns(new[] { new KnownAbility { Ability = abilityManager["Identify"], Learned = 1, Level = 1 } });

            (string rawParameters, CommandParameter[] parameters) args = BuildParametersSkipFirst("Identify mob2");
            AbilityTargetResults result = abilityManager.GetAbilityTarget(abilityManager["Identify"], characterMock.Object, out IEntity target, args.rawParameters, args.parameters);

            Assert.AreEqual(AbilityTargetResults.TargetNotFound, result);
            Assert.IsNull(target);
        }

        [TestMethod]
        public void AbilityManager_Cast_TargetItemInventory_InvalidTargetSpecified_Test()
        {
            var randomManagerMock = new Mock<IRandomManager>();
            randomManagerMock
                .Setup(x => x.Chance(It.IsAny<int>()))
                .Returns(true); // always succeed
            IAbilityManager abilityManager = new AbilityManager(randomManagerMock.Object);
            var itemMock = new Mock<IItem>();
            itemMock.SetupGet(x => x.Name).Returns("item1");
            itemMock.SetupGet(x => x.Keywords).Returns(new[] { "item1" });
            var characterMock = new Mock<ICharacter>();
            characterMock.SetupGet(x => x.Name).Returns("mob1");
            characterMock.SetupGet(x => x.Keywords).Returns(new[] { "mob1" });
            characterMock.SetupGet(x => x.Level).Returns(100);
            characterMock.SetupGet(x => x.Inventory).Returns(new[] { itemMock.Object });
            characterMock.SetupGet(x => x.KnownAbilities).Returns(new[] { new KnownAbility { Ability = abilityManager["Identify"], Learned = 1, Level = 1 } });

            (string rawParameters, CommandParameter[] parameters) args = BuildParametersSkipFirst("Identify mob1");
            AbilityTargetResults result = abilityManager.GetAbilityTarget(abilityManager["Identify"], characterMock.Object, out IEntity target, args.rawParameters, args.parameters);

            Assert.AreEqual(AbilityTargetResults.TargetNotFound, result);
            Assert.IsNull(target);
        }

        [TestMethod]
        public void TargetItemInventory_TargetSpecified_Test()
        {
            var randomManagerMock = new Mock<IRandomManager>();
            randomManagerMock
                .Setup(x => x.Chance(It.IsAny<int>()))
                .Returns(true); // always succeed
            IAbilityManager abilityManager = new AbilityManager(randomManagerMock.Object);
            var itemMock = new Mock<IItem>();
            itemMock.SetupGet(x => x.Name).Returns("item1");
            itemMock.SetupGet(x => x.Keywords).Returns(new[] { "item1" });
            var characterMock = new Mock<ICharacter>();
            characterMock.SetupGet(x => x.Level).Returns(100);
            characterMock.SetupGet(x => x.Inventory).Returns(new[] { itemMock.Object });
            characterMock.SetupGet(x => x.KnownAbilities).Returns(new[] { new KnownAbility { Ability = abilityManager["Identify"], Learned = 1, Level = 1 } });

            (string rawParameters, CommandParameter[] parameters) args = BuildParametersSkipFirst("Identify item1");
            AbilityTargetResults result = abilityManager.GetAbilityTarget(abilityManager["Identify"], characterMock.Object, out IEntity target, args.rawParameters, args.parameters);

            Assert.AreEqual(AbilityTargetResults.Ok, result);
            Assert.AreSame(itemMock.Object, target);
        }

        [TestMethod]
        public void TargetItemInventory_TargetInRoomSpecified_Test()
        {
            var randomManagerMock = new Mock<IRandomManager>();
            randomManagerMock
                .Setup(x => x.Chance(It.IsAny<int>()))
                .Returns(true); // always succeed
            IAbilityManager abilityManager = new AbilityManager(randomManagerMock.Object);
            var itemMock = new Mock<IItem>();
            itemMock.SetupGet(x => x.Name).Returns("item1");
            itemMock.SetupGet(x => x.Keywords).Returns(new[] { "item1" });
            var characterMock = new Mock<ICharacter>();
            characterMock.SetupGet(x => x.Level).Returns(100);
            characterMock.SetupGet(x => x.Inventory).Returns(new IItem[] { });
            characterMock.SetupGet(x => x.KnownAbilities).Returns(new[] { new KnownAbility { Ability = abilityManager["Identify"], Learned = 1, Level = 1 } });
            var roomMock = new Mock<IRoom>();
            roomMock.SetupGet(x => x.Content).Returns(new[] { itemMock.Object });

            (string rawParameters, CommandParameter[] parameters) args = BuildParametersSkipFirst("Identify item1");
            AbilityTargetResults result = abilityManager.GetAbilityTarget(abilityManager["Identify"], characterMock.Object, out IEntity target, args.rawParameters, args.parameters);

            Assert.AreEqual(AbilityTargetResults.TargetNotFound, result);
            Assert.IsNull(target);
        }

        //  AbilityTargets.ItemHereOrCharacterOffensive
        [TestMethod]
        public void TargetItemHereOrCharacterOffensive_NoTargetSpecified_Test()
        {
            var randomManagerMock = new Mock<IRandomManager>();
            randomManagerMock
                .Setup(x => x.Chance(It.IsAny<int>()))
                .Returns(true); // always succeed
            IAbilityManager abilityManager = new AbilityManager(randomManagerMock.Object);
            var itemMock = new Mock<IItem>();
            itemMock.SetupGet(x => x.Name).Returns("item1");
            itemMock.SetupGet(x => x.Keywords).Returns(new[] { "item1" });
            var characterMock = new Mock<ICharacter>();
            characterMock.SetupGet(x => x.Level).Returns(100);
            characterMock.SetupGet(x => x.Inventory).Returns(new[] { itemMock.Object });
            characterMock.SetupGet(x => x.KnownAbilities).Returns(new[] { new KnownAbility { Ability = abilityManager["Curse"], Learned = 1, Level = 1 } });

            (string rawParameters, CommandParameter[] parameters) args = BuildParametersSkipFirst("Curse");
            AbilityTargetResults result = abilityManager.GetAbilityTarget(abilityManager["Curse"], characterMock.Object, out IEntity target, args.rawParameters, args.parameters);

            Assert.AreEqual(AbilityTargetResults.MissingParameter, result);
            Assert.IsNull(target);
        }

        [TestMethod]
        public void TargetItemHereOrCharacterOffensive_InvalidTargetSpecified_Test()
        {
            var randomManagerMock = new Mock<IRandomManager>();
            randomManagerMock
                .Setup(x => x.Chance(It.IsAny<int>()))
                .Returns(true); // always succeed
            IAbilityManager abilityManager = new AbilityManager(randomManagerMock.Object);
            var itemMock = new Mock<IItem>();
            itemMock.SetupGet(x => x.Name).Returns("item1");
            itemMock.SetupGet(x => x.Keywords).Returns(new[] { "item1" });
            var characterMock = new Mock<ICharacter>();
            characterMock.SetupGet(x => x.Level).Returns(100);
            characterMock.SetupGet(x => x.KnownAbilities).Returns(new[] { new KnownAbility { Ability = abilityManager["Curse"], Learned = 1, Level = 1 } });
            var roomMock = new Mock<IRoom>();
            roomMock.SetupGet(x => x.People).Returns(new[] { characterMock.Object });
            roomMock.SetupGet(x => x.Content).Returns(new[] { itemMock.Object });
            characterMock.SetupGet(x => x.Room).Returns(roomMock.Object);

            (string rawParameters, CommandParameter[] parameters) args = BuildParametersSkipFirst("Curse item2");
            AbilityTargetResults result = abilityManager.GetAbilityTarget(abilityManager["Curse"], characterMock.Object, out IEntity target, args.rawParameters, args.parameters);

            Assert.AreEqual(AbilityTargetResults.TargetNotFound, result);
            Assert.IsNull(target);
        }

        [TestMethod]
        public void TargetItemHereOrCharacterOffensive_ItemTargetInRoomSpecified_Test()
        {
            var randomManagerMock = new Mock<IRandomManager>();
            randomManagerMock
                .Setup(x => x.Chance(It.IsAny<int>()))
                .Returns(true); // always succeed
            IAbilityManager abilityManager = new AbilityManager(randomManagerMock.Object);
            var itemMock = new Mock<IItem>();
            itemMock.SetupGet(x => x.Name).Returns("item1");
            itemMock.SetupGet(x => x.Keywords).Returns(new[] { "item1" });
            var characterMock = new Mock<ICharacter>();
            characterMock.SetupGet(x => x.Level).Returns(100);
            characterMock.SetupGet(x => x.KnownAbilities).Returns(new[] { new KnownAbility { Ability = abilityManager["Curse"], Learned = 1, Level = 1 } });
            var roomMock = new Mock<IRoom>();
            roomMock.SetupGet(x => x.People).Returns(new[] { characterMock.Object });
            roomMock.SetupGet(x => x.Content).Returns(new[] { itemMock.Object });
            characterMock.SetupGet(x => x.Room).Returns(roomMock.Object);

            (string rawParameters, CommandParameter[] parameters) args = BuildParametersSkipFirst("Curse item1");
            AbilityTargetResults result = abilityManager.GetAbilityTarget(abilityManager["Curse"], characterMock.Object, out IEntity target, args.rawParameters, args.parameters);

            Assert.AreEqual(AbilityTargetResults.Ok, result);
            Assert.AreSame(itemMock.Object, target);
        }

        [TestMethod]
        public void TargetItemHereOrCharacterOffensive_ItemTargetInInventorySpecified_Test()
        {
            var randomManagerMock = new Mock<IRandomManager>();
            randomManagerMock
                .Setup(x => x.Chance(It.IsAny<int>()))
                .Returns(true); // always succeed
            IAbilityManager abilityManager = new AbilityManager(randomManagerMock.Object);
            var itemMock = new Mock<IItem>();
            itemMock.SetupGet(x => x.Name).Returns("item1");
            itemMock.SetupGet(x => x.Keywords).Returns(new[] { "item1" });
            var characterMock = new Mock<ICharacter>();
            characterMock.SetupGet(x => x.Level).Returns(100);
            characterMock.SetupGet(x => x.Inventory).Returns(new[] { itemMock.Object });
            characterMock.SetupGet(x => x.KnownAbilities).Returns(new[] { new KnownAbility { Ability = abilityManager["Curse"], Learned = 1, Level = 1 } });
            var roomMock = new Mock<IRoom>();
            roomMock.SetupGet(x => x.People).Returns(new[] { characterMock.Object });
            characterMock.SetupGet(x => x.Room).Returns(roomMock.Object);

            (string rawParameters, CommandParameter[] parameters) args = BuildParametersSkipFirst("Curse item1");
            AbilityTargetResults result = abilityManager.GetAbilityTarget(abilityManager["Curse"], characterMock.Object, out IEntity target, args.rawParameters, args.parameters);

            Assert.AreEqual(AbilityTargetResults.Ok, result);
            Assert.AreSame(itemMock.Object, target);
        }

        [TestMethod]
        public void TargetItemHereOrCharacterOffensive_ItemTargetInEquipmentSpecified_Test()
        {
            var randomManagerMock = new Mock<IRandomManager>();
            randomManagerMock
                .Setup(x => x.Chance(It.IsAny<int>()))
                .Returns(true); // always succeed
            IAbilityManager abilityManager = new AbilityManager(randomManagerMock.Object);
            var itemMock = new Mock<IItem>();
            itemMock.SetupGet(x => x.Name).Returns("item1");
            itemMock.SetupGet(x => x.Keywords).Returns(new[] { "item1" });
            var characterMock = new Mock<ICharacter>();
            characterMock.SetupGet(x => x.Level).Returns(100);
            characterMock.SetupGet(x => x.Equipments).Returns(new[] { itemMock.Object });
            characterMock.SetupGet(x => x.KnownAbilities).Returns(new[] { new KnownAbility { Ability = abilityManager["Curse"], Learned = 1, Level = 1 } });
            var roomMock = new Mock<IRoom>();
            roomMock.SetupGet(x => x.People).Returns(new[] { characterMock.Object });
            characterMock.SetupGet(x => x.Room).Returns(roomMock.Object);

            (string rawParameters, CommandParameter[] parameters) args = BuildParametersSkipFirst("Curse item1");
            AbilityTargetResults result = abilityManager.GetAbilityTarget(abilityManager["Curse"], characterMock.Object, out IEntity target, args.rawParameters, args.parameters);

            Assert.AreEqual(AbilityTargetResults.Ok, result);
            Assert.AreSame(itemMock.Object, target);
        }

        [TestMethod]
        public void TargetItemHereOrCharacterOffensive_CharacterTargetSpecified_Test()
        {
            var randomManagerMock = new Mock<IRandomManager>();
            randomManagerMock
                .Setup(x => x.Chance(It.IsAny<int>()))
                .Returns(true); // always succeed
            IAbilityManager abilityManager = new AbilityManager(randomManagerMock.Object);
            var itemMock = new Mock<IItem>();
            itemMock.SetupGet(x => x.Name).Returns("item1");
            itemMock.SetupGet(x => x.Keywords).Returns(new[] { "item1" });
            var characterMock = new Mock<ICharacter>();
            characterMock.SetupGet(x => x.Name).Returns("mob1");
            characterMock.SetupGet(x => x.Keywords).Returns(new[] { "mob1" });
            characterMock.SetupGet(x => x.Level).Returns(100);
            characterMock.SetupGet(x => x.Equipments).Returns(new[] { itemMock.Object });
            characterMock.SetupGet(x => x.KnownAbilities).Returns(new[] { new KnownAbility { Ability = abilityManager["Curse"], Learned = 1, Level = 1 } });
            var roomMock = new Mock<IRoom>();
            roomMock.SetupGet(x => x.People).Returns(new[] { characterMock.Object });
            characterMock.SetupGet(x => x.Room).Returns(roomMock.Object);

            (string rawParameters, CommandParameter[] parameters) args = BuildParametersSkipFirst("Curse mob1");
            AbilityTargetResults result = abilityManager.GetAbilityTarget(abilityManager["Curse"], characterMock.Object, out IEntity target, args.rawParameters, args.parameters);

            Assert.AreEqual(AbilityTargetResults.Ok, result);
            Assert.AreSame(characterMock.Object, target);
        }

        // AbilityTargets.ItemInventoryOrCharacterDefensive
        [TestMethod]
        public void TargetItemInventoryOrCharacterDefensive_NoTargetSpecified_Test()
        {
            var randomManagerMock = new Mock<IRandomManager>();
            randomManagerMock
                .Setup(x => x.Chance(It.IsAny<int>()))
                .Returns(true); // always succeed
            IAbilityManager abilityManager = new AbilityManager(randomManagerMock.Object);
            var itemMock = new Mock<IItem>();
            itemMock.SetupGet(x => x.Name).Returns("item1");
            itemMock.SetupGet(x => x.Keywords).Returns(new[] { "item1" });
            var characterMock = new Mock<ICharacter>();
            characterMock.SetupGet(x => x.Level).Returns(100);
            characterMock.SetupGet(x => x.Inventory).Returns(new[] { itemMock.Object });
            characterMock.SetupGet(x => x.KnownAbilities).Returns(new[] { new KnownAbility { Ability = abilityManager["Bless"], Learned = 1, Level = 1 } });

            (string rawParameters, CommandParameter[] parameters) args = BuildParametersSkipFirst("Bless");
            AbilityTargetResults result = abilityManager.GetAbilityTarget(abilityManager["Bless"], characterMock.Object, out IEntity target, args.rawParameters, args.parameters);

            Assert.AreEqual(AbilityTargetResults.Ok, result);
            Assert.AreSame(characterMock.Object, target);
        }

        [TestMethod]
        public void TargetItemInventoryOrCharacterDefensive_InvalidTargetSpecified_Test()
        {
            var randomManagerMock = new Mock<IRandomManager>();
            randomManagerMock
                .Setup(x => x.Chance(It.IsAny<int>()))
                .Returns(true); // always succeed
            IAbilityManager abilityManager = new AbilityManager(randomManagerMock.Object);
            var itemMock = new Mock<IItem>();
            itemMock.SetupGet(x => x.Name).Returns("item1");
            itemMock.SetupGet(x => x.Keywords).Returns(new[] { "item1" });
            var characterMock = new Mock<ICharacter>();
            characterMock.SetupGet(x => x.Level).Returns(100);
            characterMock.SetupGet(x => x.KnownAbilities).Returns(new[] { new KnownAbility { Ability = abilityManager["Bless"], Learned = 1, Level = 1 } });
            var roomMock = new Mock<IRoom>();
            roomMock.SetupGet(x => x.People).Returns(new[] { characterMock.Object });
            roomMock.SetupGet(x => x.Content).Returns(new[] { itemMock.Object });
            characterMock.SetupGet(x => x.Room).Returns(roomMock.Object);

            (string rawParameters, CommandParameter[] parameters) args = BuildParametersSkipFirst("Bless item2");
            AbilityTargetResults result = abilityManager.GetAbilityTarget(abilityManager["Bless"], characterMock.Object, out IEntity target, args.rawParameters, args.parameters);

            Assert.AreEqual(AbilityTargetResults.TargetNotFound, result);
            Assert.IsNull(target);
        }

        [TestMethod]
        public void TargetItemInventoryOrCharacterDefensive_ItemTargetInRoomSpecified_Test()
        {
            var randomManagerMock = new Mock<IRandomManager>();
            randomManagerMock
                .Setup(x => x.Chance(It.IsAny<int>()))
                .Returns(true); // always succeed
            IAbilityManager abilityManager = new AbilityManager(randomManagerMock.Object);
            var itemMock = new Mock<IItem>();
            itemMock.SetupGet(x => x.Name).Returns("item1");
            itemMock.SetupGet(x => x.Keywords).Returns(new[] { "item1" });
            var characterMock = new Mock<ICharacter>();
            characterMock.SetupGet(x => x.Level).Returns(100);
            characterMock.SetupGet(x => x.KnownAbilities).Returns(new[] { new KnownAbility { Ability = abilityManager["Bless"], Learned = 1, Level = 1 } });
            var roomMock = new Mock<IRoom>();
            roomMock.SetupGet(x => x.People).Returns(new[] { characterMock.Object });
            roomMock.SetupGet(x => x.Content).Returns(new[] { itemMock.Object });
            characterMock.SetupGet(x => x.Room).Returns(roomMock.Object);

            (string rawParameters, CommandParameter[] parameters) args = BuildParametersSkipFirst("Bless item1");
            AbilityTargetResults result = abilityManager.GetAbilityTarget(abilityManager["Bless"], characterMock.Object, out IEntity target, args.rawParameters, args.parameters);

            Assert.AreEqual(AbilityTargetResults.TargetNotFound, result);
            Assert.IsNull(target);
        }

        [TestMethod]
        public void TargetItemInventoryOrCharacterDefensive_ItemTargetInInventorySpecified_Test()
        {
            var randomManagerMock = new Mock<IRandomManager>();
            randomManagerMock
                .Setup(x => x.Chance(It.IsAny<int>()))
                .Returns(true); // always succeed
            IAbilityManager abilityManager = new AbilityManager(randomManagerMock.Object);
            var itemMock = new Mock<IItem>();
            itemMock.SetupGet(x => x.Name).Returns("item1");
            itemMock.SetupGet(x => x.Keywords).Returns(new[] { "item1" });
            var characterMock = new Mock<ICharacter>();
            characterMock.SetupGet(x => x.Level).Returns(100);
            characterMock.SetupGet(x => x.Inventory).Returns(new[] { itemMock.Object });
            characterMock.SetupGet(x => x.KnownAbilities).Returns(new[] { new KnownAbility { Ability = abilityManager["Bless"], Learned = 1, Level = 1 } });
            var roomMock = new Mock<IRoom>();
            roomMock.SetupGet(x => x.People).Returns(new[] { characterMock.Object });
            characterMock.SetupGet(x => x.Room).Returns(roomMock.Object);

            (string rawParameters, CommandParameter[] parameters) args = BuildParametersSkipFirst("Bless item1");
            AbilityTargetResults result = abilityManager.GetAbilityTarget(abilityManager["Bless"], characterMock.Object, out IEntity target, args.rawParameters, args.parameters);

            Assert.AreEqual(AbilityTargetResults.Ok, result);
            Assert.AreSame(itemMock.Object, target);
        }

        [TestMethod]
        public void TargetItemInventoryOrCharacterDefensive_ItemTargetInEquipmentSpecified_Test()
        {
            var randomManagerMock = new Mock<IRandomManager>();
            randomManagerMock
                .Setup(x => x.Chance(It.IsAny<int>()))
                .Returns(true); // always succeed
            IAbilityManager abilityManager = new AbilityManager(randomManagerMock.Object);
            var itemMock = new Mock<IItem>();
            itemMock.SetupGet(x => x.Name).Returns("item1");
            itemMock.SetupGet(x => x.Keywords).Returns(new[] { "item1" });
            var characterMock = new Mock<ICharacter>();
            characterMock.SetupGet(x => x.Level).Returns(100);
            characterMock.SetupGet(x => x.Equipments).Returns(new[] { itemMock.Object });
            characterMock.SetupGet(x => x.KnownAbilities).Returns(new[] { new KnownAbility { Ability = abilityManager["Bless"], Learned = 1, Level = 1 } });
            var roomMock = new Mock<IRoom>();
            roomMock.SetupGet(x => x.People).Returns(new[] { characterMock.Object });
            characterMock.SetupGet(x => x.Room).Returns(roomMock.Object);

            (string rawParameters, CommandParameter[] parameters) args = BuildParametersSkipFirst("Bless item1");
            AbilityTargetResults result = abilityManager.GetAbilityTarget(abilityManager["Bless"], characterMock.Object, out IEntity target, args.rawParameters, args.parameters);

            Assert.AreEqual(AbilityTargetResults.TargetNotFound, result);
            Assert.IsNull(target);
        }

        [TestMethod]
        public void TargetItemInventoryOrCharacterDefensive_CharacterTargetSpecified_Test()
        {
            var randomManagerMock = new Mock<IRandomManager>();
            randomManagerMock
                .Setup(x => x.Chance(It.IsAny<int>()))
                .Returns(true); // always succeed
            IAbilityManager abilityManager = new AbilityManager(randomManagerMock.Object);
            var itemMock = new Mock<IItem>();
            itemMock.SetupGet(x => x.Name).Returns("item1");
            itemMock.SetupGet(x => x.Keywords).Returns(new[] { "item1" });
            var characterMock = new Mock<ICharacter>();
            characterMock.SetupGet(x => x.Name).Returns("mob1");
            characterMock.SetupGet(x => x.Keywords).Returns(new[] { "mob1" });
            characterMock.SetupGet(x => x.Level).Returns(100);
            characterMock.SetupGet(x => x.Equipments).Returns(new[] { itemMock.Object });
            characterMock.SetupGet(x => x.KnownAbilities).Returns(new[] { new KnownAbility { Ability = abilityManager["Bless"], Learned = 1, Level = 1 } });
            var roomMock = new Mock<IRoom>();
            roomMock.SetupGet(x => x.People).Returns(new[] { characterMock.Object });
            characterMock.SetupGet(x => x.Room).Returns(roomMock.Object);

            (string rawParameters, CommandParameter[] parameters) args = BuildParametersSkipFirst("Bless mob1");
            AbilityTargetResults result = abilityManager.GetAbilityTarget(abilityManager["Bless"], characterMock.Object, out IEntity target, args.rawParameters, args.parameters);

            Assert.AreEqual(AbilityTargetResults.Ok, result);
            Assert.AreSame(characterMock.Object, target);
        }

        // AbilityTargets.Custom
        [TestMethod]
        public void TargetCustom_NoParameterSpecified_Test()
        {
            var randomManagerMock = new Mock<IRandomManager>();
            randomManagerMock
                .Setup(x => x.Chance(It.IsAny<int>()))
                .Returns(true); // always succeed
            IAbilityManager abilityManager = new AbilityManager(randomManagerMock.Object);
            var itemMock = new Mock<IItem>();
            itemMock.SetupGet(x => x.Name).Returns("item1");
            itemMock.SetupGet(x => x.Keywords).Returns(new[] { "item1" });
            var characterMock = new Mock<ICharacter>();
            characterMock.SetupGet(x => x.Level).Returns(100);
            characterMock.SetupGet(x => x.Inventory).Returns(new[] { itemMock.Object });
            characterMock.SetupGet(x => x.KnownAbilities).Returns(new[] { new KnownAbility { Ability = abilityManager["Locate Object"], Learned = 1, Level = 1 } });

            (string rawParameters, CommandParameter[] parameters) args = BuildParametersSkipFirst("'Locate Object'");
            AbilityTargetResults result = abilityManager.GetAbilityTarget(abilityManager["Locate Object"], characterMock.Object, out IEntity target, args.rawParameters, args.parameters);

            Assert.AreEqual(AbilityTargetResults.Ok, result);
            Assert.IsNull(target);
        }

        [TestMethod]
        public void TargetCustom_ParameterSpecified_Test()
        {
            var randomManagerMock = new Mock<IRandomManager>();
            randomManagerMock
                .Setup(x => x.Chance(It.IsAny<int>()))
                .Returns(true); // always succeed
            IAbilityManager abilityManager = new AbilityManager(randomManagerMock.Object);
            var itemMock = new Mock<IItem>();
            itemMock.SetupGet(x => x.Name).Returns("item1");
            itemMock.SetupGet(x => x.Keywords).Returns(new[] { "item1" });
            var characterMock = new Mock<ICharacter>();
            characterMock.SetupGet(x => x.Level).Returns(100);
            characterMock.SetupGet(x => x.Inventory).Returns(new[] { itemMock.Object });
            characterMock.SetupGet(x => x.KnownAbilities).Returns(new[] { new KnownAbility { Ability = abilityManager["Locate Object"], Learned = 1, Level = 1 } });

            (string rawParameters, CommandParameter[] parameters) args = BuildParametersSkipFirst("'locate object' item2");
            AbilityTargetResults result = abilityManager.GetAbilityTarget(abilityManager["Locate Object"], characterMock.Object, out IEntity target, args.rawParameters, args.parameters);

            Assert.AreEqual(AbilityTargetResults.Ok, result);
            Assert.IsNull(target);
        }

        // AbilityTargets.OptionalItemInventory
        [TestMethod]
        public void TargetOptionalItemInventory_NoTargetSpecified_Test()
        {
            var randomManagerMock = new Mock<IRandomManager>();
            randomManagerMock
                .Setup(x => x.Chance(It.IsAny<int>()))
                .Returns(true); // always succeed
            IAbilityManager abilityManager = new AbilityManager(randomManagerMock.Object);
            var itemMock = new Mock<IItem>();
            itemMock.SetupGet(x => x.Name).Returns("item1");
            itemMock.SetupGet(x => x.Keywords).Returns(new[] { "item1" });
            var characterMock = new Mock<ICharacter>();
            characterMock.SetupGet(x => x.Level).Returns(100);
            characterMock.SetupGet(x => x.Inventory).Returns(new[] { itemMock.Object });
            characterMock.SetupGet(x => x.KnownAbilities).Returns(new[] { new KnownAbility { Ability = abilityManager["Continual Light"], Learned = 1, Level = 1 } });

            (string rawParameters, CommandParameter[] parameters) args = BuildParametersSkipFirst("'continual light'");
            AbilityTargetResults result = abilityManager.GetAbilityTarget(abilityManager["continual light"], characterMock.Object, out IEntity target, args.rawParameters, args.parameters);

            Assert.AreEqual(AbilityTargetResults.Ok, result);
            Assert.IsNull(target);
        }

        [TestMethod]
        public void TargetOptionalItemInventory_TargetNotFoundSpecified_Test()
        {
            var randomManagerMock = new Mock<IRandomManager>();
            randomManagerMock
                .Setup(x => x.Chance(It.IsAny<int>()))
                .Returns(true); // always succeed
            IAbilityManager abilityManager = new AbilityManager(randomManagerMock.Object);
            var itemMock = new Mock<IItem>();
            itemMock.SetupGet(x => x.Name).Returns("item1");
            itemMock.SetupGet(x => x.Keywords).Returns(new[] { "item1" });
            var characterMock = new Mock<ICharacter>();
            characterMock.SetupGet(x => x.Name).Returns("mob1");
            characterMock.SetupGet(x => x.Keywords).Returns(new[] { "mob1" });
            characterMock.SetupGet(x => x.Level).Returns(100);
            characterMock.SetupGet(x => x.Inventory).Returns(new[] { itemMock.Object });
            characterMock.SetupGet(x => x.KnownAbilities).Returns(new[] { new KnownAbility { Ability = abilityManager["Continual Light"], Learned = 1, Level = 1 } });

            (string rawParameters, CommandParameter[] parameters) args = BuildParametersSkipFirst("'continual light' item2");
            AbilityTargetResults result = abilityManager.GetAbilityTarget(abilityManager["continual light"], characterMock.Object, out IEntity target, args.rawParameters, args.parameters);

            Assert.AreEqual(AbilityTargetResults.TargetNotFound, result);
            Assert.IsNull(target);
        }

        [TestMethod]
        public void TargetOptionalItemInventory_InvalidTargetSpecified_Test()
        {
            var randomManagerMock = new Mock<IRandomManager>();
            randomManagerMock
                .Setup(x => x.Chance(It.IsAny<int>()))
                .Returns(true); // always succeed
            IAbilityManager abilityManager = new AbilityManager(randomManagerMock.Object);
            var itemMock = new Mock<IItem>();
            itemMock.SetupGet(x => x.Name).Returns("item1");
            itemMock.SetupGet(x => x.Keywords).Returns(new[] { "item1" });
            var characterMock = new Mock<ICharacter>();
            characterMock.SetupGet(x => x.Name).Returns("mob1");
            characterMock.SetupGet(x => x.Keywords).Returns(new[] { "mob1" });
            characterMock.SetupGet(x => x.Level).Returns(100);
            characterMock.SetupGet(x => x.Inventory).Returns(new[] { itemMock.Object });
            characterMock.SetupGet(x => x.KnownAbilities).Returns(new[] { new KnownAbility { Ability = abilityManager["Continual Light"], Learned = 1, Level = 1 } });

            (string rawParameters, CommandParameter[] parameters) args = BuildParametersSkipFirst("'continual light' mob1");
            AbilityTargetResults result = abilityManager.GetAbilityTarget(abilityManager["continual light"], characterMock.Object, out IEntity target, args.rawParameters, args.parameters);

            Assert.AreEqual(AbilityTargetResults.TargetNotFound, result);
            Assert.IsNull(target);
        }

        [TestMethod]
        public void TargetOptionalItemInventory_TargetSpecified_Test()
        {
            var randomManagerMock = new Mock<IRandomManager>();
            randomManagerMock
                .Setup(x => x.Chance(It.IsAny<int>()))
                .Returns(true); // always succeed
            IAbilityManager abilityManager = new AbilityManager(randomManagerMock.Object);
            var itemMock = new Mock<IItem>();
            itemMock.SetupGet(x => x.Name).Returns("item1");
            itemMock.SetupGet(x => x.Keywords).Returns(new[] { "item1" });
            var characterMock = new Mock<ICharacter>();
            characterMock.SetupGet(x => x.Level).Returns(100);
            characterMock.SetupGet(x => x.Inventory).Returns(new[] { itemMock.Object });
            characterMock.SetupGet(x => x.KnownAbilities).Returns(new[] { new KnownAbility { Ability = abilityManager["Continual Light"], Learned = 1, Level = 1 } });

            (string rawParameters, CommandParameter[] parameters) args = BuildParametersSkipFirst("'continual light' item1");
            AbilityTargetResults result = abilityManager.GetAbilityTarget(abilityManager["continual light"], characterMock.Object, out IEntity target, args.rawParameters, args.parameters);

            Assert.AreEqual(AbilityTargetResults.Ok, result);
            Assert.AreSame(itemMock.Object, target);
        }

        [TestMethod]
        public void TargetOptionalItemInventory_TargetInRoomSpecified_Test()
        {
            var randomManagerMock = new Mock<IRandomManager>();
            randomManagerMock
                .Setup(x => x.Chance(It.IsAny<int>()))
                .Returns(true); // always succeed
            IAbilityManager abilityManager = new AbilityManager(randomManagerMock.Object);
            var itemMock = new Mock<IItem>();
            itemMock.SetupGet(x => x.Name).Returns("item1");
            itemMock.SetupGet(x => x.Keywords).Returns(new[] { "item1" });
            var characterMock = new Mock<ICharacter>();
            characterMock.SetupGet(x => x.Level).Returns(100);
            characterMock.SetupGet(x => x.Inventory).Returns(new IItem[] { });
            characterMock.SetupGet(x => x.KnownAbilities).Returns(new[] { new KnownAbility { Ability = abilityManager["Continual Light"], Learned = 1, Level = 1 } });
            var roomMock = new Mock<IRoom>();
            roomMock.SetupGet(x => x.Content).Returns(new[] { itemMock.Object });

            (string rawParameters, CommandParameter[] parameters) args = BuildParametersSkipFirst("'continual light' item1");
            AbilityTargetResults result = abilityManager.GetAbilityTarget(abilityManager["continual light"], characterMock.Object, out IEntity target, args.rawParameters, args.parameters);

            Assert.AreEqual(AbilityTargetResults.TargetNotFound, result);
            Assert.IsNull(target);
        }

        // AbilityTargets.ArmorInventory
        [TestMethod]
        public void TargetArmorInventory_NoTargetSpecified_Test()
        {
            var randomManagerMock = new Mock<IRandomManager>();
            randomManagerMock
                .Setup(x => x.Chance(It.IsAny<int>()))
                .Returns(true); // always succeed
            IAbilityManager abilityManager = new AbilityManager(randomManagerMock.Object);
            var itemMock = new Mock<IItemArmor>();
            itemMock.SetupGet(x => x.Name).Returns("item1");
            itemMock.SetupGet(x => x.Keywords).Returns(new[] { "item1" });
            var characterMock = new Mock<ICharacter>();
            characterMock.SetupGet(x => x.Level).Returns(100);
            characterMock.SetupGet(x => x.Inventory).Returns(new IItem[] { itemMock.Object });
            characterMock.SetupGet(x => x.KnownAbilities).Returns(new[] { new KnownAbility { Ability = abilityManager["Enchant Armor"], Learned = 1, Level = 1 } });

            (string rawParameters, CommandParameter[] parameters) args = BuildParametersSkipFirst("'enchant armor'");
            AbilityTargetResults result = abilityManager.GetAbilityTarget(abilityManager["enchant armor"], characterMock.Object, out IEntity target, args.rawParameters, args.parameters);

            Assert.AreEqual(AbilityTargetResults.MissingParameter, result);
            Assert.IsNull(target);
        }

        [TestMethod]
        public void TargetArmorInventory_TargetNotFoundSpecified_Test()
        {
            var randomManagerMock = new Mock<IRandomManager>();
            randomManagerMock
                .Setup(x => x.Chance(It.IsAny<int>()))
                .Returns(true); // always succeed
            IAbilityManager abilityManager = new AbilityManager(randomManagerMock.Object);
            var itemMock = new Mock<IItemArmor>();
            itemMock.SetupGet(x => x.Name).Returns("item1");
            itemMock.SetupGet(x => x.Keywords).Returns(new[] { "item1" });
            var characterMock = new Mock<ICharacter>();
            characterMock.SetupGet(x => x.Name).Returns("mob1");
            characterMock.SetupGet(x => x.Keywords).Returns(new[] { "mob1" });
            characterMock.SetupGet(x => x.Level).Returns(100);
            characterMock.SetupGet(x => x.Inventory).Returns(new IItem[] { itemMock.Object });
            characterMock.SetupGet(x => x.KnownAbilities).Returns(new[] { new KnownAbility { Ability = abilityManager["Enchant Armor"], Learned = 1, Level = 1 } });

            (string rawParameters, CommandParameter[] parameters) args = BuildParametersSkipFirst("'enchant armor' item2");
            AbilityTargetResults result = abilityManager.GetAbilityTarget(abilityManager["enchant armor"], characterMock.Object, out IEntity target, args.rawParameters, args.parameters);

            Assert.AreEqual(AbilityTargetResults.TargetNotFound, result);
            Assert.IsNull(target);
        }

        [TestMethod]
        public void TargetArmorInventory_InvalidTargetSpecified_Test()
        {
            var randomManagerMock = new Mock<IRandomManager>();
            randomManagerMock
                .Setup(x => x.Chance(It.IsAny<int>()))
                .Returns(true); // always succeed
            IAbilityManager abilityManager = new AbilityManager(randomManagerMock.Object);
            var itemMock = new Mock<IItemArmor>();
            itemMock.SetupGet(x => x.Name).Returns("item1");
            itemMock.SetupGet(x => x.Keywords).Returns(new[] { "item1" });
            var characterMock = new Mock<ICharacter>();
            characterMock.SetupGet(x => x.Name).Returns("mob1");
            characterMock.SetupGet(x => x.Keywords).Returns(new[] { "mob1" });
            characterMock.SetupGet(x => x.Level).Returns(100);
            characterMock.SetupGet(x => x.Inventory).Returns(new IItem[] { itemMock.Object });
            characterMock.SetupGet(x => x.KnownAbilities).Returns(new[] { new KnownAbility { Ability = abilityManager["Enchant Armor"], Learned = 1, Level = 1 } });

            (string rawParameters, CommandParameter[] parameters) args = BuildParametersSkipFirst("'enchant armor' mob1");
            AbilityTargetResults result = abilityManager.GetAbilityTarget(abilityManager["enchant armor"], characterMock.Object, out IEntity target, args.rawParameters, args.parameters);

            Assert.AreEqual(AbilityTargetResults.TargetNotFound, result);
            Assert.IsNull(target);
        }

        [TestMethod]
        public void TargetArmorInventory_TargetNonArmorSpecified_Test()
        {
            var randomManagerMock = new Mock<IRandomManager>();
            randomManagerMock
                .Setup(x => x.Chance(It.IsAny<int>()))
                .Returns(true); // always succeed
            IAbilityManager abilityManager = new AbilityManager(randomManagerMock.Object);
            var itemMock = new Mock<IItem>();
            itemMock.SetupGet(x => x.Name).Returns("item1");
            itemMock.SetupGet(x => x.Keywords).Returns(new[] { "item1" });
            var characterMock = new Mock<ICharacter>();
            characterMock.SetupGet(x => x.Level).Returns(100);
            characterMock.SetupGet(x => x.Inventory).Returns(new[] { itemMock.Object });
            characterMock.SetupGet(x => x.KnownAbilities).Returns(new[] { new KnownAbility { Ability = abilityManager["Enchant Armor"], Learned = 1, Level = 1 } });

            (string rawParameters, CommandParameter[] parameters) args = BuildParametersSkipFirst("'enchant armor' item1");
            AbilityTargetResults result = abilityManager.GetAbilityTarget(abilityManager["enchant armor"], characterMock.Object, out IEntity target, args.rawParameters, args.parameters);

            Assert.AreEqual(AbilityTargetResults.InvalidTarget, result);
            Assert.IsNull(target);
        }

        [TestMethod]
        public void TargetArmorInventory_TargetInRoomSpecified_Test()
        {
            var randomManagerMock = new Mock<IRandomManager>();
            randomManagerMock
                .Setup(x => x.Chance(It.IsAny<int>()))
                .Returns(true); // always succeed
            IAbilityManager abilityManager = new AbilityManager(randomManagerMock.Object);
            var itemMock = new Mock<IItemArmor>();
            itemMock.SetupGet(x => x.Name).Returns("item1");
            itemMock.SetupGet(x => x.Keywords).Returns(new[] { "item1" });
            var characterMock = new Mock<ICharacter>();
            characterMock.SetupGet(x => x.Level).Returns(100);
            characterMock.SetupGet(x => x.Inventory).Returns(new IItem[] { });
            characterMock.SetupGet(x => x.KnownAbilities).Returns(new[] { new KnownAbility { Ability = abilityManager["Enchant Armor"], Learned = 1, Level = 1 } });
            var roomMock = new Mock<IRoom>();
            roomMock.SetupGet(x => x.Content).Returns(new IItem[] { itemMock.Object });

            (string rawParameters, CommandParameter[] parameters) args = BuildParametersSkipFirst("'enchant armor' item1");
            AbilityTargetResults result = abilityManager.GetAbilityTarget(abilityManager["enchant armor"], characterMock.Object, out IEntity target, args.rawParameters, args.parameters);

            Assert.AreEqual(AbilityTargetResults.TargetNotFound, result);
            Assert.IsNull(target);
        }

        [TestMethod]
        public void TargetArmorInventory_TargetArmorSpecified_Test()
        {
            var randomManagerMock = new Mock<IRandomManager>();
            randomManagerMock
                .Setup(x => x.Chance(It.IsAny<int>()))
                .Returns(true); // always succeed
            IAbilityManager abilityManager = new AbilityManager(randomManagerMock.Object);
            var itemMock = new Mock<IItemArmor>();
            itemMock.SetupGet(x => x.Name).Returns("item1");
            itemMock.SetupGet(x => x.Keywords).Returns(new[] { "item1" });
            var characterMock = new Mock<ICharacter>();
            characterMock.SetupGet(x => x.Level).Returns(100);
            characterMock.SetupGet(x => x.Inventory).Returns(new IItem[] { itemMock.Object });
            characterMock.SetupGet(x => x.KnownAbilities).Returns(new[] { new KnownAbility { Ability = abilityManager["Enchant Armor"], Learned = 1, Level = 1 } });

            (string rawParameters, CommandParameter[] parameters) args = BuildParametersSkipFirst("'enchant armor' item1");
            AbilityTargetResults result = abilityManager.GetAbilityTarget(abilityManager["enchant armor"], characterMock.Object, out IEntity target, args.rawParameters, args.parameters);

            Assert.AreEqual(AbilityTargetResults.Ok, result);
            Assert.AreSame(itemMock.Object, target);
        }

        // AbilityTargets.WeaponInventory
        [TestMethod]
        public void TargetWeaponInventory_NoTargetSpecified_Test()
        {
            var randomManagerMock = new Mock<IRandomManager>();
            randomManagerMock
                .Setup(x => x.Chance(It.IsAny<int>()))
                .Returns(true); // always succeed
            IAbilityManager abilityManager = new AbilityManager(randomManagerMock.Object);
            var itemMock = new Mock<IItemWeapon>();
            itemMock.SetupGet(x => x.Name).Returns("item1");
            itemMock.SetupGet(x => x.Keywords).Returns(new[] { "item1" });
            var characterMock = new Mock<ICharacter>();
            characterMock.SetupGet(x => x.Level).Returns(100);
            characterMock.SetupGet(x => x.Inventory).Returns(new IItem[] { itemMock.Object });
            characterMock.SetupGet(x => x.KnownAbilities).Returns(new[] { new KnownAbility { Ability = abilityManager["Enchant Weapon"], Learned = 1, Level = 1 } });

            (string rawParameters, CommandParameter[] parameters) args = BuildParametersSkipFirst("'enchant Weapon'");
            AbilityTargetResults result = abilityManager.GetAbilityTarget(abilityManager["enchant weapon"], characterMock.Object, out IEntity target, args.rawParameters, args.parameters);

            Assert.AreEqual(AbilityTargetResults.MissingParameter, result);
            Assert.IsNull(target);
        }

        [TestMethod]
        public void TargetWeaponInventory_TargetNotFoundSpecified_Test()
        {
            var randomManagerMock = new Mock<IRandomManager>();
            randomManagerMock
                .Setup(x => x.Chance(It.IsAny<int>()))
                .Returns(true); // always succeed
            IAbilityManager abilityManager = new AbilityManager(randomManagerMock.Object);
            var itemMock = new Mock<IItemWeapon>();
            itemMock.SetupGet(x => x.Name).Returns("item1");
            itemMock.SetupGet(x => x.Keywords).Returns(new[] { "item1" });
            var characterMock = new Mock<ICharacter>();
            characterMock.SetupGet(x => x.Name).Returns("mob1");
            characterMock.SetupGet(x => x.Keywords).Returns(new[] { "mob1" });
            characterMock.SetupGet(x => x.Level).Returns(100);
            characterMock.SetupGet(x => x.Inventory).Returns(new IItem[] { itemMock.Object });
            characterMock.SetupGet(x => x.KnownAbilities).Returns(new[] { new KnownAbility { Ability = abilityManager["Enchant Weapon"], Learned = 1, Level = 1 } });

            (string rawParameters, CommandParameter[] parameters) args = BuildParametersSkipFirst("'enchant Weapon' item2");
            AbilityTargetResults result = abilityManager.GetAbilityTarget(abilityManager["enchant weapon"], characterMock.Object, out IEntity target, args.rawParameters, args.parameters);

            Assert.AreEqual(AbilityTargetResults.TargetNotFound, result);
            Assert.IsNull(target);
        }

        [TestMethod]
        public void TargetWeaponInventory_InvalidTargetSpecified_Test()
        {
            var randomManagerMock = new Mock<IRandomManager>();
            randomManagerMock
                .Setup(x => x.Chance(It.IsAny<int>()))
                .Returns(true); // always succeed
            IAbilityManager abilityManager = new AbilityManager(randomManagerMock.Object);
            var itemMock = new Mock<IItemWeapon>();
            itemMock.SetupGet(x => x.Name).Returns("item1");
            itemMock.SetupGet(x => x.Keywords).Returns(new[] { "item1" });
            var characterMock = new Mock<ICharacter>();
            characterMock.SetupGet(x => x.Name).Returns("mob1");
            characterMock.SetupGet(x => x.Keywords).Returns(new[] { "mob1" });
            characterMock.SetupGet(x => x.Level).Returns(100);
            characterMock.SetupGet(x => x.Inventory).Returns(new IItem[] { itemMock.Object });
            characterMock.SetupGet(x => x.KnownAbilities).Returns(new[] { new KnownAbility { Ability = abilityManager["Enchant Weapon"], Learned = 1, Level = 1 } });

            (string rawParameters, CommandParameter[] parameters) args = BuildParametersSkipFirst("'enchant Weapon' mob1");
            AbilityTargetResults result = abilityManager.GetAbilityTarget(abilityManager["enchant weapon"], characterMock.Object, out IEntity target, args.rawParameters, args.parameters);

            Assert.AreEqual(AbilityTargetResults.TargetNotFound, result);
            Assert.IsNull(target);
        }

        [TestMethod]
        public void TargetWeaponInventory_TargetNonWeaponSpecified_Test()
        {
            var randomManagerMock = new Mock<IRandomManager>();
            randomManagerMock
                .Setup(x => x.Chance(It.IsAny<int>()))
                .Returns(true); // always succeed
            IAbilityManager abilityManager = new AbilityManager(randomManagerMock.Object);
            var itemMock = new Mock<IItem>();
            itemMock.SetupGet(x => x.Name).Returns("item1");
            itemMock.SetupGet(x => x.Keywords).Returns(new[] { "item1" });
            var characterMock = new Mock<ICharacter>();
            characterMock.SetupGet(x => x.Level).Returns(100);
            characterMock.SetupGet(x => x.Inventory).Returns(new[] { itemMock.Object });
            characterMock.SetupGet(x => x.KnownAbilities).Returns(new[] { new KnownAbility { Ability = abilityManager["Enchant Weapon"], Learned = 1, Level = 1 } });

            (string rawParameters, CommandParameter[] parameters) args = BuildParametersSkipFirst("'enchant Weapon' item1");
            AbilityTargetResults result = abilityManager.GetAbilityTarget(abilityManager["enchant weapon"], characterMock.Object, out IEntity target, args.rawParameters, args.parameters);

            Assert.AreEqual(AbilityTargetResults.InvalidTarget, result);
            Assert.IsNull(target);
        }

        [TestMethod]
        public void TargetWeaponInventory_TargetInRoomSpecified_Test()
        {
            var randomManagerMock = new Mock<IRandomManager>();
            randomManagerMock
                .Setup(x => x.Chance(It.IsAny<int>()))
                .Returns(true); // always succeed
            IAbilityManager abilityManager = new AbilityManager(randomManagerMock.Object);
            var itemMock = new Mock<IItemWeapon>();
            itemMock.SetupGet(x => x.Name).Returns("item1");
            itemMock.SetupGet(x => x.Keywords).Returns(new[] { "item1" });
            var characterMock = new Mock<ICharacter>();
            characterMock.SetupGet(x => x.Level).Returns(100);
            characterMock.SetupGet(x => x.Inventory).Returns(new IItem[] { });
            characterMock.SetupGet(x => x.KnownAbilities).Returns(new[] { new KnownAbility { Ability = abilityManager["Enchant Weapon"], Learned = 1, Level = 1 } });
            var roomMock = new Mock<IRoom>();
            roomMock.SetupGet(x => x.Content).Returns(new IItem[] { itemMock.Object });

            (string rawParameters, CommandParameter[] parameters) args = BuildParametersSkipFirst("'enchant Weapon' item1");
            AbilityTargetResults result = abilityManager.GetAbilityTarget(abilityManager["enchant weapon"], characterMock.Object, out IEntity target, args.rawParameters, args.parameters);

            Assert.AreEqual(AbilityTargetResults.TargetNotFound, result);
            Assert.IsNull(target);
        }

        [TestMethod]
        public void TargetWeaponInventory_TargetWeaponSpecified_Test()
        {
            var randomManagerMock = new Mock<IRandomManager>();
            randomManagerMock
                .Setup(x => x.Chance(It.IsAny<int>()))
                .Returns(true); // always succeed
            IAbilityManager abilityManager = new AbilityManager(randomManagerMock.Object);
            var itemMock = new Mock<IItemWeapon>();
            itemMock.SetupGet(x => x.Name).Returns("item1");
            itemMock.SetupGet(x => x.Keywords).Returns(new[] { "item1" });
            var characterMock = new Mock<ICharacter>();
            characterMock.SetupGet(x => x.Level).Returns(100);
            characterMock.SetupGet(x => x.Inventory).Returns(new IItem[] { itemMock.Object });
            characterMock.SetupGet(x => x.KnownAbilities).Returns(new[] { new KnownAbility { Ability = abilityManager["Enchant Weapon"], Learned = 1, Level = 1 } });

            (string rawParameters, CommandParameter[] parameters) args = BuildParametersSkipFirst("'enchant Weapon' item1");
            AbilityTargetResults result = abilityManager.GetAbilityTarget(abilityManager["enchant weapon"], characterMock.Object, out IEntity target, args.rawParameters, args.parameters);

            Assert.AreEqual(AbilityTargetResults.Ok, result);
            Assert.AreSame(itemMock.Object, target);
        }
    }
}
