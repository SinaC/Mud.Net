using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Mud.POC.Abilities;
using Mud.Server.Common;
using Mud.Server.Input;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Mud.POC.Tests
{
    [TestClass]
    public class AbilityTests
    {
        [TestMethod]
        public void AbilityManager_Ctor_Test()
        {
            var randomManagerMock = new Mock<IRandomManager>();
            IAbilityManager abilityManager = new AbilityManager(randomManagerMock.Object);

            Assert.AreEqual(typeof(Spells).GetMethods(BindingFlags.Static | BindingFlags.Public).Count(), abilityManager.Abilities.Count());
        }

        [TestMethod]
        public void AbilityManager_Cast_NoParam_Test()
        {
            var randomManagerMock = new Mock<IRandomManager>();
            IAbilityManager abilityManager = new AbilityManager(randomManagerMock.Object);
            var characterMock = new Mock<ICharacter>();

            CommandExecutionResults result = abilityManager.Cast(characterMock.Object, string.Empty);

            Assert.AreEqual(CommandExecutionResults.SyntaxErrorNoDisplay, result);
        }

        [TestMethod]
        public void AbilityManager_Cast_UnknownSpell_Test()
        {
            var randomManagerMock = new Mock<IRandomManager>();
            IAbilityManager abilityManager = new AbilityManager(randomManagerMock.Object);
            var characterMock = new Mock<ICharacter>();

            (string rawParameters, CommandParameter[] parameters) args = BuildParameters("pouet");
            CommandExecutionResults result = abilityManager.Cast(characterMock.Object, args.rawParameters, args.parameters);

            Assert.AreEqual(CommandExecutionResults.InvalidParameter, result);
        }

        [TestMethod]
        public void AbilityManager_Cast_SpellNotKnown_Test()
        {
            var randomManagerMock = new Mock<IRandomManager>();
            IAbilityManager abilityManager = new AbilityManager(randomManagerMock.Object);
            var characterMock = new Mock<ICharacter>();

            (string rawParameters, CommandParameter[] parameters) args = BuildParameters("Teleport");
            CommandExecutionResults result = abilityManager.Cast(characterMock.Object, args.rawParameters, args.parameters);

            Assert.AreEqual(CommandExecutionResults.InvalidParameter, result);
        }

        [TestMethod]
        public void AbilityManager_Cast_SpellNotYetLearned_Test()
        {
            var randomManagerMock = new Mock<IRandomManager>();
            IAbilityManager abilityManager = new AbilityManager(randomManagerMock.Object);
            var characterMock = new Mock<ICharacter>();
            characterMock.SetupGet(x => x.Level).Returns(100);
            characterMock.SetupGet(x => x.KnownAbilities).Returns(new[] { new KnownAbility { Ability = abilityManager["Teleport"], Learned = 0, Level = 1 } });

            (string rawParameters, CommandParameter[] parameters) args = BuildParameters("Teleport");
            CommandExecutionResults result = abilityManager.Cast(characterMock.Object, args.rawParameters, args.parameters);

            Assert.AreEqual(CommandExecutionResults.InvalidParameter, result);
        }

        [TestMethod]
        public void AbilityManager_Cast_TooLowLevel_Test()
        {
            var randomManagerMock = new Mock<IRandomManager>();
            IAbilityManager abilityManager = new AbilityManager(randomManagerMock.Object);
            var characterMock = new Mock<ICharacter>();
            characterMock.SetupGet(x => x.Level).Returns(5);
            characterMock.SetupGet(x => x.KnownAbilities).Returns(new[] { new KnownAbility { Ability = abilityManager["Teleport"], Learned = 1, Level = 20 } });

            (string rawParameters, CommandParameter[] parameters) args = BuildParameters("Teleport");
            CommandExecutionResults result = abilityManager.Cast(characterMock.Object, args.rawParameters, args.parameters);

            Assert.AreEqual(CommandExecutionResults.InvalidParameter, result);
        }

        [TestMethod]
        public void AbilityManager_Cast_Failed_Test()
        {
            var randomManagerMock = new Mock<IRandomManager>();
            randomManagerMock
                .Setup(x => x.Chance(It.IsAny<int>()))
                .Returns(false); // always fails
            IAbilityManager abilityManager = new AbilityManager(randomManagerMock.Object);
            var characterMock = new Mock<ICharacter>();
            characterMock.SetupGet(x => x.Level).Returns(100);
            characterMock.SetupGet(x => x.KnownAbilities).Returns(new[] { new KnownAbility { Ability = abilityManager["Teleport"], Learned = 1, Level = 1 } });

            (string rawParameters, CommandParameter[] parameters) args = BuildParameters("Teleport");
            CommandExecutionResults result = abilityManager.Cast(characterMock.Object, args.rawParameters, args.parameters);

            Assert.AreEqual(CommandExecutionResults.NoExecution, result);
        }

        // AbilityTargets.None
        [TestMethod]
        public void AbilityManager_Cast_TargetNone_NoTargetSpecified_Test()
        {
            var randomManagerMock = new Mock<IRandomManager>();
            randomManagerMock
                .Setup(x => x.Chance(It.IsAny<int>()))
                .Returns(true); // always succeed
            IAbilityManager abilityManager = new AbilityManager(randomManagerMock.Object);
            var characterMock = new Mock<ICharacter>();
            characterMock.SetupGet(x => x.Level).Returns(100);
            characterMock.SetupGet(x => x.KnownAbilities).Returns(new[] { new KnownAbility { Ability = abilityManager["Teleport"], Learned = 1, Level = 1} });

            (string rawParameters, CommandParameter[] parameters) args = BuildParameters("Teleport");
            CommandExecutionResults result = abilityManager.Cast(characterMock.Object, args.rawParameters, args.parameters);

            Assert.AreEqual(CommandExecutionResults.Ok, result);
        }

        [TestMethod]
        public void AbilityManager_Cast_TargetNone_AdditionalParameter_Test()
        {
            var randomManagerMock = new Mock<IRandomManager>();
            randomManagerMock
                .Setup(x => x.Chance(It.IsAny<int>()))
                .Returns(true); // always succeed
            IAbilityManager abilityManager = new AbilityManager(randomManagerMock.Object);
            var characterMock = new Mock<ICharacter>();
            characterMock.SetupGet(x => x.Level).Returns(100);
            characterMock.SetupGet(x => x.KnownAbilities).Returns(new[] { new KnownAbility { Ability = abilityManager["Teleport"], Learned = 1, Level = 1 } });

            (string rawParameters, CommandParameter[] parameters) args = BuildParameters("Teleport should not be specified");
            CommandExecutionResults result = abilityManager.Cast(characterMock.Object, args.rawParameters, args.parameters);

            Assert.AreEqual(CommandExecutionResults.Ok, result);
        }

        // AbilityTargets.CharacterOffensive
        [TestMethod]
        public void AbilityManager_Cast_TargetCharacterOffensive_NoTargetSpecified_NoFighting_Test()
        {
            var randomManagerMock = new Mock<IRandomManager>();
            randomManagerMock
                .Setup(x => x.Chance(It.IsAny<int>()))
                .Returns(true); // always succeed
            IAbilityManager abilityManager = new AbilityManager(randomManagerMock.Object);
            var characterMock = new Mock<ICharacter>();
            characterMock.SetupGet(x => x.Level).Returns(100);
            characterMock.SetupGet(x => x.KnownAbilities).Returns(new[] { new KnownAbility { Ability = abilityManager["Acid Blast"], Learned = 1, Level = 1 } });

            (string rawParameters, CommandParameter[] parameters) args = BuildParameters("'Acid Blast'");
            CommandExecutionResults result = abilityManager.Cast(characterMock.Object, args.rawParameters, args.parameters);

            Assert.AreEqual(CommandExecutionResults.SyntaxErrorNoDisplay, result);
        }

        [TestMethod]
        public void AbilityManager_Cast_TargetCharacterOffensive_NoTargetSpecified_Fighting_Test()
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

            (string rawParameters, CommandParameter[] parameters) args = BuildParameters("'Acid Blast'");
            CommandExecutionResults result = abilityManager.Cast(mob1Mock.Object, args.rawParameters, args.parameters);

            Assert.AreEqual(CommandExecutionResults.Ok, result);
        }

        [TestMethod]
        public void AbilityManager_Cast_TargetCharacterOffensive_InvalidTargetSpecified_Test()
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

            (string rawParameters, CommandParameter[] parameters) args = BuildParameters("'Acid Blast' mob2");
            CommandExecutionResults result = abilityManager.Cast(characterMock.Object, args.rawParameters, args.parameters);

            Assert.AreEqual(CommandExecutionResults.TargetNotFound, result);
        }

        [TestMethod]
        public void AbilityManager_Cast_TargetCharacterOffensive_CorrectTarget_Test()
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

            (string rawParameters, CommandParameter[] parameters) args = BuildParameters("'Acid Blast' mob2");
            CommandExecutionResults result = abilityManager.Cast(mob1Mock.Object, args.rawParameters, args.parameters);

            Assert.AreEqual(CommandExecutionResults.Ok, result);
        }

        // AbilityTargets.CharacterDefensive
        [TestMethod]
        public void AbilityManager_Cast_TargetCharacterDefensive_NoTargetSpecified_Test()
        {
            var randomManagerMock = new Mock<IRandomManager>();
            randomManagerMock
                .Setup(x => x.Chance(It.IsAny<int>()))
                .Returns(true); // always succeed
            IAbilityManager abilityManager = new AbilityManager(randomManagerMock.Object);
            var characterMock = new Mock<ICharacter>();
            characterMock.SetupGet(x => x.Level).Returns(100);
            characterMock.SetupGet(x => x.KnownAbilities).Returns(new[] { new KnownAbility { Ability = abilityManager["Armor"], Learned = 1, Level = 1 } });

            (string rawParameters, CommandParameter[] parameters) args = BuildParameters("Armor");
            CommandExecutionResults result = abilityManager.Cast(characterMock.Object, args.rawParameters, args.parameters);

            Assert.AreEqual(CommandExecutionResults.Ok, result);
        }

        [TestMethod]
        public void AbilityManager_Cast_TargetCharacterDefensive_TargetSelfSpecified_Test()
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

            (string rawParameters, CommandParameter[] parameters) args = BuildParameters("Armor mob1");
            CommandExecutionResults result = abilityManager.Cast(characterMock.Object, args.rawParameters, args.parameters);

            Assert.AreEqual(CommandExecutionResults.Ok, result);
        }

        [TestMethod]
        public void AbilityManager_Cast_TargetCharacterDefensive_TargetSpecified_Test()
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

            (string rawParameters, CommandParameter[] parameters) args = BuildParameters("Armor mob2");
            CommandExecutionResults result = abilityManager.Cast(mob1Mock.Object, args.rawParameters, args.parameters);

            Assert.AreEqual(CommandExecutionResults.Ok, result);
        }

        // AbilityTargets.CharacterSelf
        [TestMethod]
        public void AbilityManager_Cast_TargetCharacterSelf_NoTargetSpecified_Test()
        {
            var randomManagerMock = new Mock<IRandomManager>();
            randomManagerMock
                .Setup(x => x.Chance(It.IsAny<int>()))
                .Returns(true); // always succeed
            IAbilityManager abilityManager = new AbilityManager(randomManagerMock.Object);
            var characterMock = new Mock<ICharacter>();
            characterMock.SetupGet(x => x.Level).Returns(100);
            characterMock.SetupGet(x => x.KnownAbilities).Returns(new[] { new KnownAbility { Ability = abilityManager["Detect Evil"], Learned = 1, Level = 1 } });

            (string rawParameters, CommandParameter[] parameters) args = BuildParameters("'detect evil'");
            CommandExecutionResults result = abilityManager.Cast(characterMock.Object, args.rawParameters, args.parameters);

            Assert.AreEqual(CommandExecutionResults.Ok, result);
        }

        [TestMethod]
        public void AbilityManager_Cast_TargetCharacterSelf_TargetSelfSpecified_Test()
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

            (string rawParameters, CommandParameter[] parameters) args = BuildParameters("'detect evil' mob1");
            CommandExecutionResults result = abilityManager.Cast(characterMock.Object, args.rawParameters, args.parameters);

            Assert.AreEqual(CommandExecutionResults.Ok, result);
        }

        [TestMethod]
        public void AbilityManager_Cast_TargetCharacterSelf_TargetNotFoundSpecified_Test()
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

            (string rawParameters, CommandParameter[] parameters) args = BuildParameters("'detect evil' mob3");
            CommandExecutionResults result = abilityManager.Cast(mob1Mock.Object, args.rawParameters, args.parameters);

            Assert.AreEqual(CommandExecutionResults.InvalidTarget, result);
        }

        [TestMethod]
        public void AbilityManager_Cast_TargetCharacterSelf_TargetSpecified_Test()
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

            (string rawParameters, CommandParameter[] parameters) args = BuildParameters("'detect evil' mob2");
            CommandExecutionResults result = abilityManager.Cast(mob1Mock.Object, args.rawParameters, args.parameters);

            Assert.AreEqual(CommandExecutionResults.InvalidTarget, result);
        }

        // AbilityTargets.ItemInventory
        [TestMethod]
        public void AbilityManager_Cast_TargetItemInventory_NoTargetSpecified_Test()
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
            characterMock.SetupGet(x => x.Inventory).Returns(new IItem[] { itemMock.Object });
            characterMock.SetupGet(x => x.KnownAbilities).Returns(new[] { new KnownAbility { Ability = abilityManager["Identify"], Learned = 1, Level = 1 } });

            (string rawParameters, CommandParameter[] parameters) args = BuildParameters("Identify");
            CommandExecutionResults result = abilityManager.Cast(characterMock.Object, args.rawParameters, args.parameters);

            Assert.AreEqual(CommandExecutionResults.SyntaxErrorNoDisplay, result);
        }

        [TestMethod]
        public void AbilityManager_Cast_TargetItemInventory_TargetNotFoundSpecified_Test()
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
            characterMock.SetupGet(x => x.Inventory).Returns(new IItem[] { itemMock.Object });
            characterMock.SetupGet(x => x.KnownAbilities).Returns(new[] { new KnownAbility { Ability = abilityManager["Identify"], Learned = 1, Level = 1 } });

            (string rawParameters, CommandParameter[] parameters) args = BuildParameters("Identify mob2");
            CommandExecutionResults result = abilityManager.Cast(characterMock.Object, args.rawParameters, args.parameters);

            Assert.AreEqual(CommandExecutionResults.TargetNotFound, result);
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
            characterMock.SetupGet(x => x.Inventory).Returns(new IItem[] { itemMock.Object });
            characterMock.SetupGet(x => x.KnownAbilities).Returns(new[] { new KnownAbility { Ability = abilityManager["Identify"], Learned = 1, Level = 1 } });

            (string rawParameters, CommandParameter[] parameters) args = BuildParameters("Identify mob1");
            CommandExecutionResults result = abilityManager.Cast(characterMock.Object, args.rawParameters, args.parameters);

            Assert.AreEqual(CommandExecutionResults.TargetNotFound, result);
        }

        [TestMethod]
        public void AbilityManager_Cast_TargetItemInventory_TargetSpecified_Test()
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
            characterMock.SetupGet(x => x.Inventory).Returns(new IItem[] { itemMock.Object });
            characterMock.SetupGet(x => x.KnownAbilities).Returns(new[] { new KnownAbility { Ability = abilityManager["Identify"], Learned = 1, Level = 1 } });

            (string rawParameters, CommandParameter[] parameters) args = BuildParameters("Identify item1");
            CommandExecutionResults result = abilityManager.Cast(characterMock.Object, args.rawParameters, args.parameters);

            Assert.AreEqual(CommandExecutionResults.Ok, result);
        }

        [TestMethod]
        public void AbilityManager_Cast_TargetItemInventory_TargetInRoomSpecified_Test()
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
            roomMock.SetupGet(x => x.Content).Returns(new IItem[] { itemMock.Object });

            (string rawParameters, CommandParameter[] parameters) args = BuildParameters("Identify item1");
            CommandExecutionResults result = abilityManager.Cast(characterMock.Object, args.rawParameters, args.parameters);

            Assert.AreEqual(CommandExecutionResults.TargetNotFound, result);
        }

        //  AbilityTargets.ItemHereOrCharacterOffensive
        [TestMethod]
        public void AbilityManager_Cast_TargetItemHereOrCharacterOffensive_NoTargetSpecified_Test() 
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
            characterMock.SetupGet(x => x.Inventory).Returns(new IItem[] { itemMock.Object });
            characterMock.SetupGet(x => x.KnownAbilities).Returns(new[] { new KnownAbility { Ability = abilityManager["Curse"], Learned = 1, Level = 1 } });

            (string rawParameters, CommandParameter[] parameters) args = BuildParameters("Curse");
            CommandExecutionResults result = abilityManager.Cast(characterMock.Object, args.rawParameters, args.parameters);

            Assert.AreEqual(CommandExecutionResults.SyntaxErrorNoDisplay, result);
        }

        [TestMethod]
        public void AbilityManager_Cast_TargetItemHereOrCharacterOffensive_InvalidTargetSpecified_Test()
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

            (string rawParameters, CommandParameter[] parameters) args = BuildParameters("Curse item2");
            CommandExecutionResults result = abilityManager.Cast(characterMock.Object, args.rawParameters, args.parameters);

            Assert.AreEqual(CommandExecutionResults.TargetNotFound, result);
        }

        [TestMethod]
        public void AbilityManager_Cast_TargetItemHereOrCharacterOffensive_ItemTargetInRoomSpecified_Test()
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

            (string rawParameters, CommandParameter[] parameters) args = BuildParameters("Curse item1");
            CommandExecutionResults result = abilityManager.Cast(characterMock.Object, args.rawParameters, args.parameters);

            Assert.AreEqual(CommandExecutionResults.Ok, result);
        }

        [TestMethod]
        public void AbilityManager_Cast_TargetItemHereOrCharacterOffensive_ItemTargetInInventorySpecified_Test()
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
            characterMock.SetupGet(x => x.Inventory).Returns(new IItem[] { itemMock.Object });
            characterMock.SetupGet(x => x.KnownAbilities).Returns(new[] { new KnownAbility { Ability = abilityManager["Curse"], Learned = 1, Level = 1 } });
            var roomMock = new Mock<IRoom>();
            roomMock.SetupGet(x => x.People).Returns(new[] { characterMock.Object });
            characterMock.SetupGet(x => x.Room).Returns(roomMock.Object);

            (string rawParameters, CommandParameter[] parameters) args = BuildParameters("Curse item1");
            CommandExecutionResults result = abilityManager.Cast(characterMock.Object, args.rawParameters, args.parameters);

            Assert.AreEqual(CommandExecutionResults.Ok, result);
        }

        [TestMethod]
        public void AbilityManager_Cast_TargetItemHereOrCharacterOffensive_ItemTargetInEquipmentSpecified_Test()
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
            characterMock.SetupGet(x => x.Equipments).Returns(new IItem[] { itemMock.Object });
            characterMock.SetupGet(x => x.KnownAbilities).Returns(new[] { new KnownAbility { Ability = abilityManager["Curse"], Learned = 1, Level = 1 } });
            var roomMock = new Mock<IRoom>();
            roomMock.SetupGet(x => x.People).Returns(new[] { characterMock.Object });
            characterMock.SetupGet(x => x.Room).Returns(roomMock.Object);

            (string rawParameters, CommandParameter[] parameters) args = BuildParameters("Curse item1");
            CommandExecutionResults result = abilityManager.Cast(characterMock.Object, args.rawParameters, args.parameters);

            Assert.AreEqual(CommandExecutionResults.Ok, result);
        }

        [TestMethod]
        public void AbilityManager_Cast_TargetItemHereOrCharacterOffensive_CharacterTargetSpecified_Test()
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
            characterMock.SetupGet(x => x.Equipments).Returns(new IItem[] { itemMock.Object });
            characterMock.SetupGet(x => x.KnownAbilities).Returns(new[] { new KnownAbility { Ability = abilityManager["Curse"], Learned = 1, Level = 1 } });
            var roomMock = new Mock<IRoom>();
            roomMock.SetupGet(x => x.People).Returns(new[] { characterMock.Object });
            characterMock.SetupGet(x => x.Room).Returns(roomMock.Object);

            (string rawParameters, CommandParameter[] parameters) args = BuildParameters("Curse mob1");
            CommandExecutionResults result = abilityManager.Cast(characterMock.Object, args.rawParameters, args.parameters);

            Assert.AreEqual(CommandExecutionResults.Ok, result);
        }

        // AbilityTargets.ItemInventoryOrCharacterDefensive
        [TestMethod]
        public void AbilityManager_Cast_TargetItemInventoryOrCharacterDefensive_NoTargetSpecified_Test()
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
            characterMock.SetupGet(x => x.Inventory).Returns(new IItem[] { itemMock.Object });
            characterMock.SetupGet(x => x.KnownAbilities).Returns(new[] { new KnownAbility { Ability = abilityManager["Bless"], Learned = 1, Level = 1 } });

            (string rawParameters, CommandParameter[] parameters) args = BuildParameters("Bless");
            CommandExecutionResults result = abilityManager.Cast(characterMock.Object, args.rawParameters, args.parameters);

            Assert.AreEqual(CommandExecutionResults.Ok, result);
        }

        [TestMethod]
        public void AbilityManager_Cast_TargetItemInventoryOrCharacterDefensive_InvalidTargetSpecified_Test()
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

            (string rawParameters, CommandParameter[] parameters) args = BuildParameters("Bless item2");
            CommandExecutionResults result = abilityManager.Cast(characterMock.Object, args.rawParameters, args.parameters);

            Assert.AreEqual(CommandExecutionResults.TargetNotFound, result);
        }

        [TestMethod]
        public void AbilityManager_Cast_TargetItemInventoryOrCharacterDefensive_ItemTargetInRoomSpecified_Test()
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

            (string rawParameters, CommandParameter[] parameters) args = BuildParameters("Bless item1");
            CommandExecutionResults result = abilityManager.Cast(characterMock.Object, args.rawParameters, args.parameters);

            Assert.AreEqual(CommandExecutionResults.TargetNotFound, result);
        }

        [TestMethod]
        public void AbilityManager_Cast_TargetItemInventoryOrCharacterDefensive_ItemTargetInInventorySpecified_Test()
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
            characterMock.SetupGet(x => x.Inventory).Returns(new IItem[] { itemMock.Object });
            characterMock.SetupGet(x => x.KnownAbilities).Returns(new[] { new KnownAbility { Ability = abilityManager["Bless"], Learned = 1, Level = 1 } });
            var roomMock = new Mock<IRoom>();
            roomMock.SetupGet(x => x.People).Returns(new[] { characterMock.Object });
            characterMock.SetupGet(x => x.Room).Returns(roomMock.Object);

            (string rawParameters, CommandParameter[] parameters) args = BuildParameters("Bless item1");
            CommandExecutionResults result = abilityManager.Cast(characterMock.Object, args.rawParameters, args.parameters);

            Assert.AreEqual(CommandExecutionResults.Ok, result);
        }

        [TestMethod]
        public void AbilityManager_Cast_TargetItemInventoryOrCharacterDefensive_ItemTargetInEquipmentSpecified_Test()
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
            characterMock.SetupGet(x => x.Equipments).Returns(new IItem[] { itemMock.Object });
            characterMock.SetupGet(x => x.KnownAbilities).Returns(new[] { new KnownAbility { Ability = abilityManager["Bless"], Learned = 1, Level = 1 } });
            var roomMock = new Mock<IRoom>();
            roomMock.SetupGet(x => x.People).Returns(new[] { characterMock.Object });
            characterMock.SetupGet(x => x.Room).Returns(roomMock.Object);

            (string rawParameters, CommandParameter[] parameters) args = BuildParameters("Bless item1");
            CommandExecutionResults result = abilityManager.Cast(characterMock.Object, args.rawParameters, args.parameters);

            Assert.AreEqual(CommandExecutionResults.TargetNotFound, result);
        }

        [TestMethod]
        public void AbilityManager_Cast_TargetItemInventoryOrCharacterDefensive_CharacterTargetSpecified_Test()
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
            characterMock.SetupGet(x => x.Equipments).Returns(new IItem[] { itemMock.Object });
            characterMock.SetupGet(x => x.KnownAbilities).Returns(new[] { new KnownAbility { Ability = abilityManager["Bless"], Learned = 1, Level = 1 } });
            var roomMock = new Mock<IRoom>();
            roomMock.SetupGet(x => x.People).Returns(new[] { characterMock.Object });
            characterMock.SetupGet(x => x.Room).Returns(roomMock.Object);

            (string rawParameters, CommandParameter[] parameters) args = BuildParameters("Bless mob1");
            CommandExecutionResults result = abilityManager.Cast(characterMock.Object, args.rawParameters, args.parameters);

            Assert.AreEqual(CommandExecutionResults.Ok, result);
        }

        // AbilityTargets.Custom
        [TestMethod]
        public void AbilityManager_Cast_TargetCustom_NoParameterSpecified_Test()
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
            characterMock.SetupGet(x => x.Inventory).Returns(new IItem[] { itemMock.Object });
            characterMock.SetupGet(x => x.KnownAbilities).Returns(new[] { new KnownAbility { Ability = abilityManager["Locate Object"], Learned = 1, Level = 1 } });

            (string rawParameters, CommandParameter[] parameters) args = BuildParameters("Locate Object");
            CommandExecutionResults result = abilityManager.Cast(characterMock.Object, args.rawParameters, args.parameters);

            Assert.AreEqual(CommandExecutionResults.Ok, result);
        }

        [TestMethod]
        public void AbilityManager_Cast_TargetCustom_ParameterSpecified_Test()
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
            characterMock.SetupGet(x => x.Inventory).Returns(new IItem[] { itemMock.Object });
            characterMock.SetupGet(x => x.KnownAbilities).Returns(new[] { new KnownAbility { Ability = abilityManager["Locate Object"], Learned = 1, Level = 1 } });

            (string rawParameters, CommandParameter[] parameters) args = BuildParameters("'locate object' item2");
            CommandExecutionResults result = abilityManager.Cast(characterMock.Object, args.rawParameters, args.parameters);

            Assert.AreEqual(CommandExecutionResults.Ok, result);
        }

        // AbilityTargets.OptionalItemInventory
        [TestMethod]
        public void AbilityManager_Cast_TargetOptionalItemInventory_NoTargetSpecified_Test()
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
            characterMock.SetupGet(x => x.Inventory).Returns(new IItem[] { itemMock.Object });
            characterMock.SetupGet(x => x.KnownAbilities).Returns(new[] { new KnownAbility { Ability = abilityManager["Continual Light"], Learned = 1, Level = 1 } });

            (string rawParameters, CommandParameter[] parameters) args = BuildParameters("'continual light'");
            CommandExecutionResults result = abilityManager.Cast(characterMock.Object, args.rawParameters, args.parameters);

            Assert.AreEqual(CommandExecutionResults.Ok, result);
        }

        [TestMethod]
        public void AbilityManager_Cast_TargetOptionalItemInventory_TargetNotFoundSpecified_Test()
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
            characterMock.SetupGet(x => x.Inventory).Returns(new IItem[] { itemMock.Object });
            characterMock.SetupGet(x => x.KnownAbilities).Returns(new[] { new KnownAbility { Ability = abilityManager["Continual Light"], Learned = 1, Level = 1 } });

            (string rawParameters, CommandParameter[] parameters) args = BuildParameters("'continual light' item2");
            CommandExecutionResults result = abilityManager.Cast(characterMock.Object, args.rawParameters, args.parameters);

            Assert.AreEqual(CommandExecutionResults.TargetNotFound, result);
        }

        [TestMethod]
        public void AbilityManager_Cast_TargetOptionalItemInventory_InvalidTargetSpecified_Test()
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
            characterMock.SetupGet(x => x.Inventory).Returns(new IItem[] { itemMock.Object });
            characterMock.SetupGet(x => x.KnownAbilities).Returns(new[] { new KnownAbility { Ability = abilityManager["Continual Light"], Learned = 1, Level = 1 } });

            (string rawParameters, CommandParameter[] parameters) args = BuildParameters("'continual light' mob1");
            CommandExecutionResults result = abilityManager.Cast(characterMock.Object, args.rawParameters, args.parameters);

            Assert.AreEqual(CommandExecutionResults.TargetNotFound, result);
        }

        [TestMethod]
        public void AbilityManager_Cast_TargetOptionalItemInventory_TargetSpecified_Test()
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
            characterMock.SetupGet(x => x.Inventory).Returns(new IItem[] { itemMock.Object });
            characterMock.SetupGet(x => x.KnownAbilities).Returns(new[] { new KnownAbility { Ability = abilityManager["Continual Light"], Learned = 1, Level = 1 } });

            (string rawParameters, CommandParameter[] parameters) args = BuildParameters("'continual light' item1");
            CommandExecutionResults result = abilityManager.Cast(characterMock.Object, args.rawParameters, args.parameters);

            Assert.AreEqual(CommandExecutionResults.Ok, result);
        }

        [TestMethod]
        public void AbilityManager_Cast_TargetOptionalItemInventory_TargetInRoomSpecified_Test()
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
            roomMock.SetupGet(x => x.Content).Returns(new IItem[] { itemMock.Object });

            (string rawParameters, CommandParameter[] parameters) args = BuildParameters("'continual light' item1");
            CommandExecutionResults result = abilityManager.Cast(characterMock.Object, args.rawParameters, args.parameters);

            Assert.AreEqual(CommandExecutionResults.TargetNotFound, result);
        }

        // AbilityTargets.ArmorInventory
        [TestMethod]
        public void AbilityManager_Cast_TargetArmorInventory_NoTargetSpecified_Test()
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

            (string rawParameters, CommandParameter[] parameters) args = BuildParameters("'enchant armor'");
            CommandExecutionResults result = abilityManager.Cast(characterMock.Object, args.rawParameters, args.parameters);

            Assert.AreEqual(CommandExecutionResults.SyntaxErrorNoDisplay, result);
        }

        [TestMethod]
        public void AbilityManager_Cast_TargetArmorInventory_TargetNotFoundSpecified_Test()
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

            (string rawParameters, CommandParameter[] parameters) args = BuildParameters("'enchant armor' item2");
            CommandExecutionResults result = abilityManager.Cast(characterMock.Object, args.rawParameters, args.parameters);

            Assert.AreEqual(CommandExecutionResults.TargetNotFound, result);
        }

        [TestMethod]
        public void AbilityManager_Cast_TargetArmorInventory_InvalidTargetSpecified_Test()
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

            (string rawParameters, CommandParameter[] parameters) args = BuildParameters("'enchant armor' mob1");
            CommandExecutionResults result = abilityManager.Cast(characterMock.Object, args.rawParameters, args.parameters);

            Assert.AreEqual(CommandExecutionResults.TargetNotFound, result);
        }

        [TestMethod]
        public void AbilityManager_Cast_TargetArmorInventory_TargetNonArmorSpecified_Test()
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
            characterMock.SetupGet(x => x.Inventory).Returns(new IItem[] { itemMock.Object });
            characterMock.SetupGet(x => x.KnownAbilities).Returns(new[] { new KnownAbility { Ability = abilityManager["Enchant Armor"], Learned = 1, Level = 1 } });

            (string rawParameters, CommandParameter[] parameters) args = BuildParameters("'enchant armor' item1");
            CommandExecutionResults result = abilityManager.Cast(characterMock.Object, args.rawParameters, args.parameters);

            Assert.AreEqual(CommandExecutionResults.InvalidTarget, result);
        }

        [TestMethod]
        public void AbilityManager_Cast_TargetArmorInventory_TargetInRoomSpecified_Test()
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

            (string rawParameters, CommandParameter[] parameters) args = BuildParameters("'enchant armor' item1");
            CommandExecutionResults result = abilityManager.Cast(characterMock.Object, args.rawParameters, args.parameters);

            Assert.AreEqual(CommandExecutionResults.TargetNotFound, result);
        }

        [TestMethod]
        public void AbilityManager_Cast_TargetArmorInventory_TargetArmorSpecified_Test()
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

            (string rawParameters, CommandParameter[] parameters) args = BuildParameters("'enchant armor' item1");
            CommandExecutionResults result = abilityManager.Cast(characterMock.Object, args.rawParameters, args.parameters);

            Assert.AreEqual(CommandExecutionResults.Ok, result);
        }

        // AbilityTargets.WeaponInventory
        [TestMethod]
        public void AbilityManager_Cast_TargetWeaponInventory_NoTargetSpecified_Test()
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

            (string rawParameters, CommandParameter[] parameters) args = BuildParameters("'enchant Weapon'");
            CommandExecutionResults result = abilityManager.Cast(characterMock.Object, args.rawParameters, args.parameters);

            Assert.AreEqual(CommandExecutionResults.SyntaxErrorNoDisplay, result);
        }

        [TestMethod]
        public void AbilityManager_Cast_TargetWeaponInventory_TargetNotFoundSpecified_Test()
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

            (string rawParameters, CommandParameter[] parameters) args = BuildParameters("'enchant Weapon' item2");
            CommandExecutionResults result = abilityManager.Cast(characterMock.Object, args.rawParameters, args.parameters);

            Assert.AreEqual(CommandExecutionResults.TargetNotFound, result);
        }

        [TestMethod]
        public void AbilityManager_Cast_TargetWeaponInventory_InvalidTargetSpecified_Test()
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

            (string rawParameters, CommandParameter[] parameters) args = BuildParameters("'enchant Weapon' mob1");
            CommandExecutionResults result = abilityManager.Cast(characterMock.Object, args.rawParameters, args.parameters);

            Assert.AreEqual(CommandExecutionResults.TargetNotFound, result);
        }

        [TestMethod]
        public void AbilityManager_Cast_TargetWeaponInventory_TargetNonWeaponSpecified_Test()
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
            characterMock.SetupGet(x => x.Inventory).Returns(new IItem[] { itemMock.Object });
            characterMock.SetupGet(x => x.KnownAbilities).Returns(new[] { new KnownAbility { Ability = abilityManager["Enchant Weapon"], Learned = 1, Level = 1 } });

            (string rawParameters, CommandParameter[] parameters) args = BuildParameters("'enchant Weapon' item1");
            CommandExecutionResults result = abilityManager.Cast(characterMock.Object, args.rawParameters, args.parameters);

            Assert.AreEqual(CommandExecutionResults.InvalidTarget, result);
        }

        [TestMethod]
        public void AbilityManager_Cast_TargetWeaponInventory_TargetInRoomSpecified_Test()
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

            (string rawParameters, CommandParameter[] parameters) args = BuildParameters("'enchant Weapon' item1");
            CommandExecutionResults result = abilityManager.Cast(characterMock.Object, args.rawParameters, args.parameters);

            Assert.AreEqual(CommandExecutionResults.TargetNotFound, result);
        }

        [TestMethod]
        public void AbilityManager_Cast_TargetWeaponInventory_TargetWeaponSpecified_Test()
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

            (string rawParameters, CommandParameter[] parameters) args = BuildParameters("'enchant Weapon' item1");
            CommandExecutionResults result = abilityManager.Cast(characterMock.Object, args.rawParameters, args.parameters);

            Assert.AreEqual(CommandExecutionResults.Ok, result);
        }

        private (string rawParameters, CommandParameter[] parameters) BuildParameters(string parameters)
        {
            var commandParameters = CommandHelpers.SplitParameters(parameters).Select(x => CommandHelpers.ParseParameter(x)).ToArray();
            return (parameters, commandParameters);
        }
    }
}
